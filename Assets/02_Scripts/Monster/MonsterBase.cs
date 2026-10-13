using System.Collections.Generic;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    private MonsterAnimatorController _monsterAnimController;

    [Header("Stats")]
    private float _maxHp = 100f;
    private float _moveSpeed = 3f;
    private float _aggroRange = 7f;
    protected float _currentHp;


    [Header("Skills")]
    public List<MonsterSkill> Skills = new List<MonsterSkill>();
    protected List<float> _skillCooldownTimers = new List<float>();

    protected Transform _playerTransform;
    protected bool _isAggro = false;

    protected virtual void Awake()
    {
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

        CheckAggro();
    }

    protected void CheckAggro()
    {
        if (_playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        _isAggro = distance <= _aggroRange;
    }

    public virtual void TakeDamage(float damage)
    {
        _monsterAnimController.ChangeState(MonsterState.Hit);
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _monsterAnimController.ChangeState(MonsterState.Dead);
        Destroy(gameObject);
    }
}
