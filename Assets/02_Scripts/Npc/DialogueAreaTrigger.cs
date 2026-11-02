using UnityEngine;

public class DialogueAreaTrigger : MonoBehaviour
{
    [SerializeField] private DialogueWorldTrigger _worldTrigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled || !other.CompareTag("Player"))
        {
            return;
        }

        _worldTrigger.Activate();
    }
}
