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
    [SerializeField] private int _attackPatternCount = 2;
    protected float _currentHp;


    [Header("Skills")]
    public List<MonsterSkill> Skills = new List<MonsterSkill>();
    protected List<float> _skillCooldownTimers = new List<float>();
    [SerializeField] public Transform _playerTransform;

    public Rigidbody2D _rigidbody2D;

    protected bool _isMovingRight = false;
    protected bool _isAggro = false;
    protected bool _isAttacking = false;
    private float _attackCooldownTimer = 0f;
    private float _attackCooldown = 2f;
    private bool _isAttackUseable = false;

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
        _monsterAnimController.ChangeState(MonsterState.Idle);
        if ( _isAttackUseable == false) return;
        if (_isAttacking) return;
        _isAttacking = true;

        _attackCooldownTimer = _attackCooldown;
        _isAttackUseable = false;

        int selectedNum = Random.Range(1, _attackPatternCount + 1);
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
        _monsterAnimController.ChangeState(MonsterState.Hit);
        _currentHp -= damage;
        Debug.LogError($"몬스터가 플레이어에게 {damage}데미지를 입었습니다.");
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _monsterAnimController.ChangeState(MonsterState.Dead);
    }

    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }

    public void OnApplyAttackDamage()
    {
        if (_playerTransform.TryGetComponent<PlayerStatus>(out var playerStatus))
        {
            playerStatus.TakeDamage(_attackDamage);
        }
    }

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
        _monsterAnimController.ChangeState(MonsterState.Idle);
    }
}
