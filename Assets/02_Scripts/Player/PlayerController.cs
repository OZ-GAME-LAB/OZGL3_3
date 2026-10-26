using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerCombatHandler))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerStatSet))]
[RequireComponent(typeof(PlayerArtifactEquipment))]
public class PlayerController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private Rigidbody2D _rigidbody2D_Player;
    [SerializeField] private PlayerInputHandler _playerInputHandler;
    [SerializeField] private PlayerCombatHandler _playerCombatHandler;
    [SerializeField] private Animator _animator_Player;
    [SerializeField] private SpriteRenderer _spriteRenderer_Player;
    [SerializeField] private Transform _transform_GroundCheck;
    [SerializeField] private LayerMask _layerMask_Ground;
    [SerializeField] private LayerMask _layerMask_Ladder;
    private PlayerStateMachine _stateMachine;
    private PlayerStateContext _context;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.2f;

    [Header("Ladder")]
    [SerializeField] private float _ladderCoolDown = 1f;

    [Header("Dash")]
    [SerializeField, Min(0.01f)] private float _dashDistance = 4f;
    [SerializeField, Min(0.01f)] private float _dashDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float _dashCoolDuration = 4f;

    [Header("InputTime")]
    [SerializeField, Range(0f, 0.4f)] private float _comboInputStart = 0.2f;
    [SerializeField, Range(0.4f, 0.8f)] private float _comboInputEnd = 0.78f;

    private float _runSpeed => PlayerStatSet.Instance.RunSpeed;
    private int _maxDashChance => PlayerStatSet.Instance.MaxDashChance;

    public int CurrentDashCharges => _dashCharge != null ? _dashCharge.Current : 0;
    public int MaxDashCharges => Mathf.Max(1, _maxDashChance);

    private bool _isSubscribedToPlayerStatus;

    private PlayerDashCharge _dashCharge;
    private DashState _dashState;

    private bool _isDashing => _dashState != null && _dashState.IsDashing;

    private bool _isGrounded
    {
        get => _context != null && _context.IsGrounded;
        set
        {
            if (_context != null)
            {
                _context.IsGrounded = value;
            }
        }
    }

    private LadderState _ladderState;
    private PlayerAttackFlow _attackFlow;

    private bool _isLadder => _ladderState != null && _ladderState.IsClimbing;

    public event Action<int, int, bool> OnDashChargesChanged;

    private void Awake()
    {
        if (_rigidbody2D_Player == null)
        {
            _rigidbody2D_Player = GetComponent<Rigidbody2D>();
        }

        if (_playerInputHandler == null)
        {
            _playerInputHandler = GetComponent<PlayerInputHandler>();
        }

        if (_animator_Player == null)
        {
            _animator_Player = GetComponent<Animator>();
        }

        if (_spriteRenderer_Player == null)
        {
            _spriteRenderer_Player = GetComponent<SpriteRenderer>();
        }

        if (_rigidbody2D_Player == null ||
            _playerInputHandler == null ||
            _animator_Player == null ||
            _spriteRenderer_Player == null ||
            _transform_GroundCheck == null)
        {
            Logger.LogError("Inspector참조를 확인하세요.");
            enabled = false;
            return;
        }

        if (_layerMask_Ground.value == 0)
        {
            _layerMask_Ground = LayerMask.GetMask("Ground");
        }

        if (_layerMask_Ladder.value == 0)
        {
            _layerMask_Ladder = LayerMask.GetMask("Ladder");
        }

        InitializeStateMachine();
    }

    private void OnEnable()
    {
        //TODO(이주헌): 시점정리 전 임시
        TrySubscribeToPlayerStatus();
    }

    private void Start()
    {
        //TODO(이주헌): 시점정리 전 임시
        TrySubscribeToPlayerStatus();
    }

    private bool IsDiedState => _stateMachine.CurrentType == PlayerState.Died;

    private void Update()
    {
        if (IsDiedState)
        {
            return;
        }

        _playerInputHandler.ReadInput();
        _attackFlow.UpdateState();
        UpdateFacingDirection();
        _attackFlow.HandleInput();
    }

    private void FixedUpdate()
    {
        if (IsDiedState)
        {
            return;
        }

        _dashCharge.Tick();
        CheckGrounded();
        TryEnterLadder();

        _stateMachine.ChangeStateSign(GetNextState());
        _stateMachine.StateFixedUpdateSign();
        UpdateAnimatorParameters();
    }

    private void OnDisable()
    {
        if (_stateMachine != null && _stateMachine.CurrentType == PlayerState.Block)
        {
            _stateMachine.ChangeStateSign(PlayerState.Idle);
        }

        //TODO(이주헌): 시점정리 전 임시
        UnsubscribeFromPlayerStatus();
    }
    //TODO(이주헌): 시점정리 전 임시
    private void TrySubscribeToPlayerStatus()
    {
        if (_isSubscribedToPlayerStatus)
        {
            return;
        }

        if (PlayerStatus.Instance == null)
        {
            Logger.LogWarning("PlayerStatus 인스턴스가 아직 준비되지 않았습니다.");
            return;
        }

        PlayerStatus.Instance.OnDied += HandleDied;
        _isSubscribedToPlayerStatus = true;

        if (PlayerStatus.Instance.IsDead)
        {
            HandleDied();
        }
    }
    private void InitializeStateMachine()
    {
        _context = new PlayerStateContext(
            _rigidbody2D_Player,
            _animator_Player,
            _spriteRenderer_Player,
            _playerInputHandler
            );

        _dashCharge = new PlayerDashCharge(_dashCoolDuration);
        _dashCharge.Changed += (current, max, wasUsed) =>
            OnDashChargesChanged?.Invoke(current, max, wasUsed);

        _dashState = new DashState(_context, _dashCharge, _dashDistance, _dashDuration);
        _ladderState = new LadderState(_context, _layerMask_Ladder, _ladderCoolDown);
        _attackFlow = new PlayerAttackFlow(
            _context, _dashState, _ladderState, _comboInputStart, _comboInputEnd);

        _stateMachine = new PlayerStateMachine();
        _stateMachine.RegisterState(PlayerState.Idle, new IdleState(_context));
        _stateMachine.RegisterState(PlayerState.Walk, new WalkState(_context));
        _stateMachine.RegisterState(PlayerState.Run, new RunState(_context));
        _stateMachine.RegisterState(PlayerState.Crouch, new CrouchState(_context));
        _stateMachine.RegisterState(PlayerState.Block, new BlockState(_context));
        _stateMachine.RegisterState(PlayerState.Jump, new JumpState(_context));
        _stateMachine.RegisterState(PlayerState.Fall, new FallState(_context));
        _stateMachine.RegisterState(PlayerState.Dash, _dashState);
        _stateMachine.RegisterState(PlayerState.Ladder, _ladderState);
        _stateMachine.RegisterState(PlayerState.ComboAttack, new ComboAttackState(_context));
        _stateMachine.RegisterState(PlayerState.JumpAttack, new JumpAttackState(_context));
        _stateMachine.RegisterState(PlayerState.Died, new DiedState(_context));

        _stateMachine.ChangeStateSign(PlayerState.Idle);
    }

    //TODO(이주헌): 시점정리 전 임시
    private void UnsubscribeFromPlayerStatus()
    {
        if (!_isSubscribedToPlayerStatus)
        {
            return;
        }

        PlayerStatus playerStatus = PlayerStatus.Instance;

        if (playerStatus != null)
        {
            playerStatus.OnDied -= HandleDied;
        }

        _isSubscribedToPlayerStatus = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _ladderState?.OnTriggerEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _ladderState?.OnTriggerExit(other);
    }

    private void UpdateFacingDirection()
    {
        if (_playerInputHandler.MoveInput < 0f)
        {
            _spriteRenderer_Player.flipX = true;
        }
        else if (_playerInputHandler.MoveInput > 0f)
        {
            _spriteRenderer_Player.flipX = false;
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(
            _transform_GroundCheck.position,
            _groundCheckRadius,
            _layerMask_Ground) != null;

        if (!wasGrounded && _isGrounded)
        {
            _context.ResetAirActions();
        }
    }

    private void TryEnterLadder()
    {
        if (_isDashing ||
            _attackFlow.IsJumpAttack ||
            _attackFlow.IsComboAttack)
        {
            return;
        }

        _ladderState.TryBegin();
    }

    private PlayerState GetNextState()
    {
        if (_isDashing)
        {
            _playerInputHandler.ConsumeDashInput();
            return PlayerState.Dash;
        }

        if (_attackFlow.IsJumpAttack)
        {
            return PlayerState.JumpAttack;
        }

        if (_attackFlow.IsComboAttack)
        {
            _playerInputHandler.ConsumeDashInput();
            return PlayerState.ComboAttack;
        }

        if (_isLadder)
        {
            _playerInputHandler.ConsumeDashInput();
            return PlayerState.Ladder;
        }

        if (_playerInputHandler.DashQueued)
        {
            _playerInputHandler.ConsumeDashInput();

            if (_dashCharge.HasCharge)
            {
                return PlayerState.Dash;
            }
        }

        if (!_isGrounded)
        {
            return GetAirState();
        }

        return GetGroundedState();
    }

    private PlayerState GetGroundedState()
    {
        if (_playerInputHandler.BlockInput)
        {
            return PlayerState.Block;
        }

        if (_playerInputHandler.CrouchInput)
        {
            return PlayerState.Crouch;
        }

        if (_playerInputHandler.JumpQueued)
        {
            return PlayerState.Jump;
        }

        if (Mathf.Abs(_playerInputHandler.MoveInput) < 0.01f)
        {
            return PlayerState.Idle;
        }

        if (_playerInputHandler.RunInput)
        {
            return PlayerState.Run;
        }
        else
        {
            return PlayerState.Walk;
        }
    }

    private PlayerState GetAirState()
    {
        if (_rigidbody2D_Player.linearVelocity.y > 0f)
        {
            return PlayerState.Jump;
        }
        else
        {
            return PlayerState.Fall;
        }
    }

    private void HandleDied()
    {
        if (_stateMachine == null || IsDiedState)
        {
            return;
        }

        _attackFlow?.ResetFlags();

        _stateMachine.ChangeStateSign(PlayerState.Died);
    }

    private void UpdateAnimatorParameters()
    {
        bool isCrouching = _stateMachine.CurrentType == PlayerState.Crouch;

        float normalizedMoveSpeed = _runSpeed > 0f
            ? Mathf.Abs(_rigidbody2D_Player.linearVelocity.x) / _runSpeed
            : 0f;

        _animator_Player.SetFloat(
            "MoveSpeed",
            normalizedMoveSpeed,
            0.08f,
            Time.fixedDeltaTime);

        _animator_Player.SetBool("IsGrounded", _context.IsGrounded);
        _animator_Player.SetBool("IsCrouching", isCrouching);
        _animator_Player.SetBool("IsLadder", _ladderState.IsClimbing);
        _animator_Player.SetFloat("LadderDirection", _ladderState.Direction);
        _animator_Player.SetFloat("VerticalSpeed", _rigidbody2D_Player.linearVelocity.y);
    }

    private void OnDrawGizmos()
    {
        if (_transform_GroundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_transform_GroundCheck.position, _groundCheckRadius);
    }
}