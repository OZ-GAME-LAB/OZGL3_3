using UnityEngine;
using UnityEngine.Events;

public class DialogueTriggerListener : MonoBehaviour
{
    [SerializeField] private string _triggerId;
    [SerializeField] private UnityEvent _onTriggered = new();

    private DialogueService _dialogue;

    private void OnEnable()
    {
        if (_dialogue != null)
        {
            _dialogue.OnTrigger += HandleTrigger;
        }
    }

    private void Start()
    {
        _dialogue = GameManager.Npc.Dialogue;
        _dialogue.OnTrigger += HandleTrigger;
    }

    private void OnDisable()
    {
        if (_dialogue != null)
        {
            _dialogue.OnTrigger -= HandleTrigger;
        }
    }

    private void HandleTrigger(DialogueTriggerData trigger)
    {
        if (trigger.TriggerId == _triggerId)
        {
            _onTriggered.Invoke();
        }
    }
}
