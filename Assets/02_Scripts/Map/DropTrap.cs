using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DropTrap : MonoBehaviour
{
    private const string STRING_PLAYER_TAG = "Player";

    [Header("Detection")]
    [SerializeField] private Collider2D _collider2D_Children_DropTrap;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask LayerMask_Ground;

    [Header("Fall")]
    [SerializeField, Min(0.01f)] private float _fallGravityScale = 1f;

    private Rigidbody2D _rigidbody2D_DropTrap;
    private Collider2D _collider2D_DropTrap;
    private bool _hasTriggered;

    private void Awake()
    {
        _rigidbody2D_DropTrap = GetComponent<Rigidbody2D>();
        _collider2D_DropTrap = GetComponent<Collider2D>();

        if (_collider2D_Children_DropTrap == null)
        {
            Logger.LogError(
                "DropTrap: Inspector에서 자식 감지용 Collider2D를 연결하세요.");
            enabled = false;
            return;
        }

        if (!_collider2D_Children_DropTrap.transform.IsChildOf(transform))
        {
            Logger.LogError(
                "DropTrap: 감지용 Collider2D는 DropTrap의 자식 오브젝트에 있어야 합니다.");
            enabled = false;
            return;
        }

        if (_collider2D_Children_DropTrap.attachedRigidbody !=
            _rigidbody2D_DropTrap)
        {
            Logger.LogError(
                "DropTrap: 자식 감지 Collider2D는 DropTrap의 Rigidbody2D를 사용해야 합니다.");
            enabled = false;
            return;
        }

        if (LayerMask_Ground.value == 0)
        {
            Logger.LogError(
                "DropTrap: Inspector에서 Ground 레이어를 지정하세요.");
            enabled = false;
            return;
        }

        // 루트 콜라이더: 실제 충돌 판정
        _collider2D_DropTrap.isTrigger = false;

        // 자식 콜라이더: 플레이어 감지 및 낙하 시작 판정
        _collider2D_Children_DropTrap.isTrigger = true;

        // 플레이어 감지 전에는 중력으로 떨어지지 않음
        _rigidbody2D_DropTrap.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D_DropTrap.gravityScale = _fallGravityScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered || !IsPlayer(other))
        {
            return;
        }

        DoFall();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 루트의 실제 콜라이더가 발생시킨 충돌만 처리
        if (collision.otherCollider != _collider2D_DropTrap)
        {
            return;
        }

        Collider2D collider2D_Other = collision.collider;

        // 플레이어와의 실제 충돌은 Ground 충돌로 처리하지 않음
        if (IsPlayer(collider2D_Other))
        {
            collider2D_Other.GetComponent<PlayerStatus>()?.TakeDamage(1);
            Destroy(gameObject);
            return;
        }

        // 낙하 시작 후 루트 콜라이더가 Ground에 닿으면 제거
        if (_hasTriggered &&
            IsGroundLayer(collider2D_Other.gameObject.layer))
        {
            Destroy(gameObject);
        }
    }

    private bool IsPlayer(Collider2D collider2D_Other)
    {
        if (collider2D_Other.CompareTag(STRING_PLAYER_TAG))
        {
            return true;
        }

        Rigidbody2D rigidbody2D_Other =
            collider2D_Other.attachedRigidbody;

        return rigidbody2D_Other != null &&
               rigidbody2D_Other.CompareTag(STRING_PLAYER_TAG);
    }

    private bool IsGroundLayer(int layer)
    {
        return (LayerMask_Ground.value & (1 << layer)) != 0;
    }

    private void DoFall()
    {
        _hasTriggered = true;
        _rigidbody2D_DropTrap.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D_DropTrap.gravityScale = _fallGravityScale;
        _rigidbody2D_DropTrap.WakeUp();
    }
}