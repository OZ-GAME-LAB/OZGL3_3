using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTestScene : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private DialogueTestStation[] _stations;
    [SerializeField] private Vector3 _overviewCenter;
    [SerializeField] private float _overviewSize;
    [SerializeField] private TMP_Text _selectionText;
    [SerializeField] private TMP_Text _conditionText;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private TMP_Text _eventText;
    [SerializeField] private TMP_Text _notificationText;
    [SerializeField] private TMP_Text _speakerText;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private UnityEngine.UI.Image _portrait;
    [SerializeField] private UnityEngine.UI.Toggle _previewToggle;

    private readonly HashSet<string> _viewedRepeats = new();
    private readonly HashSet<string> _observedTriggers = new();
    private readonly Queue<string> _eventLog = new();
    private DataTableManager _tables;
    private DataTableManager _previewTables;
    private UserDialogueData _flowData;
    private DialogueService _flowDialogue;
    private DialogueService _previewDialogue;
    private DialogueService _activeDialogue;
    private string _lastFlowGroupId;
    private string _focusedGroupId;
    private int _selectedIndex;
    private bool _cancelled;
    private Coroutine _pendingDialogue;

    private void Awake()
    {
        _tables = new DataTableManager();
        _tables.LoadAllData();
        _previewTables = new DataTableManager();
        _previewTables.LoadAllData();
        foreach (DialogueGroupData group in _previewTables.DialogueGroup.Values)
        {
            group.ConditionId = null;
            group.IsRepeatable = true;
        }
        foreach (DialogueTriggerData trigger in _previewTables.DialogueTrigger.Values)
        {
            trigger.NextDialogueGroupId = null;
        }
        ResetProgress();
    }

    private void Update()
    {
        _activeDialogue?.Tick(Time.unscaledDeltaTime);
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }
        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            Advance();
        }
        if (keyboard.leftArrowKey.wasPressedThisFrame) PreviousLocation();
        if (keyboard.rightArrowKey.wasPressedThisFrame) NextLocation();
    }

    public void Execute(DialogueTestStation station)
    {
        Focus(Array.IndexOf(_stations, station));
        if (_activeDialogue?.IsActive == true)
        {
            _notificationText.text = "현재 대화를 마치거나 중단한 뒤 실행하세요.";
            return;
        }
        if (station.Type == DialogueTestStationType.AutomaticTrigger)
        {
            _notificationText.text = "이 트리거는 앞 대화를 완료할 때 자동으로 발생합니다.";
            return;
        }
        if (station.Type == DialogueTestStationType.Dialogue)
        {
            StartDialogue(station.DialogueGroupId, _previewToggle.isOn);
            return;
        }
        if (_previewToggle.isOn || !_flowDialogue.MatchConditions(station.ConditionId))
        {
            _notificationText.text = "진행 모드에서 앞선 전투 시작 안내를 완료한 뒤 실행하세요.";
            return;
        }

        _flowDialogue.RecordEvent(station.EventId);
        _flowDialogue.RequestGroup(station.DialogueGroupId);
        Log("수동 이벤트: " + station.EventId);
        RefreshStations();
    }

    public void ExecuteSelected()
    {
        Execute(_stations[_selectedIndex]);
    }

    public void ExecuteNext()
    {
        if (_activeDialogue?.IsActive == true)
        {
            Advance();
            return;
        }
        if (_previewToggle.isOn)
        {
            ExecuteSelected();
            return;
        }
        foreach (DialogueTestStation station in _stations)
        {
            if (station.Type == DialogueTestStationType.Dialogue && !IsViewed(station)
                && _flowDialogue.MatchConditions(_tables.DialogueGroup[station.DialogueGroupId].ConditionId))
            {
                Execute(station);
                return;
            }
            if (station.Type == DialogueTestStationType.ExternalEvent && !_flowData.Events.Contains(station.EventId)
                && _flowDialogue.MatchConditions(station.ConditionId))
            {
                Execute(station);
                return;
            }
        }
        _notificationText.text = "현재 데이터의 모든 대화와 이벤트를 확인했습니다. 처음부터 다시 시작할 수 있습니다.";
    }

    public void Advance()
    {
        if (_activeDialogue?.IsActive == true) _activeDialogue.Advance();
        else ExecuteNext();
    }

    public void CompleteDialogue()
    {
        _activeDialogue?.Skip();
    }

    public void CancelDialogue()
    {
        _cancelled = true;
        _activeDialogue?.Cancel();
        _cancelled = false;
    }

    public void ResetProgress()
    {
        Unsubscribe();
        string session = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/DialogueTestScene", Guid.NewGuid().ToString("N")));
        UserDataManager flowUser = CreateUserData(Path.Combine(session, "Flow"));
        _flowData = flowUser.GetUserData<UserDialogueData>();
        _flowDialogue = new DialogueService(_tables, flowUser);
        _previewDialogue = new DialogueService(_previewTables, CreateUserData(Path.Combine(session, "Preview")));
        _flowDialogue.OnChanged += RefreshDialogue;
        _flowDialogue.OnFinished += HandleFinished;
        _flowDialogue.OnProgressed += RefreshStations;
        _flowDialogue.OnTrigger += HandleTrigger;
        _flowDialogue.OnNotification += HandleNotification;
        _flowDialogue.OnRequested += SchedulePendingDialogue;
        _previewDialogue.OnChanged += RefreshDialogue;
        _previewDialogue.OnFinished += HandleFinished;
        _activeDialogue = _flowDialogue;
        _lastFlowGroupId = null;
        _focusedGroupId = null;
        _viewedRepeats.Clear();
        _observedTriggers.Clear();
        _eventLog.Clear();
        _eventText.text = "발생한 트리거와 이벤트가 여기에 표시됩니다.";
        _notificationText.text = "각 오브젝트의 실행 버튼 또는 [다음 순서 실행]을 누르세요. Enter는 다음 대사입니다.";
        _previewToggle.SetIsOnWithoutNotify(false);
        _dialoguePanel.SetActive(false);
        Focus(0);
        RefreshStations();
    }

    public void ChangePreviewMode(bool preview)
    {
        _notificationText.text = preview
            ? "개별 보기: 선택한 대화만 조건을 무시하고 재생합니다. 실제 테스트 진행은 바뀌지 않습니다."
            : "진행 모드: JSON의 실제 조건과 트리거를 적용합니다.";
        RefreshStations();
        SchedulePendingDialogue();
    }

    public void PreviousLocation() => Focus(Math.Max(0, _selectedIndex - 1));
    public void NextLocation() => Focus(Math.Min(_stations.Length - 1, _selectedIndex + 1));

    public void ShowOverview()
    {
        _camera.transform.position = _overviewCenter;
        _camera.orthographicSize = _overviewSize;
    }

    private void StartDialogue(string groupId, bool preview)
    {
        _activeDialogue = preview ? _previewDialogue : _flowDialogue;
        _focusedGroupId = null;
        if (!_activeDialogue.TryStartGroup(groupId))
        {
            _notificationText.text = "아직 조건을 만족하지 않았거나 이미 완료한 대화입니다. 오른쪽 조건을 확인하세요.";
        }
        RefreshStations();
    }

    private void RefreshDialogue()
    {
        bool active = _activeDialogue?.IsActive == true;
        _dialoguePanel.SetActive(active);
        _previewToggle.interactable = !active;
        if (!active) return;
        DialogueData line = _activeDialogue.CurrentLine;
        _speakerText.text = (_activeDialogue == _previewDialogue ? "[개별 보기] " : "") + line.SpeakerId;
        _dialogueText.text = _activeDialogue.VisibleText;
        string groupId = _activeDialogue.CurrentGroup.Id;
        if (_activeDialogue == _flowDialogue) _lastFlowGroupId = groupId;
        if (_focusedGroupId == groupId) return;
        _focusedGroupId = groupId;
        int index = Array.FindIndex(_stations, station => station.Type == DialogueTestStationType.Dialogue && station.DialogueGroupId == groupId);
        Focus(index);
        _portrait.sprite = _stations[index].Portrait;
        _portrait.enabled = _portrait.sprite != null;
    }

    private void HandleFinished()
    {
        if (!_cancelled && _activeDialogue == _flowDialogue && _lastFlowGroupId != null
            && _tables.DialogueGroup[_lastFlowGroupId].IsRepeatable)
        {
            _viewedRepeats.Add(_lastFlowGroupId);
        }
        RefreshDialogue();
        RefreshStations();
        SchedulePendingDialogue();
    }

    private void SchedulePendingDialogue()
    {
        if (_pendingDialogue != null || _previewToggle.isOn || !_flowDialogue.HasPendingGroups
            || (_activeDialogue?.IsActive ?? false))
        {
            return;
        }
        _pendingDialogue = StartCoroutine(OpenPendingDialogue());
    }

    private IEnumerator OpenPendingDialogue()
    {
        yield return null;
        _pendingDialogue = null;
        if (_previewToggle.isOn || (_activeDialogue?.IsActive ?? false))
        {
            yield break;
        }
        string pending = _flowDialogue.TakePendingGroup();
        if (pending != null)
        {
            StartDialogue(pending, false);
        }
    }

    private void HandleTrigger(DialogueTriggerData trigger)
    {
        _observedTriggers.Add(trigger.Id);
        Log("자동 트리거: " + trigger.Id);
        RefreshStations();
    }

    private void HandleNotification(DialogueNotificationData notification)
    {
        _notificationText.text = notification.Message;
    }

    private void RefreshStations()
    {
        bool busy = _activeDialogue?.IsActive == true;
        foreach (DialogueTestStation station in _stations)
        {
            if (station.Type == DialogueTestStationType.AutomaticTrigger)
            {
                bool triggered = _observedTriggers.Contains(station.TriggerId);
                station.SetState(triggered ? "자동 발생 완료" : "앞 대화 완료 시 자동 발생", false,
                    triggered ? new Color(.45f, .9f, .6f) : new Color(.8f, .65f, 1f));
                continue;
            }
            bool viewed = IsViewed(station);
            string condition = station.Type == DialogueTestStationType.Dialogue
                ? _tables.DialogueGroup[station.DialogueGroupId].ConditionId : station.ConditionId;
            bool ready = _flowDialogue.MatchConditions(condition) && !viewed;
            bool preview = _previewToggle.isOn && station.Type == DialogueTestStationType.Dialogue;
            station.SetState(preview ? "조건 없이 개별 보기" : viewed ? "확인 완료" : ready ? "실행 가능" : "선행 조건 대기",
                !busy && (preview || (!_previewToggle.isOn && ready)),
                viewed ? new Color(.45f, .9f, .6f) : ready ? new Color(1f, .85f, .5f) : new Color(.65f, .7f, .8f));
        }
        int viewedGroups = _tables.DialogueGroup.Keys.Count(id => _flowData.CompletedDialogueGroupIds.Contains(id) || _viewedRepeats.Contains(id));
        _progressText.text = $"대화 {viewedGroups}/{_tables.DialogueGroup.Count}  |  자동 트리거 {_observedTriggers.Count}/{_tables.DialogueTrigger.Count}  |  테스트 전용 저장";
        RefreshConditionText();
    }

    private bool IsViewed(DialogueTestStation station)
    {
        return station.Type == DialogueTestStationType.ExternalEvent
            ? _flowData.Events.Contains(station.EventId)
            : _flowData.CompletedDialogueGroupIds.Contains(station.DialogueGroupId) || _viewedRepeats.Contains(station.DialogueGroupId);
    }

    private void Focus(int index)
    {
        _selectedIndex = index;
        _camera.transform.position = _stations[index].transform.position + new Vector3(2.2f, 2.2f, -10f);
        _camera.orthographicSize = 5.8f;
        for (int i = 0; i < _stations.Length; i++) _stations[i].SetSelected(i == index);
        _selectionText.text = $"{index + 1:00}/{_stations.Length}  {_stations[index].Title}";
        RefreshConditionText();
    }

    private void RefreshConditionText()
    {
        DialogueTestStation station = _stations[_selectedIndex];
        if (station.Type == DialogueTestStationType.AutomaticTrigger)
        {
            DialogueTriggerData trigger = _tables.DialogueTrigger[station.TriggerId];
            _conditionText.text = "자동 트리거\n" + trigger.Id + "\n\nFlagId: " + trigger.FlagId
                + "\nEventId: " + trigger.EventId + "\nStoryStage: " + trigger.StoryStage
                + "\nNextGroup: " + trigger.NextDialogueGroupId + "\n\n앞 NPC의 대화를 완료하면 자동으로 처리됩니다.";
            return;
        }
        string conditionId = station.Type == DialogueTestStationType.Dialogue
            ? _tables.DialogueGroup[station.DialogueGroupId].ConditionId : station.ConditionId;
        List<string> conditions = new();
        while (!string.IsNullOrEmpty(conditionId))
        {
            DialogueConditionData condition = _tables.DialogueCondition[conditionId];
            conditions.Add(condition.ConditionType + "\n" + condition.Value);
            conditionId = condition.NextConditionId;
        }
        _conditionText.text = station.Type == DialogueTestStationType.ExternalEvent
            ? "수동 이벤트\n" + station.EventId + "\n\n전투 완료를 대신 보고합니다.\n\n필요 조건\n"
            : "대화 그룹\n" + station.DialogueGroupId + "\n\n필요 조건\n";
        _conditionText.text += string.Join("\n\n", conditions);
    }

    private void Log(string message)
    {
        _eventLog.Enqueue(message);
        while (_eventLog.Count > 5) _eventLog.Dequeue();
        _eventText.text = string.Join("\n\n", _eventLog);
    }

    private static UserDataManager CreateUserData(string directory)
    {
        UserDataManager user = new();
        user.UserDataList.Add(new UserDialogueData(directory));
        user.UserDataList.Add(new UserInventoryData(directory));
        user.UserDataList.Add(new UserArtifactData(directory));
        user.UserDataList.Add(new UserShopData(directory));
        user.SetAllDefaultUserData();
        return user;
    }

    private void Unsubscribe()
    {
        if (_flowDialogue == null) return;
        if (_pendingDialogue != null)
        {
            StopCoroutine(_pendingDialogue);
            _pendingDialogue = null;
        }
        _flowDialogue.OnChanged -= RefreshDialogue;
        _flowDialogue.OnFinished -= HandleFinished;
        _flowDialogue.OnProgressed -= RefreshStations;
        _flowDialogue.OnTrigger -= HandleTrigger;
        _flowDialogue.OnNotification -= HandleNotification;
        _flowDialogue.OnRequested -= SchedulePendingDialogue;
        _previewDialogue.OnChanged -= RefreshDialogue;
        _previewDialogue.OnFinished -= HandleFinished;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
