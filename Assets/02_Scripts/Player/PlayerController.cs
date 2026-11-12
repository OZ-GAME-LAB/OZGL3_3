using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerCombatHandler))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerStatSet))]
[RequireComponent(typeof(PlayerArtifactEquipment))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : SingletonBehaviour<PlayerController>
{
    [Header("Core")]
    [SerializeField] private Rigidbody2D _rigidbody2D_Player;
    [SerializeField] private PlayerInputHandler _playerInputHandler;
    [SerializeField] private PlayerCombatHandler _playerCombatHandler;
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private Animator _animator_Player;
    [SerializeField] private SpriteRenderer _spriteRenderer_Player;
    [SerializeField] private Transform _transform_GroundCheck;
    [SerializeField] private LayerMask _layerMask_Ground;
    [SerializeField] private LayerMask _layerMask_Ladder;
    [SerializeField] private PlayerStateMachine _stateMachine;
    [SerializeField] private PlayerStateContext _playerContext;
    [SerializeField] private CapsuleCollider2D _playerCollider;


    [Header("Visual")]
    [SerializeField] private PlayerVisualOffset _visualOffset;
    [SerializeField, Min(0f)] private float _crouchCooldown = 2f;
    [SerializeField] private float _crouchColliderOffsetY = -0.3f;
    [SerializeField, Min(0.01f)] private float _crouchColliderHeight = 0.7f;



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
    public float HealSkillCooldownRemaining => Mathf.Max(0f, _healSkillCooldownEndTime - Time.time);
    public float GuardedAttackSkillCooldownRemaining => Mathf.Max(0f, _guardedAttackSkillCooldownEndTime - Time.time);

    private PlayerDashCharge _dashCharge;
    private DashState _dashState;
    private HurtState _hurtState;
    private LadderState _ladderState;
    private PlayerAttackFlow _attackFlow;
    private PlayerHealSkill _playerHealSkill;
    private PlayerGuaredeAttackSkill _playerGuardedAttackSkill;
    private float _dashCooldownDecrease;

    private bool _isCrouchActionActive;
    private bool _isHurtOverlayActive;

    private float _crouchReadyTime;
    private float _healSkillCooldownEndTime;
    private float _guardedAttackSkillCooldownEndTime;
    private float _hurtOverlayEndTime;

    private int _hurtOverlayLayerIndex = -1;
    
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
    private bool _isLadder => _ladderState != null && _ladderState.IsClimbing;
    private bool IsDiedState => _stateMachine.CurrentType == PlayerState.Died;

    public event Action<int, int, bool> OnDashChargesChanged;
    public event Action<string, float> OnSkillCooldownStarted;
    public event Action<string> OnSkillCooldownEnded;
    public event Action OnCrouchStarted;

    public void SetDashCooldownDecrease(float decreasePercent)
    {
        _dashCooldownDecrease = Mathf.Max(0f, decreasePercent);
        UpdateDashArtifactModifiers();
    }

    public void RemoveDashCooldownDecrease()
    {
        _dashCooldownDecrease = 0f;
        UpdateDashArtifactModifiers();
    }

    private void Awake()
    {
        _playerCollider = GetComponent<CapsuleCollider2D>();
        _playerStatus = GetComponent<PlayerStatus>();

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
        if (_playerStatus == null)
        {
            Logger.LogError("PlayerController: PlayerStatus를 찾을 수 없습니다.");
            return;
        }

        _playerStatus.OnDied += HandleDied;
        _playerStatus.OnDamaged += HandleDamaged;

        if (_playerStatus.IsDead)
        {
            HandleDied();
        }
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
            if (IsGroundedSkillState(_stateMachine.CurrentType) &&
                _isGrounded &&
                HealSkillCooldownRemaining <= 0f &&
                _playerHealSkill.TryUse())
            {
                StartSkillCooldown(
                    Const.HealSkillId,
                    _playerHealSkill.CooldownDuration,
                    ref _healSkillCooldownEndTime);
            }
        }

        if (_playerHealSkill.IsSkillActive)
        {
            return;
        }

        if(_playerInputHandler.ConsumeGuardedAttackSkillInput())
        {
            if (GuardedAttackSkillCooldownRemaining <= 0f &&
                _playerGuardedAttackSkill.TryUse())
            {
                StartSkillCooldown(
                    Const.GuardedAttackSkillId,
                    _playerGuardedAttackSkill.CooldownDuration,
                    ref _guardedAttackSkillCooldownEndTime);
            }
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

        PlayerState currentState = _stateMachine.CurrentType;
        PlayerState nextState = GetNextState();
        if (currentState != PlayerState.Crouch &&
            nextState == PlayerState.Crouch)
        {
            OnCrouchStarted?.Invoke();
        }

        if (currentState == PlayerState.Crouch &&
            nextState != PlayerState.Crouch &&
            _isCrouchActionActive)
        {
            _isCrouchActionActive = false;
        }

        _stateMachine.ChangeStateSign(nextState);
        _stateMachine.StateFixedUpdateSign();

        if (_playerGuardedAttackSkill.IsSkillActive)
        {
            _rigidbody2D_Player.linearVelocity = new Vector2(
                0f,
                _rigidbody2D_Player.linearVelocity.y);
        }

        UpdateAnimatorParameters();
    }

    public void AnimationHeal()
    {
        _playerHealSkill.AnimationEffect();
    }

    public void AnimationCrouchEnd()
    {
        _isCrouchActionActive = false;
        _crouchReadyTime = Time.time + _crouchCooldown;
    }

    public void AnimationHealEnd()
    {
        _playerHealSkill.AnimationEnd();
    }

    private void StartSkillCooldown(string skillId, float duration, ref float cooldownEndTime)
    {
        cooldownEndTime = Time.time + duration;
        OnSkillCooldownStarted?.Invoke(skillId, duration);
        StartCoroutine(NotifySkillCooldownEnded(skillId, duration, cooldownEndTime));
    }

    private IEnumerator NotifySkillCooldownEnded(string skillId, float duration, float expectedEndTime)
    {
        yield return new WaitForSeconds(duration);

        if (skillId == Const.HealSkillId)
        {
            if (_healSkillCooldownEndTime != expectedEndTime)
            {
                yield break;
            }

            float remaining = HealSkillCooldownRemaining;
            if (remaining > 0f)
            {
                yield return new WaitForSeconds(remaining);
            }

            if (_healSkillCooldownEndTime != expectedEndTime)
            {
                yield break;
            }

            _healSkillCooldownEndTime = 0f;
        }
        else if (skillId == Const.GuardedAttackSkillId)
        {
            if (_guardedAttackSkillCooldownEndTime != expectedEndTime)
            {
                yield break;
            }

            float remaining = GuardedAttackSkillCooldownRemaining;
            if (remaining > 0f)
            {
                yield return new WaitForSeconds(remaining);
            }

            if (_guardedAttackSkillCooldownEndTime != expectedEndTime)
            {
                yield break;
            }

            _guardedAttackSkillCooldownEndTime = 0f;
        }
        else
        {
            yield break;
        }

        OnSkillCooldownEnded?.Invoke(skillId);
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

        if (_playerStatus != null)
        {
            _playerStatus.OnDied -= HandleDied;
            _playerStatus.OnDamaged -= HandleDamaged;
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
        UpdateDashArtifactModifiers();
        _ladderState = new LadderState(_playerContext, _layerMask_Ladder, _ladderCoolDown);
        _attackFlow = new PlayerAttackFlow(
            _playerContext, _dashState, _ladderState, _comboInputStart, _comboInputEnd);

        _stateMachine = new PlayerStateMachine();
        _stateMachine.RegisterState(PlayerState.Idle, new IdleState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Walk, new WalkState(_playerContext));
        _stateMachine.RegisterState(PlayerState.Run, new RunState(_playerContext));
        _stateMachine.RegisterState(
            PlayerState.Crouch,
            new CrouchState(_playerContext, _playerCollider, _crouchColliderOffsetY, _crouchColliderHeight));
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

    private void UpdateDashArtifactModifiers()
    {
        if (_dashCharge != null)
        {
            _dashCharge.SetCoolDuration(
                _dashCoolDuration * (1f - Mathf.Clamp01(_dashCooldownDecrease)));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _ladderState?.OnTriggerEnter(other);
        _playerInputHandler?.HandlePlayerTriggerEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _ladderState?.OnTriggerExit(other);
        _playerInputHandler?.HandlePlayerTriggerExit(other);
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
        bool crouchPressed = _playerInputHandler.ConsumeCrouchInput();

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

        if (_isCrouchActionActive &&
            _stateMachine.CurrentType == PlayerState.Crouch)
        {
            return PlayerState.Crouch;
        }

        return GetGroundedState(crouchPressed);
    }

    private PlayerState GetGroundedState(bool crouchPressed)
    {
        if (_playerInputHandler.BlockInput)
        {
            return PlayerState.Block;
        }

        if (crouchPressed && Time.time >= _crouchReadyTime)
        {
            _isCrouchActionActive = true;
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

    public bool Revive()
    {
        if (_playerStatus == null || _stateMachine == null)
        {
            Logger.LogError("PlayerController: 부활에 필요한 플레이어 상태가 준비되지 않았습니다.");
            return false;
        }

        if (!_playerStatus.IsDead || !IsDiedState)
        {
            Logger.LogWarning("PlayerController: 사망 상태가 아니어서 부활할 수 없습니다.");
            return false;
        }

        _playerStatus.RestoreForRevive();
        _playerInputHandler.ClearInput();
        _attackFlow?.ResetFlags();
        _playerHealSkill?.Cancel();
        StopHurtOverlay();

        _playerContext.ResetAirActions();
        _visualOffset?.ResetOffset();
        _isGrounded = false;
        _isCrouchActionActive = false;
        _animator_Player.ResetTrigger("ComboAttack");
        _animator_Player.ResetTrigger("JumpAttack");
        _animator_Player.SetBool("IsDead", false);
        _stateMachine.ChangeStateSign(PlayerState.Idle);
        return true;
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