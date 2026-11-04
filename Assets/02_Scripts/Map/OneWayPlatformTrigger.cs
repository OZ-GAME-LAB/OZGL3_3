
using UnityEngine;

public class OneWayPlatformTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        Open,
        Close
    }

    [Header("Trigger")]
    [SerializeField] private TriggerType Enum_TriggerType;

    private OneWayPlatform _oneWayPlatform;

    private void Awake()
    {
        _oneWayPlatform = GetComponentInParent<OneWayPlatform>();
    }

    private void OnTriggerEnter2D(Collider2D Collider2D_Other)
    {
        if (Collider2D_Other.CompareTag("Player") == false)
        {
            return;
        }

        switch (Enum_TriggerType)
        {
            case TriggerType.Open:
                _oneWayPlatform.DoOpen();
                break;

            case TriggerType.Close:
                _oneWayPlatform.DoClose();
                break;
        }
    }
}
