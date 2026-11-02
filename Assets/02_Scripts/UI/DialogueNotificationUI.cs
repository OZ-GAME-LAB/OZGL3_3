using TMPro;
using UnityEngine;

public class DialogueNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField, Min(0f)] private float _duration = 3f;

    private DialogueService _dialogue;
    private float _hideTime;

    private void OnEnable()
    {
        if (_dialogue != null)
        {
            _dialogue.OnNotification += Show;
        }
    }

    private void Start()
    {
        _dialogue = GameManager.Npc.Dialogue;
        _dialogue.OnNotification += Show;
        _content.SetActive(false);
    }

    private void OnDisable()
    {
        if (_dialogue != null)
        {
            _dialogue.OnNotification -= Show;
            _content.SetActive(false);
        }
    }

    private void Show(DialogueNotificationData notification)
    {
        _messageText.text = notification.Message;
        _hideTime = Time.unscaledTime + _duration;
        _content.SetActive(true);
    }

    private void Update()
    {
        if (_content.activeSelf && Time.unscaledTime >= _hideTime)
        {
            _content.SetActive(false);
        }
    }
}
