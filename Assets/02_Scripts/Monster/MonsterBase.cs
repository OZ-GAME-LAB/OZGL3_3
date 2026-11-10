using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    private MonsterAnimatorController _monsterAnimController;

    [Header("Stats")]
    [SerializeField] private float _maxHp = 5f;
    [SerializeField] public float _moveSpeed = 3f;
    [SerializeField] private float _aggroRange = 7f;
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private int _attackPatternCount = 2;
    [SerializeField] private Vector2 _attackBoxSize = new Vector2(1f, 0.5f);    //[TODO] 어택박스 사이즈, 오프셋 데이터드리븐으로 받아오기
    [SerializeField] private Vector2 _attackBoxOffset = new Vector2(1f, 0.5f);
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] protected bool _isOnlyUseSkill = false;
    [SerializeField] protected bool _isAssassin = false;

    [SerializeField] public MonsterType _monsterType;

    protected float _currentHp { get; private set; }


    [Header("Skills")]
    public List<MonsterSkill> Skills = new List<MonsterSkill>();
    protected List<float> _skillCooldownTimers = new List<float>();
    [SerializeField] public Transform _playerTransform;

    public Rigidbody2D _rigidbody2D;

    protected bool _isMovingRight = false;
    protected bool _isAggro = false;
    protected bool _isAttacking = false;
    protected bool _isDead = false;
    public bool _isHit = false;
    public bool _isUsingStealthSkill = false;
    private float _attackCooldownTimer = 0f;
    private float _attackCooldown = 2f;
    public bool _isAttackUseable = false;
    public bool _isSkillUseable = false;
    protected bool _isUsingSkill = false;
    

    public event Action<float, float> OnHpChanged;
    public event Action OnDied;


    protected virtual void Awake()
    {
        _monsterAnimController = GetComponent<MonsterAnimatorController>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _currentHp = _maxHp;
        _isSkillUseable = false;

        for (int i = 0; i < Skills.Count; i++)
        {
            _skillCooldownTimers.Add(0f);
        }
    }

    private void OnEnable()
    {
        _isDead = false;
    }

    protected virtual void Update()
    {
        if (_isDead) return;
        for (int i = 0; i < _skillCooldownTimers.Count; i++)
        {
            if (_skillCooldownTimers[i] > 0)
                _skillCooldownTimers[i] -= Time.deltaTime;
        }

        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }

        CheckAggro();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _playerTransform == null)
        {
            _playerTransform = collision.transform;
        }
    }

    protected void CheckAggro()
    {
        if (_playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        _isAggro = distance <= _aggroRange;
    }

    protected void CheckAttackUseable()
    {
        if (_attackCooldownTimer <= 0)
        {
            _isAttackUseable = true;
        }
        else
        {
            _isAttackUseable = false;
        }
    }

    public virtual void AttackTarget()
    {
        if (_isAttacking) return;

        _monsterAnimController.ChangeState(MonsterState.Idle);
        
        _isAttacking = true;

        _attackCooldownTimer = _attackCooldown;
        _isAttackUseable = false;

        int selectedNum = UnityEngine.Random.Range(1, _attackPatternCount + 1);
        if (selectedNum == 1)
        {
            _monsterAnimController.ChangeState(MonsterState.Attack_1);
        }
        else if (selectedNum == 2)
        {
            _monsterAnimController.ChangeState(MonsterState.Attack_2);
        }

        if (_isSkillUseable) return;
        _isSkillUseable = true;
    }


    public virtual void TakeDamage(float damage)
    {
        if (_isDead || _isUsingStealthSkill) return;
        _isAttacking = false;
        _isUsingSkill = false;
        _currentHp -= damage;
        OnHpChanged?.Invoke(_currentHp, _maxHp);
        //Debug.LogError($"몬스터가 플레이어에게 {damage}데미지를 입었습니다.");
        if (_currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (_isHit || _isUsingSkill) return;
            _isHit = true;
            _monsterAnimController.ChangeState(MonsterState.Hit);
        }
    }

    public void SetUsingSkill(bool isUsing)
    {
        _isUsingSkill = isUsing;
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.simulated = false;
        }

        _monsterAnimController.ChangeState(MonsterState.Dead);
        OnDied?.Invoke();
    }

    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }

    public void OnHitAnimationEnd()
    {
        _isHit = false;
        _monsterAnimController.ChangeState(MonsterState.Idle);
    }

    public void OnApplyAttackDamage()
    {
        float direction = _isMovingRight ? 1f : -1f;
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(_attackBoxOffset.x * direction, _attackBoxOffset.y);

        Collider2D hitCollider = Physics2D.OverlapBox(boxCenter, _attackBoxSize, 0f, _playerLayer);

        if (hitCollider != null)
        {
            if (hitCollider.TryGetComponent<PlayerStatus>(out var playerStatus))
            {
                playerStatus.TakeDamage(_attackDamage, attackPosition: this.transform.position, knockbackVelocity: new Vector2(0, 0));
                Logger.LogError("플레이어가 데미지를 입었습니다.");
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
        _isUsingSkill = false;
        _monsterAnimController.ChangeState(MonsterState.Idle);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float direction = _isMovingRight ? 1f : -1f;
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(_attackBoxOffset.x * direction, _attackBoxOffset.y);

        Gizmos.DrawWireCube(boxCenter, _attackBoxSize);
    }
}
