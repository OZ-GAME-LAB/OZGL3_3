using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BossDialogueBridge : MonoBehaviour
{
    [SerializeField] private string _battleTriggerId;
    [SerializeField] private string _battleDialogueGroupId;
    [SerializeField] private string _defeatedEventId;
    [SerializeField] private string _defeatedDialogueGroupId;
    [SerializeField] private UnityEvent _onBattleStarted = new();
    [SerializeField] private UnityEvent _onDefeated = new();

    private DialogueService _dialogue;
    private UserDialogueData _data;
    private UserDataManager _userData;
    private bool _defeated;
    private bool _battleStarted;

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
        _dialogue = GameManager.Npc.Dialogue;
        _userData = GameManager.UserData;
        _data = _userData.GetUserData<UserDialogueData>();
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
        }
    }

    private void RestoreState()
    {
        _defeated = _data.Events.Contains(_defeatedEventId);
        _battleStarted = !_defeated && _data.CompletedDialogueGroupIds.Contains(_battleDialogueGroupId);
        if (_defeated)
        {
            _dialogue.RequestGroup(_defeatedDialogueGroupId);
        }
    }

    private void HandleTrigger(DialogueTriggerData trigger)
    {
        if (trigger.TriggerId == _battleTriggerId)
        {
            StartBattle();
        }
    }

    public void StartBattle()
    {
        if (_battleStarted || _defeated)
        {
            return;
        }

        _battleStarted = true;
        _onBattleStarted.Invoke();
    }

    // 보스 담당 코드 또는 UnityEvent에서 처치가 확정됐을 때 호출한다.
    public void ReportDefeated()
    {
        if (_defeated)
        {
            return;
        }

        _defeated = true;
        _battleStarted = false;
        _dialogue.RecordEvent(_defeatedEventId);
        _dialogue.RequestGroup(_defeatedDialogueGroupId);
        _onDefeated.Invoke();
    }
}
