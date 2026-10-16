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
    [SerializeField] private CircleCollider2D _playerTransformCheckCollider;    //[TODO] 플레이어가 OnTriggerExit하고나서 시간이 지나면 Destroy하기

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

    private int _pendingSkillIndex = -1;
    private bool _isUsingSkill = false;

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

        if (_isUsingSkill)
        {
            if (_monsterType == MonsterType.FlyingCreature || _monsterType == MonsterType.Flying)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (_isAttacking)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        if (_isAggro)
        {
            TraceAndAttack();
        }
        else
        {
            if (_monsterType == MonsterType.Flying || _monsterType == MonsterType.FlyingCreature)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
            }
            else
            {
                Patrol();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _playerTransform == null)
        {
            _playerTransform = collision.transform;
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
        if (_monsterType == MonsterType.FlyingCreature || _monsterType == MonsterType.Flying)
        {
            _isGroundAhead = true;
            _isGrounded = true;
            _isBlocked = false;
            return;
        }
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
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _checkGroundRadius, _groundLayer);

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
        }

        if (_monsterType == MonsterType.FlyingCreature || _monsterType == MonsterType.Flying)
        {
            Vector2 direction = (_playerTransform.position - this.transform.position).normalized;
            _rigidbody2D.linearVelocity = direction * _moveSpeed;
        }

        PreventMonsterFromDrop();

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        for (int i = 0; i < Skills.Count; i++)
        {
            if (distance <= Skills[i].Range && _skillCooldownTimers[i] <= 0f)
            {
                UpdateFacingDirection();

                _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocityY);

                _pendingSkillIndex = i;
                _isUsingSkill = true;

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
                return;
            }
        }

        if (distance <= _attackDistance)
        {
            CheckAttackUseable();
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
        if (_playerTransform == null)
        {
            Logger.LogError("[NormalMonster] PlayerTransform이 Null 입니다.");
        }

        float directionX = _playerTransform.position.x - transform.position.x;

        if (Mathf.Abs(directionX) < 0.2) return;

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
        if (_isUsingSkill && _pendingSkillIndex >= 0 && _pendingSkillIndex < Skills.Count)
        {
            Skills[_pendingSkillIndex].UseSkill(this, _playerTransform);
            _skillCooldownTimers[_pendingSkillIndex] = Skills[_pendingSkillIndex].CoolDown;

            _pendingSkillIndex = -1;
        }

        _isUsingSkill = false;

        if (_monsterType == MonsterType.Flying || _monsterType == MonsterType.FlyingCreature)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        if (_isSpawned && _playerTransform != null)
        {
            AnimController_NormalMonster.ChangeState(MonsterState.Idle);
        }
    }
}
