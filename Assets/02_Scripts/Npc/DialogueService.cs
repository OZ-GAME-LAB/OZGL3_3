using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class DialogueService
{
    private readonly DataTableManager _tables;
    private readonly UserDialogueData _dialogueData;
    private readonly UserInventoryData _inventoryData;
    private readonly UserArtifactData _artifactData;
    private readonly List<DialogueData> _lines = new();
    private DialogueGroupData _group;
    private int _lineIndex;
    private int[] _characterStarts = Array.Empty<int>();
    private float _typingProgress;
    private readonly List<string> _pendingGroupIds = new();

    public float CharactersPerSecond { get; set; } = 30f;
    public bool IsActive => _group != null;
    public DialogueGroupData CurrentGroup => _group;
    public DialogueData CurrentLine => IsActive ? _lines[_lineIndex] : null;
    public int VisibleCharacterCount { get; private set; }
    public bool IsTyping => IsActive && VisibleCharacterCount < _characterStarts.Length;
    public string VisibleText => !IsActive ? string.Empty : IsTyping
        ? CurrentLine.DialogueText.Substring(0, _characterStarts[VisibleCharacterCount])
        : CurrentLine.DialogueText ?? string.Empty;

    public event Action OnChanged;
    public event Action OnFinished;
    public event Action<DialogueNotificationData> OnNotification;
    public event Action<DialogueTriggerData> OnTrigger;
    public event Action OnProgressed;

    public DialogueService(DataTableManager tables, UserDataManager userData)
    {
        _tables = tables;
        _dialogueData = userData.GetUserData<UserDialogueData>();
        _inventoryData = userData.GetUserData<UserInventoryData>();
        _artifactData = userData.GetUserData<UserArtifactData>();
    }

    public bool TryStart(string npcId)
    {
        if (IsActive)
        {
            return false;
        }

        DialogueGroupData group = SelectGroup(npcId);

        if (group == null)
        {
            return false;
        }

        StartGroup(group);
        return true;
    }

    public bool RequestNpcDialogue(string npcId)
    {
        DialogueGroupData group = SelectGroup(npcId);
        return group != null && RequestGroup(group.DialogueGroupId);
    }

    public bool RequestGroup(string dialogueGroupId)
    {
        HashSet<string> visited = new();
        DialogueGroupData group = _tables.DialogueGroup[dialogueGroupId];
        while (!group.IsRepeatable && _dialogueData.CompletedDialogueGroupIds.Contains(group.DialogueGroupId))
        {
            if (!visited.Add(group.DialogueGroupId))
            {
                throw new InvalidOperationException($"대화 그룹 연결이 순환합니다: {group.DialogueGroupId}");
            }

            string nextGroupId = ReadLines(group)
                .Where(line => !string.IsNullOrEmpty(line.TriggerId))
                .Select(line => _tables.DialogueTrigger[line.TriggerId].NextDialogueGroupId)
                .LastOrDefault(id => !string.IsNullOrEmpty(id));
            if (string.IsNullOrEmpty(nextGroupId))
            {
                return false;
            }

            group = _tables.DialogueGroup[nextGroupId];
        }

        if (!CanStart(group) || (IsActive && _group.DialogueGroupId == group.DialogueGroupId)
            || _pendingGroupIds.Contains(group.DialogueGroupId))
        {
            return false;
        }

        _pendingGroupIds.Add(group.DialogueGroupId);
        return true;
    }

    public string TakePendingGroup()
    {
        if (IsActive)
        {
            return null;
        }

        foreach (string id in _pendingGroupIds
            .OrderByDescending(id => _tables.DialogueGroup[id].Priority)
            .ThenBy(id => id, StringComparer.Ordinal).ToArray())
        {
            _pendingGroupIds.Remove(id);
            if (CanStart(_tables.DialogueGroup[id]))
            {
                return id;
            }
        }

        return null;
    }

    public void RecordEvent(string eventId, bool value = true)
    {
        if (_dialogueData.Events.Contains(eventId) == value)
        {
            return;
        }

        _dialogueData.SetEvent(eventId, value);
        _dialogueData.SaveData();
        OnProgressed?.Invoke();
    }

    public void ApplyTrigger(string triggerId)
    {
        DialogueTriggerData trigger = _tables.DialogueTrigger[triggerId];
        if (DialogueProgression.Apply(trigger, _dialogueData))
        {
            _dialogueData.SaveData();
            OnProgressed?.Invoke();
        }

        OnTrigger?.Invoke(trigger);
        if (!string.IsNullOrEmpty(trigger.NextDialogueGroupId))
        {
            RequestGroup(trigger.NextDialogueGroupId);
        }
    }

    private DialogueGroupData SelectGroup(string npcId)
    {
        return _tables.DialogueGroup.Values
            .Where(candidate => candidate.NpcId == npcId)
            .OrderByDescending(candidate => candidate.Priority)
            .ThenBy(candidate => candidate.DialogueGroupId, StringComparer.Ordinal)
            .FirstOrDefault(CanStart);
    }

    public bool TryStartGroup(string dialogueGroupId)
    {
        if (IsActive)
        {
            return false;
        }

        DialogueGroupData group = _tables.DialogueGroup[dialogueGroupId];
        if (!CanStart(group))
        {
            return false;
        }

        StartGroup(group);
        return true;
    }

    private bool CanStart(DialogueGroupData group)
    {
        return (group.IsRepeatable || !_dialogueData.CompletedDialogueGroupIds.Contains(group.DialogueGroupId))
            && MatchConditions(group.ConditionId);
    }

    private void StartGroup(DialogueGroupData group)
    {
        List<DialogueData> lines = ReadLines(group);
        _lines.Clear();
        _lines.AddRange(lines);
        _group = group;
        _lineIndex = 0;
        BeginLine();
    }

    public void Tick(float unscaledDeltaTime)
    {
        if (!IsTyping)
        {
            return;
        }

        _typingProgress += unscaledDeltaTime * CharactersPerSecond;
        int count = Math.Min((int)_typingProgress, _characterStarts.Length);
        if (count != VisibleCharacterCount)
        {
            VisibleCharacterCount = count;
            OnChanged?.Invoke();
        }
    }

    public void Advance()
    {
        if (!IsActive)
        {
            return;
        }

        if (IsTyping)
        {
            VisibleCharacterCount = _characterStarts.Length;
            _typingProgress = VisibleCharacterCount;
            OnChanged?.Invoke();
            return;
        }

        if (_lineIndex + 1 < _lines.Count)
        {
            _lineIndex++;
            BeginLine();
            return;
        }

        Complete();
    }

    public void Skip()
    {
        if (IsActive)
        {
            Complete();
        }
    }

    public void Cancel()
    {
        if (!IsActive)
        {
            return;
        }

        _group = null;
        _lines.Clear();
        OnChanged?.Invoke();
        OnFinished?.Invoke();
    }

    private void BeginLine()
    {
        _characterStarts = StringInfo.ParseCombiningCharacters(CurrentLine.DialogueText ?? string.Empty);
        VisibleCharacterCount = 0;
        _typingProgress = 0f;
        OnChanged?.Invoke();
    }

    private List<DialogueData> ReadLines(DialogueGroupData group)
    {
        List<DialogueData> lines = new();
        HashSet<string> visited = new();
        string dialogueId = group.StartDialogueId;
        while (!string.IsNullOrEmpty(dialogueId))
        {
            if (!visited.Add(dialogueId))
            {
                throw new InvalidOperationException($"대사 연결이 순환합니다: {dialogueId}");
            }

            DialogueData line = _tables.Dialogue[dialogueId];
            if (line.DialogueGroupId != group.DialogueGroupId)
            {
                throw new InvalidOperationException($"대사가 다른 그룹을 가리킵니다: {dialogueId}");
            }

            lines.Add(line);
            dialogueId = line.NextDialogueId;
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException($"시작 대사가 없습니다: {group.DialogueGroupId}");
        }

        return lines;
    }

    public bool MatchConditions(string conditionId, bool includeNextConditions = true)
    {
        HashSet<string> visited = new();
        while (!string.IsNullOrEmpty(conditionId))
        {
            if (!visited.Add(conditionId))
            {
                throw new InvalidOperationException($"대화 조건이 순환합니다: {conditionId}");
            }

            DialogueConditionData condition = _tables.DialogueCondition[conditionId];
            if (!EvaluateCondition(condition))
            {
                return false;
            }

            conditionId = includeNextConditions ? condition.NextConditionId : null;
        }

        return true;
    }

    private bool EvaluateCondition(DialogueConditionData condition)
    {
        int actual;
        int expected;
        if (condition.ConditionType == "StoryStage")
        {
            actual = _dialogueData.StoryStage;
            expected = int.Parse(condition.Value, CultureInfo.InvariantCulture);
        }
        else
        {
            IReadOnlyCollection<string> values = condition.ConditionType switch
            {
                "Flag" => _dialogueData.Flags,
                "Event" => _dialogueData.Events,
                "CompletedDialogueGroup" => _dialogueData.CompletedDialogueGroupIds,
                _ => throw new InvalidOperationException($"알 수 없는 조건 종류: {condition.ConditionType}")
            };
            string[] parts = condition.Value.Split(new[] { ':' }, 2);
            bool value = values.Contains(parts[0]);
            actual = value ? 1 : 0;
            expected = bool.Parse(parts[1]) ? 1 : 0;
        }

        return condition.Operator switch
        {
            "Equal" => actual == expected,
            "NotEqual" => actual != expected,
            "Greater" => actual > expected,
            "GreaterOrEqual" => actual >= expected,
            "Less" => actual < expected,
            "LessOrEqual" => actual <= expected,
            _ => throw new InvalidOperationException($"알 수 없는 비교 연산자: {condition.Operator}")
        };
    }

    private void Complete()
    {
        DialogueData[] completedLines = _lines.ToArray();
        bool dialogueChanged = false;
        bool inventoryChanged = false;
        bool artifactChanged = false;
        string nextDialogueGroupId = null;
        foreach (DialogueData line in completedLines)
        {
            if (!string.IsNullOrEmpty(line.RewardItemId))
            {
                int amount = line.RewardAmount;
                if (_tables.Artifact.ContainsKey(line.RewardItemId))
                {
                    if (!_artifactData.OwnedArtifactIds.Contains(line.RewardItemId))
                    {
                        _artifactData.OwnedArtifactIds.Add(line.RewardItemId);
                        artifactChanged = true;
                    }
                }
                else
                {
                    inventoryChanged |= amount != 0
                        || !_inventoryData.Items.Any(item => item.Id == line.RewardItemId);
                    _inventoryData.AddItem(line.RewardItemId, amount);
                }
            }

            if (!string.IsNullOrEmpty(line.TriggerId))
            {
                DialogueTriggerData trigger = _tables.DialogueTrigger[line.TriggerId];
                dialogueChanged |= DialogueProgression.Apply(trigger, _dialogueData);
                if (!string.IsNullOrEmpty(trigger.NextDialogueGroupId))
                {
                    nextDialogueGroupId = trigger.NextDialogueGroupId;
                }
            }
        }

        if (!_group.IsRepeatable)
        {
            _dialogueData.CompletedDialogueGroupIds.Add(_group.DialogueGroupId);
            dialogueChanged = true;
        }

        _group = null;
        _lines.Clear();
        if (dialogueChanged)
        {
            _dialogueData.SaveData();
            OnProgressed?.Invoke();
        }

        if (inventoryChanged)
        {
            _inventoryData.SaveData();
        }

        if (artifactChanged)
        {
            _artifactData.SaveData();
        }

        foreach (DialogueData line in completedLines)
        {
            if (!string.IsNullOrEmpty(line.NotificationId))
            {
                OnNotification?.Invoke(_tables.DialogueNotification[line.NotificationId]);
            }

            if (!string.IsNullOrEmpty(line.TriggerId))
            {
                OnTrigger?.Invoke(_tables.DialogueTrigger[line.TriggerId]);
            }
        }

        if (!string.IsNullOrEmpty(nextDialogueGroupId))
        {
            TryStartGroup(nextDialogueGroupId);
        }

        if (!IsActive)
        {
            OnChanged?.Invoke();
            OnFinished?.Invoke();
        }
    }
}
