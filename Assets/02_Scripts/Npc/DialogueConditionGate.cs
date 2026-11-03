using UnityEngine;
using UnityEngine.Events;

public class DialogueConditionGate : MonoBehaviour
{
    [SerializeField] private string _conditionId;
    [SerializeField] private bool _includeNextConditions = true;
    [SerializeField] private bool _invert;
    [SerializeField] private UnityEvent<bool> _onStateChanged = new();

    private DialogueService _dialogue;
    private UserDataManager _userData;
    private bool? _lastResult;

    private void OnEnable()
    {
        if (_dialogue != null)
        {
            _lastResult = null;
            Subscribe();
            Refresh();
        }
    }

    private void Start()
    {
        _dialogue = GameManager.Npc.Dialogue;
        _userData = GameManager.UserData;
        Subscribe();
        Refresh();
    }

    private void Subscribe()
    {
        _dialogue.OnProgressed += Refresh;
        _userData.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        if (_dialogue != null)
        {
            _dialogue.OnProgressed -= Refresh;
            _userData.OnChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        bool result = _dialogue.MatchConditions(_conditionId, _includeNextConditions) != _invert;
        if (_lastResult == result)
        {
            return;
        }

        _lastResult = result;
        _onStateChanged.Invoke(result);
    }
}
