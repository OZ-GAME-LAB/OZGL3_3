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
    [SerializeField] private int _attackDamage = 0;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private int _attackPatternCount = 2;
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
    private float _attackCooldownTimer = 0f;
    private float _attackCooldown = 2f;
    public bool _isAttackUseable = false;
    protected bool _isUsingSkill = false;

    public event Action<float, float> OnHpChanged;


    protected virtual void Awake()
    {
        _monsterAnimController = GetComponent<MonsterAnimatorController>();
        //[TODO] _playerTransform 찾기 일단은 직접 등록으로
        _currentHp = _maxHp;

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
            Logger.LogError("[MonsterBase] 플레이어트렌스폼 받아오기 성공");
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

    public virtual void AttackTarget(float damage)  //[TODO] 데미지 데이터드리븐으로
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
    }


    public virtual void TakeDamage(float damage)
    {
        if (_isHit || _isDead) return;
        _isAttacking = false;
        _isUsingSkill = false;
        _isHit = true;
        _currentHp -= damage;
        OnHpChanged?.Invoke(_currentHp, _maxHp);
        Debug.LogError($"몬스터가 플레이어에게 {damage}데미지를 입었습니다.");
        if (_currentHp <= 0)
        {
            Die();
        }
        else
        {
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
        _monsterAnimController.ChangeState(MonsterState.Dead);
        Logger.LogError($"몬스터{this.name}가 죽었습니다.");
    }

    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }

    public void OnHitAnimationEnd()
    {
        _monsterAnimController.ChangeState(MonsterState.Idle);
        _isHit = false;
    }

    public void OnApplyAttackDamage()
    {
        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        if (distance > _attackRange) return;
        if (_playerTransform.TryGetComponent<PlayerStatus>(out var playerStatus))
        {
            playerStatus.TakeDamage(_attackDamage);
        }
    }

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
        _isUsingSkill = false;
        _monsterAnimController.ChangeState(MonsterState.Idle);
    }
}
