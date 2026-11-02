using UnityEngine;

public class DamagePlatform : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private const float EFFECT_COOLDOWN = 1f;

    [Header("Damage")]
    [SerializeField, Min(0)] private int _damageAmount = 1;

    [Header("Knockback")]
    [SerializeField, Min(0f)] private float _knockbackHorizontalSpeed = 4f;
    [SerializeField, Min(0f)] private float _knockbackVerticalSpeed = 6f;

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

        _nextEffectTime = Time.time + EFFECT_COOLDOWN;
        playerStatus.TakeDamage(_damageAmount);

        Rigidbody2D rigidbody2DPlayer =
            collider2DOther.attachedRigidbody;

        if (rigidbody2DPlayer == null)
        {
            return;
        }

        float knockbackDirectionX = Mathf.Sign(
            rigidbody2DPlayer.position.x - transform.position.x);

        if (Mathf.Approximately(knockbackDirectionX, 0f))
        {
            knockbackDirectionX = 1f;
        }

        rigidbody2DPlayer.linearVelocity = new Vector2(
            knockbackDirectionX * _knockbackHorizontalSpeed,
            _knockbackVerticalSpeed);
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