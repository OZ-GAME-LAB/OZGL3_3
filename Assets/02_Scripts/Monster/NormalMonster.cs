using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class NormalMonster : MonsterBase
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _checkGroundRadius = 0.2f;
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private Transform _edgeCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _attackDistance = 1f;

    [Header("Anim Settings")]
    [SerializeField] private MonsterAnimatorController AnimController_NormalMonster;
    [SerializeField] private MonsterType _monsterType;

    private Rigidbody2D _rigidbody2D;
    private bool _isMovingRight = false;
    private bool _isGrounded = false;
    private bool _isBlocked = false;
    private bool _isGroundAhead = false;
    private float _checkTimer = 0.1f;
    private float _checkInterval = 0.1f;
    private bool _isSpawned = false;

    protected override void Awake()
    {
        base.Awake();
        _isSpawned = true;
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _checkTimer = Random.Range(0f, _checkInterval);
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
        PreventMonsterFromDrop();

        float direction = _isMovingRight ? 1f : -1f;
        _rigidbody2D.linearVelocity = new Vector2(direction * _moveSpeed, _rigidbody2D.linearVelocityY);
    }

    private void PreventMonsterFromDrop()
    {
        if (_groundCheck == null)
        {
            Logger.LogError("[NormalMonster] _groundCheck가 Null입니다.");
            return;
        }
        if (_edgeCheck == null)
        {
            Logger.LogError("[NormalMonster] _edgeCheck가 Null입니다.");
            return;
        }
        if (_wallCheck == null)
        {
            Logger.LogError("[NormalMonster] _wallCheck가 Null입니다.");
            return;
        }
        _isGroundAhead = Physics2D.OverlapCircle(_edgeCheck.position, _checkGroundRadius, _groundLayer);
        _isBlocked = Physics2D.Raycast(_wallCheck.position, _isMovingRight ? Vector2.right : Vector2.left, 0.5f, _groundLayer);

        if (_monsterType == MonsterType.Flying)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _checkGroundRadius, _groundLayer);
        }

        if (_isAggro)
        {
            return;
        }

        _checkTimer -= Time.deltaTime;
        if (_checkTimer <= 0f)
        {
            _checkTimer = _checkInterval;


            if (!_isGrounded || _isBlocked || !_isGroundAhead)
            {
                _isMovingRight = !_isMovingRight;
                Flip();
            }
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void TraceAndAttack()
    {
        if (_playerTransform == null)
        {
            Logger.LogError("[NormalMonster] _playerTransform이 Null입니다.");
            return;
        }
        if (Skills == null)
        {
            Logger.LogError("[NormalMonster] Skills가 Null입니다.");
            return;
        }
        PreventMonsterFromDrop();

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        for (int i = 0; i < Skills.Count; i++)
        {
            if (distance <= Skills[i].Range && _skillCooldownTimers[i] <= 0f)
            {
                UpdateFacingDirection();
                switch (i)
                {
                    case 0:
                        {
                            AnimController_NormalMonster.ChangeState(MonsterState.Skill_1);
                        }
                        break;
                    case 1:
                        {
                            AnimController_NormalMonster.ChangeState(MonsterState.Skill_2);
                        }
                        break;
                    case 2:
                        {
                            AnimController_NormalMonster.ChangeState(MonsterState.Skill_3);
                        }
                        break;
                    case 3:
                        {
                            AnimController_NormalMonster.ChangeState(MonsterState.Skill_4);
                        }
                        break;
                    default:
                        {
                            AnimController_NormalMonster.ChangeState(MonsterState.Idle);
                        }
                        break;
                }
                Skills[i].UseSkill(this, _playerTransform);
                _skillCooldownTimers[i] = Skills[i].CoolDown;
                return;
            }
        }

        if (distance <= _attackDistance)
        {
            AttackTarget(10);   // [TODO] 데미지 데이터드리븐으로 처리
        }

        if (!_isGrounded || _isBlocked || !_isGroundAhead)
        {
            UpdateFacingDirection();
            _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocityY);
        }

        else
        {
            UpdateFacingDirection();

            float direction = _isMovingRight ? 1f : -1f;
            _rigidbody2D.linearVelocity = new Vector2(direction * _moveSpeed, _rigidbody2D.linearVelocityY);
        }
    }

    private void UpdateFacingDirection()
    {
        float directionX = _playerTransform.position.x - transform.position.x;
        bool isShouldMoveRight = directionX > 0f;

        if (_isMovingRight != isShouldMoveRight)
        {
            _isMovingRight = isShouldMoveRight;
            Flip();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _checkGroundRadius);
        }
    }

    public void OnAnimationEnd()
    {

        if (_isSpawned && _playerTransform != null)
        {
            AnimController_NormalMonster.ChangeState(MonsterState.Idle);
        }
    }
}
