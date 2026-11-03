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
    private PlayerStateContext _playerContext;

    [Header("Visual")]
    [SerializeField] private PlayerVisualOffset _visualOffset;
    [SerializeField] private float _crouchVisualOffsetY = -0.37f;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.2f;

    [Header("Ladder")]
    [SerializeField] private float _ladderCoolDown = 1f;

    [Header("Dash")]
    [SerializeField, Min(0.01f)] private float _dashDistance = 4f;
    [SerializeField, Min(0.01f)] private float _dashDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float _dashCoolDuration = 4f;

    [Header("Hurt")]
    [SerializeField, Min(0f)] private float _hurtKnockbackX = 4f;
    [SerializeField, Min(0f)] private float _hurtKnockbackY = 4f;
    [SerializeField, Min(0.01f)] private float _hurtMaxDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float _hurtOverlayDuration = 0.4f;

    [Header("InputTime")]
    [SerializeField, Range(0f, 0.4f)] private float _comboInputStart = 0.2f;
    [SerializeField, Range(0.4f, 0.8f)] private float _comboInputEnd = 0.78f;

    private float _runSpeed => PlayerStatSet.Instance.RunSpeed;
    private int _maxDashChance => PlayerStatSet.Instance.MaxDashChance;

    public int CurrentDashCharges => _dashCharge != null ? _dashCharge.CurrentDashCharge : 0;
    public int MaxDashCharges => Mathf.Max(1, _maxDashChance);

    private bool _isSubscribedToPlayerStatus;

    private PlayerDashCharge _dashCharge;
    private DashState _dashState;
    private HurtState _hurtState;
    private int _hurtOverlayLayerIndex = -1;
    private float _hurtOverlayEndTime;
    private bool _isHurtOverlayActive;

    private bool _isDashing => _dashState != null && _dashState.IsDashing;

    private bool _isGrounded
    {
        get => _playerContext != null && _playerContext.IsGrounded;
        set
        {
            if (_playerContext != null)
            {
                _playerContext.IsGrounded = value;
            }
        }
    }

    public bool IsDoubleJumpUnlocked =>
        _playerContext != null && _playerContext.IsDoubleJumpUnlocked;

    private LadderState _ladderState;
    private PlayerAttackFlow _attackFlow;
    private PlayerHealSkill _playerHealSkill;
    private PlayerGuaredeAttackSkill _playerGuardedAttackSkill;

    private bool _isLadder => _ladderState != null && _ladderState.IsClimbing;
    private bool IsDiedState => _stateMachine.CurrentType == PlayerState.Died;

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
            _animator_Player = GetComponentInChildren<Animator>();
        }

        if (_spriteRenderer_Player == null)
        {
            _spriteRenderer_Player = GetComponentInChildren<SpriteRenderer>();
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

        _hurtOverlayLayerIndex = _animator_Player.GetLayerIndex("Hurt Overlay");
        if (_hurtOverlayLayerIndex < 0)
        {
            Logger.LogError("Animator에 Hurt Overlay 레이어가 없습니다.");
        }
        else
        {
            _animator_Player.SetLayerWeight(_hurtOverlayLayerIndex, 0f);
        }

        _playerHealSkill = new PlayerHealSkill(
            GetComponent<PlayerStatus>(),
            _animator_Player);

        _playerGuardedAttackSkill = new PlayerGuaredeAttackSkill(
            GetComponent<PlayerStatus>(),
            _animator_Player,
            _playerCombatHandler);

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

    private void Update()
    {
        UpdateHurtOverlay();

        if (IsDiedState)
        {
            return;
        }

        if (_stateMachine.CurrentType == PlayerState.Hurt)
        {
            _playerInputHandler.ClearInput();
            return;
        }

        if (_playerHealSkill.IsSkillActive || _stateMachine.CurrentType == PlayerState.Heal)
        {
            _playerInputHandler.ClearInput();
            return;
        }

        _playerInputHandler.ReadInput();
        _attackFlow.UpdateState();
        if (_playerInputHandler.ConsumeHealSkillInput())
        {
            if (IsGroundedSkillState(_stateMachine.CurrentType) && _isGrounded)
            {
                _playerHealSkill.TryUse();
            }
        }

        if (_playerHealSkill.IsSkillActive)
        {
            return;
        }

        if(_playerInputHandler.ConsumeGuardedAttackSkillInput())
        {
            _playerGuardedAttackSkill.TryUse();
        }

        if(_playerGuardedAttackSkill.IsSkillActive)
        {
            return;
        }

        UpdateFacingDirection();
        _attackFlow.HandleInput();
    }

    private void FixedUpdate()
    {
        if (IsDiedState)
        {
            return;
        }

        _dashCharge.UpdateDashCharge();
        CheckGrounded();
        TryEnterLadder();

        _stateMachine.ChangeStateSign(GetNextState());
        _stateMachine.StateFixedUpdateSign();
        UpdateAnimatorParameters();
    }

    public void AnimationHeal()
    {
        _playerHealSkill.AnimationEffect();
    }

    public void AnimationHealEnd()
    {
        _playerHealSkill.AnimationEnd();
    }

    public void AnimationGuardedAttackHit()
    {
        _playerGuardedAttackSkill.AnimationSkillEffect();
    }

    public void AnimationGuardedAttackHitEnd()
    {
        _playerGuardedAttackSkill.AnimationEnd();
    }

    private void OnDisable()
    {
        _playerHealSkill?.Cancel();
        StopHurtOverlay();

        if (_stateMachine != null &&
            (_stateMachine.CurrentType == PlayerState.Block ||
             _stateMachine.CurrentType == PlayerState.Heal ||
             _stateMachine.CurrentType == PlayerState.Hurt))
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
        PlayerStatus.Instance.OnDamaged += HandleDamaged;
        _isSubscribedToPlayerStatus = true;

        if (PlayerStatus.Instance.IsDead)
        {
            HandleDied();
        }
    }
    private void InitializeStateMachine()
    {
        _playerContext = new PlayerStateContext(
            _rigidbody2D_Player,
            _animator_Player,
            _spriteRenderer_Player,
            _playerInputHandler,
            _visualOffset
            );

        _dashCharge = new PlayerDashCharge(_dashCoolDuration);
        _dashCharge.DashChanceChanged += (current, max, wasUsed) =>
            OnDashChargesChanged?.Invoke(current, max, wasUsed);

        _dashState = new DashState(_playerContext, _dashCharge, _dashDistance, _dashDuration);
        _ladderState = new LadderState(_playerContext, _layerMask_Ladder, _ladderCoolDown);
        _attackFlow = new PlayerAttackFlow(
            _playerContext, _dashState, _ladderState, _comboInputStart, _comboInputEnd);

        _stateMachine = new PlayerStateMachine();
        _stateMachine.RegisterState(PlayerState.Idle, new IdleState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Walk, new WalkState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Run, new RunState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Crouch, new CrouchState(_playerContext, _crouchVisualOffsetY));
        _stateMachine.RegisterState(PlayerState.Block, new BlockState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Jump, new JumpState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Fall, new FallState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Dash, _dashState);
        _stateMachine.RegisterState(PlayerState.Ladder, _ladderState);
        _stateMachine.RegisterState(PlayerState.ComboAttack, new ComboAttackState(_playerContext));
        _stateMachine.RegisterState(PlayerState.JumpAttack, new JumpAttackState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Heal, new HealState(_playerContext));
        _hurtState = new HurtState(
            _playerContext,
            _hurtKnockbackX,
            _hurtKnockbackY,
            _hurtMaxDuration);
        _stateMachine.RegisterState(PlayerState.Hurt, _hurtState);
        _stateMachine.RegisterState(PlayerState.Died, new DiedState(_playerContext));

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
            playerStatus.OnDamaged -= HandleDamaged;
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
            _playerContext.ResetAirActions();
        }
    }

    private void TryEnterLadder()
    {
        if (_playerHealSkill.IsSkillActive ||
            _isDashing ||
            _stateMachine.CurrentType == PlayerState.Hurt ||
            _attackFlow.IsJumpAttack ||
            _attackFlow.IsComboAttack)
        {
            return;
        }

        _ladderState.TryBegin();
    }

    private PlayerState GetNextState()
    {
        if (_stateMachine.CurrentType == PlayerState.Hurt &&
            !_hurtState.IsFinished)
        {
            return PlayerState.Hurt;
        }

        if (_playerHealSkill.IsSkillActive)
        {
            return PlayerState.Heal;
        }

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

            if (_dashCharge.HasDashCharge)
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
        _playerHealSkill?.Cancel();
        StopHurtOverlay();

        _stateMachine.ChangeStateSign(PlayerState.Died);
    }

    private void HandleDamaged(
        Vector2? damageSourcePosition,
        Vector2? knockbackVelocity)
    {
        if (_stateMachine == null || IsDiedState)
        {
            return;
        }

        if (_isDashing || _playerGuardedAttackSkill.IsSkillActive)
        {
            return;
        }

        if (_playerHealSkill.IsSkillActive ||
            _stateMachine.CurrentType == PlayerState.Heal)
        {
            PlayHurtOverlay();
            return;
        }

        if (knockbackVelocity.HasValue)
        {
            _hurtState.SetKnockbackVelocity(knockbackVelocity.Value);
        }
        else
        {
            float knockbackDirection = -_playerContext.Facing;
            if (damageSourcePosition.HasValue)
            {
                float sourceDeltaX =
                    _rigidbody2D_Player.position.x - damageSourcePosition.Value.x;

                if (!Mathf.Approximately(sourceDeltaX, 0f))
                {
                    knockbackDirection = Mathf.Sign(sourceDeltaX);
                }
            }

            _hurtState.SetKnockbackDirection(knockbackDirection);
        }

        if (_stateMachine.CurrentType == PlayerState.Hurt)
        {
            _hurtState.ApplyKnockback();
            return;
        }

        _attackFlow.ResetFlags();
        _animator_Player.ResetTrigger("ComboAttack");
        _animator_Player.ResetTrigger("JumpAttack");
        _stateMachine.ChangeStateSign(PlayerState.Hurt);
    }

    private void PlayHurtOverlay()
    {
        if (_hurtOverlayLayerIndex < 0)
        {
            return;
        }

        _isHurtOverlayActive = true;
        _hurtOverlayEndTime =
            Time.time + _hurtOverlayDuration;
        _animator_Player.SetLayerWeight(_hurtOverlayLayerIndex, 1f);
        _animator_Player.Play("PlayerAnim_HurtOverlay", _hurtOverlayLayerIndex, 0f);
    }

    private void UpdateHurtOverlay()
    {
        if (_isHurtOverlayActive &&
            Time.time >= _hurtOverlayEndTime)
        {
            StopHurtOverlay();
        }
    }

    private void StopHurtOverlay()
    {
        _isHurtOverlayActive = false;
        if (_hurtOverlayLayerIndex >= 0 && _animator_Player != null)
        {
            _animator_Player.SetLayerWeight(_hurtOverlayLayerIndex, 0f);
        }
    }

    private static bool IsGroundedSkillState(PlayerState state)
    {
        return state == PlayerState.Idle ||
               state == PlayerState.Walk ||
               state == PlayerState.Run ||
               state == PlayerState.Crouch;
    }

    public void SetDoubleJumpUnlocked(bool unlocked)
    {
        if (_playerContext == null)
        {
            Logger.LogError("PlayerStateContext가 null입니다.");
            return;
        }

        _playerContext.IsDoubleJumpUnlocked = unlocked;
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

        _animator_Player.SetBool("IsGrounded", _playerContext.IsGrounded);
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