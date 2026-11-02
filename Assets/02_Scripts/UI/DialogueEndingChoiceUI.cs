using System.Linq;
using UnityEngine;

public class DialogueEndingChoiceUI : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private string _choiceTriggerId;
    [SerializeField] private string _promptDialogueGroupId;
    [SerializeField] private string _healEventId;
    [SerializeField] private string _healDialogueGroupId;
    [SerializeField] private string _sealEventId;
    [SerializeField] private string _sealDialogueGroupId;

    private NpcManager _npc;
    private DialogueService _dialogue;
    private UserDataManager _userData;
    private UserDialogueData _data;

    private void OnEnable()
    {
        if (_dialogue != null)
        {
            Subscribe();
            RestoreState();
        }
    }

    private void Start()
    {
        _npc = GameManager.Npc;
        _dialogue = _npc.Dialogue;
        _userData = GameManager.UserData;
        _data = _userData.GetUserData<UserDialogueData>();
        _content.SetActive(false);
        Subscribe();
        RestoreState();
    }

    private void Subscribe()
    {
        _dialogue.OnTrigger += HandleTrigger;
        _userData.OnChanged += RestoreState;
    }

    private void OnDisable()
    {
        if (_dialogue != null)
        {
            _dialogue.OnTrigger -= HandleTrigger;
            _userData.OnChanged -= RestoreState;
            _npc.SetEndingChoiceOpen(false);
            _content.SetActive(false);
        }
    }

    private void RestoreState()
    {
        string conditionId = _npc.Tables.DialogueGroup[_promptDialogueGroupId].ConditionId;
        if (!_data.CompletedDialogueGroupIds.Contains(_promptDialogueGroupId) || !_dialogue.MatchConditions(conditionId))
        {
            _content.SetActive(false);
            _npc.SetEndingChoiceOpen(false);
            return;
        }

        if (_data.Events.Contains(_healEventId))
        {
            _content.SetActive(false);
            _npc.SetEndingChoiceOpen(false);
            _dialogue.RequestGroup(_healDialogueGroupId);
        }
        else if (_data.Events.Contains(_sealEventId))
        {
            _content.SetActive(false);
            _npc.SetEndingChoiceOpen(false);
            _dialogue.RequestGroup(_sealDialogueGroupId);
        }
        else
        {
            ShowChoices();
        }
    }

    private void HandleTrigger(DialogueTriggerData trigger)
    {
        if (trigger.TriggerId == _choiceTriggerId)
        {
            ShowChoices();
        }
    }

    private void ShowChoices()
    {
        _npc.SetEndingChoiceOpen(true);
        _content.SetActive(true);
    }

    public void ChooseHeal()
    {
        Choose(_healEventId, _sealEventId, _healDialogueGroupId);
    }

    public void ChooseSeal()
    {
        Choose(_sealEventId, _healEventId, _sealDialogueGroupId);
    }

    private void Choose(string selectedEventId, string otherEventId, string dialogueGroupId)
    {
        if (!_npc.IsChoosingEnding)
        {
            return;
        }

        _content.SetActive(false);
        _npc.SetEndingChoiceOpen(false);
        _dialogue.RecordEvent(otherEventId, false);
        _dialogue.RecordEvent(selectedEventId);
        _dialogue.RequestGroup(dialogueGroupId);
    }
}
