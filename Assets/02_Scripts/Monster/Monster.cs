using Cysharp.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;

public class Monster : MonsterBase
{
    [Header("AnimatorController")]
    [SerializeField] private MonsterAnimatorController AnimController_Monster;

    [Header("Setting")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _spawnRange = 10f;
    [SerializeField] private float _agroRange = 8f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private Transform Transform_PlayerTransform;

    private Rigidbody2D _rigidbody2D;
    private MonsterState _currentState = MonsterState.None;
    private bool _isLookRight = true;
    private bool _isSpawned = false;

    private bool _isAttacking = false;
    private int _hp = 100;

    private int _entityInstancId;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        ChangeState(MonsterState.None);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }

        if (_isSpawned && Transform_PlayerTransform != null)
        {
            if (_isAttacking == false && _currentState != MonsterState.Hit && _currentState != MonsterState.Dead)
            {
                MoveToPlayer();
            }
        }
    }

    public void Spawn()
    {
        if (_isSpawned == true) return;
        _isSpawned = true;
        this.gameObject.SetActive(true);
        ChangeState(MonsterState.Spawn);
    }

    public void Attack()
    {
        if (_isAttacking) return;
        _isAttacking = true;

        Stop();
        ChangeState(MonsterState.Attack_1);
    }

    public void Idle()
    {
        ChangeState(MonsterState.Idle);
    }

    private void Stop()
    {
        _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocityY);
    }

    private void MoveToPlayer()
    {
        float distanceToPlayer = Vector2.Distance(this.transform.position, Transform_PlayerTransform.position);

        if (distanceToPlayer <= _agroRange)
        {
            ChangeState(MonsterState.Forward);

            if (Transform_PlayerTransform == null) return;

            float direction = Transform_PlayerTransform.position.x > transform.position.x ? 1f : -1f;

            if ((direction > 0 && _isLookRight == false) || (direction < 0 && _isLookRight == true))
            {
                Flip();
            }

            // 올바른 속도 계산식
            _rigidbody2D.linearVelocity = new Vector2(direction * _moveSpeed, _rigidbody2D.linearVelocityY);
        }
        else
        {
            Stop();
            ChangeState(MonsterState.Idle);
        }
    }

    private void Flip()
    {
        _isLookRight = !_isLookRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void TakeDamage(int damage)
    {
        ChangeState(MonsterState.Hit);
        _hp -= damage;
        if (_hp <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        ChangeState(MonsterState.Dead);
        this.gameObject.SetActive(false);
    }

    public void InitEnemyInfo(int instanceId)
    {
        _entityInstancId = instanceId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isSpawned == false) return;

        if (_currentState != MonsterState.Idle) return;

        if (collision.CompareTag("Player"))
        {
            Transform_PlayerTransform = collision.transform;
            ChangeState(MonsterState.Forward);
        }
    }

    private void ChangeState(MonsterState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        AnimController_Monster.ChangeState(newState);
    }

    public void OnAnimationEnd()
    {
        _isAttacking = false;

        if (_isSpawned && Transform_PlayerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(this.transform.position, Transform_PlayerTransform.position);
            if (distanceToPlayer <= _attackRange)
            {
                Attack();
                return;
            }

            if (distanceToPlayer <= _agroRange)
            {
                ChangeState(MonsterState.Forward);
                return;
            }
        }
        ChangeState(MonsterState.Idle);
    }
}