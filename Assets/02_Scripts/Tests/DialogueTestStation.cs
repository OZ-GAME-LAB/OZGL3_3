using TMPro;
using UnityEngine;

public enum DialogueTestStationType
{
    Dialogue,
    ExternalEvent,
    AutomaticTrigger
}

public class DialogueTestStation : MonoBehaviour
{
    [SerializeField] private DialogueTestScene _testScene;
    [SerializeField] private DialogueTestStationType _type;
    [SerializeField] private string _title;
    [SerializeField] private string _dialogueGroupId;
    [SerializeField] private string _conditionId;
    [SerializeField] private string _eventId;
    [SerializeField] private string _triggerId;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private UnityEngine.UI.Button _executeButton;
    [SerializeField] private GameObject _selection;

    public DialogueTestStationType Type => _type;
    public string Title => _title;
    public string DialogueGroupId => _dialogueGroupId;
    public string ConditionId => _conditionId;
    public string EventId => _eventId;
    public string TriggerId => _triggerId;
    public Sprite Portrait => _portrait;

    public void Execute()
    {
        _testScene.Execute(this);
    }

    public void SetState(string text, bool interactable, Color color)
    {
        _stateText.text = text;
        _stateText.color = color;
        _executeButton.interactable = interactable;
    }

    public void SetSelected(bool selected)
    {
        _selection.SetActive(selected);
    }
}
