
using UnityEngine;

public class DamagePlatform : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    [Header("Damage")]
    [SerializeField, Min(0)] private int _damageAmount = 1;
    [SerializeField, Min(0f)] private float _effectCooldown = 1f;

    private float _nextEffectTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        DoApplyDamageAndKnockback(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DoApplyDamageAndKnockback(other);
    }

    private void DoApplyDamageAndKnockback(Collider2D collider2DOther)
    {
        if (!IsPlayerCollider(collider2DOther) ||
            Time.time < _nextEffectTime)
        {
            return;
        }

        PlayerStatus playerStatus =
            collider2DOther.GetComponentInParent<PlayerStatus>();

        if (playerStatus == null)
        {
            playerStatus = PlayerStatus.Instance;
        }

        if (playerStatus == null || playerStatus.IsDead)
        {
            return;
        }

        _nextEffectTime = Time.time + _effectCooldown;

        playerStatus.TakeDamage(_damageAmount);
    }

    private bool IsPlayerCollider(Collider2D collider2DOther)
    {
        if (collider2DOther == null)
        {
            return false;
        }

        if (collider2DOther.CompareTag(PLAYER_TAG))
        {
            return true;
        }

        Rigidbody2D rigidbody2DPlayer =
            collider2DOther.attachedRigidbody;

        return rigidbody2DPlayer != null &&
               rigidbody2DPlayer.CompareTag(PLAYER_TAG);
    }
}
