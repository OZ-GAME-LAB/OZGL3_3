using UnityEngine;

public class DialogueWorldTrigger : MonoBehaviour
{
    [SerializeField] private string _conditionId;
    [SerializeField] private string _triggerId;
    [SerializeField] private string _eventId;
    [SerializeField] private string _npcId;
    [SerializeField] private string _dialogueGroupId;
    [SerializeField] private bool _activateOnStart;

    private void Start()
    {
        if (_activateOnStart)
        {
            Activate();
        }
    }

    public void Activate()
    {
        DialogueService dialogue = GameManager.Npc.Dialogue;
        if (!dialogue.MatchConditions(_conditionId))
        {
            return;
        }

        if (!string.IsNullOrEmpty(_triggerId))
        {
            dialogue.ApplyTrigger(_triggerId);
        }

        if (!string.IsNullOrEmpty(_eventId))
        {
            dialogue.RecordEvent(_eventId);
        }

        if (!string.IsNullOrEmpty(_dialogueGroupId))
        {
            dialogue.RequestGroup(_dialogueGroupId);
        }
        else if (!string.IsNullOrEmpty(_npcId))
        {
            dialogue.RequestNpcDialogue(_npcId);
        }
    }
}
