using UnityEngine;

public class NormalMonster : MonsterBase
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _checkGroundRadius = 0.2f;
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody2D _rigidbody2D;
    private bool _isMovingRight = false;
    private bool _isGrounded = false;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();

        if (_isAggro)
        {
            TraceAndAttack();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _checkGroundRadius, _groundLayer);
        bool isBlocked = Physics2D.Raycast(_wallCheck.position, _isMovingRight ? Vector2.right : Vector2.left, 0.5f, _groundLayer);

        if (!_isGrounded || isBlocked)
        {
            _isMovingRight = !_isMovingRight;
            Flip();
        }

        float direction = _isMovingRight ? 1f : -1f;
        _rigidbody2D.linearVelocity = new Vector2(direction * _moveSpeed, _rigidbody2D.linearVelocityY);
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void TraceAndAttack()
    {
        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        for (int i = 0; i < Skills.Count; i++)
        {
            if (distance <= Skills[i].Range && _skillCooldownTimers[i] <= 0f)
            {
                Skills[i].UseSkill(this, _playerTransform);
                _skillCooldownTimers[i] = Skills[i].CoolDown;
                return;
            }
        }

        Vector2 direction = (_playerTransform.position - transform.position).normalized;
        transform.Translate(new Vector2(direction.x, 0) * _moveSpeed * Time.deltaTime, Space.World);
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _checkGroundRadius);
        }
    }
}
