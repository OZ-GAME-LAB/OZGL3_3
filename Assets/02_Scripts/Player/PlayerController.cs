using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerCombatHandler))]
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

    private float _jumpValue => PlayerStats.Instance.JumpValue;
    private float _walkSpeed => PlayerStats.Instance.WalkSpeed;
    private float _runSpeed => PlayerStats.Instance.RunSpeed;
    private float _crouchSpeed => PlayerStats.Instance.CrouchSpeed;
    private float _ladderSpeed => PlayerStats.Instance.LadderSpeed;
    private int _maxDashChance => PlayerStats.Instance.MaxDashChance;

    public int CurrentDashCharges => _currentDashCharges;
    public int MaxDashCharges => Mathf.Max(1, _maxDashChance);

    private HashSet<Collider2D> _ladderTriggers;
    private Collider2D _currentLadderCollider;

    private bool _isGrounded;
    private bool _isLadder;
    private bool _isDashing;
    private bool _isNearLadeer;
    private bool _attackRequested;
    private bool _isJumpAttack;
    private bool _isBlocking;
    private bool _jumpAttackAnimationEntered;
    private bool _isSubscribedToPlayerStatus;
    private bool _dashChargesInitialized;

    private float _originalGravityScale;
    private float _ladderDirection = 1f;
    private float _ladderCoolDownTime;
    private float _dashEndTime;
    private float _dashRechargeTime;
    private float _dashDirection;
    private float _dashSpeed;
    private float _ladderJumpValue = 3f;

    private int _currentDashCharges;
    private int _lastDashMaxCharges;
    private int _jumpAttacksUsed;
    private int _airDashesUsed;

    private int _nextAttackDirection;
    private int _lastAttackStep;

    private PlayerState _currentState = PlayerState.Idle;

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

        if (_playerCombatHandler == null)
        {
            _playerCombatHandler = GetComponent<PlayerCombatHandler>();
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
            _playerCombatHandler == null ||
            _animator_Player == null ||
            _spriteRenderer_Player == null ||
            _transform_GroundCheck == null)
        {
            Logger.LogError("Inspector참조를 확인하세요.");
            enabled = false;
            return;
        }
        _originalGravityScale = _rigidbody2D_Player.gravityScale;

        _ladderTriggers = new HashSet<Collider2D>();

        if (_layerMask_Ground.value == 0)
        {
            _layerMask_Ground = LayerMask.GetMask("Ground");
        }

        if (_layerMask_Ladder.value == 0)
        {
            _layerMask_Ladder = LayerMask.GetMask("Ladder");
        }

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
        if (_currentState == PlayerState.Died)
        {
            return;
        }

        _playerInputHandler.ReadInput();
        UpdateJumpAttackState();
        HandleComboAttackState();
        UpdateFacingDirection();
        HandleComboAttackInput();
    }

    private void FixedUpdate()
    {
        if (_currentState == PlayerState.Died)
        {
            return;
        }

        UpdateDashCharges();
        CheckGrounded();
        TryEnterLadder();

        _currentState = GetNextState();
        UpdateBlockState();
        HandleState();
    }

    private void OnDisable()
    {
        if (_isDashing)
        {
            EndDash();
        }

        if (_isBlocking)
        {
            SetBlocking(false);
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
        if (!other.isTrigger || !IsLadderLayer(other.gameObject.layer))
        {
            return;
        }

        _ladderTriggers.Add(other);
        UpdateCurrentLadderCollider();
        _isNearLadeer = _ladderTriggers.Count > 0;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_ladderTriggers == null)
        {
            return;
        }

        _ladderTriggers.Remove(other);
        UpdateCurrentLadderCollider();
        _isNearLadeer = _ladderTriggers.Count > 0;

        if (!_isNearLadeer && _isLadder)
        {
            ExitLadder();
        }
    }

    private void UpdateCurrentLadderCollider()
    {
        _currentLadderCollider = null;
        float nearestDistance = float.PositiveInfinity;

        _ladderTriggers.RemoveWhere(collider => collider == null);

        foreach (Collider2D ladderCollider in _ladderTriggers)
        {
            Vector2 ladderCenter = ladderCollider.bounds.center;
            Vector2 offset = ladderCenter - _rigidbody2D_Player.position;
            float distance = offset.sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                _currentLadderCollider = ladderCollider;
            }
        }
    }

    private bool IsLadderLayer(int layer)
    {
        return (_layerMask_Ladder.value & (1 << layer)) != 0;
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
            _jumpAttacksUsed = 0;
            _airDashesUsed = 0;
        }
    }

    private void TryEnterLadder()
    {
        if (_isLadder ||
            _isDashing ||
            !_isNearLadeer ||
            Time.time < _ladderCoolDownTime ||
            _attackRequested ||
            _isJumpAttack ||
            IsAttackStepChange())
        {
            return;
        }

        bool upPressed = _playerInputHandler.UpInput;
        bool downPressed = _playerInputHandler.DownInput;

        bool hasLadderInput = _isGrounded
            ? upPressed != downPressed
            : upPressed && !downPressed;

        if ( (!hasLadderInput))
        {
            return;
        }

        _isLadder = true;

        if (_currentLadderCollider != null)
        {
            Vector2 position = _rigidbody2D_Player.position;
            position.x = _currentLadderCollider.bounds.center.x;
            _rigidbody2D_Player.position = position;
        }

        _originalGravityScale =  _rigidbody2D_Player.gravityScale;
        _rigidbody2D_Player.gravityScale = 0f;
        _rigidbody2D_Player.linearVelocity = Vector2.zero;
        _ladderDirection = upPressed ? 1f : -1f;

        _playerInputHandler.ClearJumpInput();
        _animator_Player.SetBool("IsLadder", true);
    }

    private PlayerState GetNextState()
    {
        if (_isDashing)
        {
            _playerInputHandler.ConsumeDashInput();
            return PlayerState.Dash;
        }

        if (_isJumpAttack)
        {
            return PlayerState.JumpAttack;
        }

        if (_attackRequested || IsAttackStepChange())
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

            if (_currentDashCharges > 0)
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

    private void HandleState()
    {
        switch (_currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;
            case PlayerState.Walk:
                HandleWalk();
                break;
            case PlayerState.Run:
                HandleRun();
                break;
            case PlayerState.Block:
                _playerInputHandler.ClearJumpInput();
                HandleBlock();
                break;
            case PlayerState.Crouch:
                _playerInputHandler.ClearJumpInput();
                HandleCrouch();
                break;
            case PlayerState.Ladder:
                HandleLadder();
                break;
            case PlayerState.Dash:
                HandleDash();
                break;
            case PlayerState.Jump:
            {
                bool isStartingJump = _isGrounded &&
                    _playerInputHandler.ComsumeJumpInput();

                _playerInputHandler.ClearJumpInput();

                if (isStartingJump)
                {
                    HandleJump(true);
                }
                else if (_rigidbody2D_Player.linearVelocity.y > 0f)
                {
                    HandleJump(false);
                }
                else
                {
                    HandleFall();
                }
                break;
            }
            case PlayerState.Fall:
                _playerInputHandler.ClearJumpInput();
                HandleFall();
                break;
            case PlayerState.ComboAttack:
                HandleAttackMovement();
                break;
            case PlayerState.JumpAttack:
                HandleJumpAttackMovement();
                break;
            case PlayerState.Died:
                break;
        }
    }

    private void HandleIdle()
    {
        SetHorizontalSpeed(0f);
        UpdateAnimatorParameters(false);
    }

    private void HandleWalk()
    {
        SetHorizontalSpeed(_playerInputHandler.MoveInput * _walkSpeed);
        UpdateAnimatorParameters(false);
    }

    private void HandleRun()
    {
        SetHorizontalSpeed(_playerInputHandler.MoveInput * _runSpeed);
        UpdateAnimatorParameters(false);
    }

    private void UpdateBlockState()
    {
        bool shouldBlock = _currentState == PlayerState.Block;
        if (_isBlocking == shouldBlock)
        {
            return;
        }

        SetBlocking(shouldBlock);
    }

    private void SetBlocking(bool isBlocking)
    {
        _isBlocking = isBlocking;
        _animator_Player.SetBool("IsBlocking", isBlocking);

        if (PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.SetBlocking(isBlocking);
        }
    }

    private void HandleBlock()
    {
        SetHorizontalSpeed(0f);
        UpdateAnimatorParameters(false);
    }

    private void HandleCrouch()
    {
        float targetSpeed = _playerInputHandler.MoveInput * _crouchSpeed;
        SetHorizontalSpeed(targetSpeed);
        UpdateAnimatorParameters(true);
    }

    private void HandleLadder()
    {
        float horizontalInput = _playerInputHandler.MoveInput;
        bool isTryingLadderJump =
            _playerInputHandler.JumpInput &&
            Mathf.Abs(horizontalInput) > 0.01f;

        _playerInputHandler.ClearJumpInput();

        if (isTryingLadderJump)
        {
            JumpLadderOut(horizontalInput);
            return;
        }

        float verticalInput =
            (_playerInputHandler.UpInput ? 1f : 0f) -
            (_playerInputHandler.DownInput ? 1f : 0f);

        if (Mathf.Abs(verticalInput) > 0.01f)
        { 
            _ladderDirection = Mathf.Sign(verticalInput);

            _rigidbody2D_Player.linearVelocity = new Vector2(
                0f,
                verticalInput * _ladderSpeed);

            _animator_Player.SetFloat("LadderAnimSpeed", 1f);
        }
        else
        {
            _rigidbody2D_Player.linearVelocity = Vector2.zero;
            _animator_Player.SetFloat("LadderAnimSpeed", 0f);
        }

        _animator_Player.SetFloat("LadderDirection", _ladderDirection);
        UpdateAnimatorParameters(false);
    }

    private void HandleDash()
    {
        if (!_isDashing)
        {
            StartDash();
        }

        if (Time.time >= _dashEndTime)
        {
            EndDash();
            return;
        }

        _rigidbody2D_Player.linearVelocity = new Vector2(
            _dashDirection * _dashSpeed,
            0f);
        UpdateAnimatorParameters(false);
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashDirection = Mathf.Abs(_playerInputHandler.MoveInput) > 0.01f
            ? Mathf.Sign(_playerInputHandler.MoveInput)
            : (_spriteRenderer_Player.flipX ? -1f : 1f);
        float dashDuration = Mathf.Max(0.01f, _dashDuration);
        _dashSpeed = Mathf.Max(0f, _dashDistance) / dashDuration;
        _dashEndTime = Time.time + dashDuration;
        if (_currentDashCharges == _lastDashMaxCharges)
        {
            _dashRechargeTime = Time.time + _dashCoolDuration;
        }

        _currentDashCharges--;
        if (!_isGrounded)
        {
            _airDashesUsed++;
        }

        NotifyDashChargesChanged(true);
        _rigidbody2D_Player.linearVelocity = new Vector2(
            _rigidbody2D_Player.linearVelocity.x,
            0f);
        _animator_Player.SetBool("IsDash", true);
    }

    private void UpdateDashCharges()
    {
        int previousDashCharges = _currentDashCharges;
        int previousMaxDashCharges = _lastDashMaxCharges;
        int maxDashCharges = MaxDashCharges;

        if (!_dashChargesInitialized)
        {
            _currentDashCharges = maxDashCharges;
            _lastDashMaxCharges = maxDashCharges;
            _dashChargesInitialized = true;
            NotifyDashChargesChanged(false);
            return;
        }

        if (maxDashCharges > _lastDashMaxCharges)
        {
            _currentDashCharges = Mathf.Min(
                maxDashCharges,
                _currentDashCharges + maxDashCharges - _lastDashMaxCharges);
        }
        else if (maxDashCharges < _lastDashMaxCharges)
        {
            _currentDashCharges = Mathf.Min(_currentDashCharges, maxDashCharges);
        }

        _lastDashMaxCharges = maxDashCharges;

        if (_currentDashCharges >= maxDashCharges)
        {
            _dashRechargeTime = 0f;
        }
        else if (_dashRechargeTime <= 0f)
        {
            _dashRechargeTime = Time.time + _dashCoolDuration;
        }
        else if (Time.time >= _dashRechargeTime)
        {
            _currentDashCharges++;
            _dashRechargeTime = _currentDashCharges < maxDashCharges
                ? Time.time + _dashCoolDuration
                : 0f;
        }

        if (_currentDashCharges != previousDashCharges ||
            maxDashCharges != previousMaxDashCharges)
        {
            NotifyDashChargesChanged(false);
        }
    }

    private void NotifyDashChargesChanged(bool wasDashUsed)
    {
        OnDashChargesChanged?.Invoke(
            _currentDashCharges,
            MaxDashCharges,
            wasDashUsed);
    }

    private void EndDash()
    {
        _isDashing = false;
        _rigidbody2D_Player.linearVelocity = Vector2.zero;
        _animator_Player.SetBool("IsDash", false);
        UpdateAnimatorParameters(false);
    }

    private void JumpLadderOut(float horizontalInput)
    {
        float jumpDirection = Mathf.Sign(horizontalInput);

        _isLadder = false;
        _rigidbody2D_Player.gravityScale = _originalGravityScale;
        _ladderCoolDownTime = Time.time + _ladderCoolDown;

        _rigidbody2D_Player.linearVelocity = new Vector2(
            jumpDirection * _ladderJumpValue,
            _jumpValue);

        _spriteRenderer_Player.flipX = jumpDirection < 0f;
        _isGrounded = false;

        _animator_Player.SetFloat("LadderAnimSpeed", 0f);
        _animator_Player.SetBool("IsLadder", false);
        _playerInputHandler.ClearJumpInput();

        UpdateAnimatorParameters(false);
    }

    private void ExitLadder()
    {
        _isLadder = false;
        _rigidbody2D_Player.gravityScale = _originalGravityScale;
        _rigidbody2D_Player.linearVelocity = new Vector2(
            _rigidbody2D_Player.linearVelocity.x,
            0f);

        _animator_Player.SetFloat("LadderAnimSpeed", 0f);
        _animator_Player.SetBool("IsLadder", false);
    }

    private void HandleComboAttackInput()
    {
        AnimatorStateInfo stateInfo = _animator_Player.GetCurrentAnimatorStateInfo(0);
        int attackStep = GetComboAttackStep(stateInfo);

        if (_isJumpAttack)
        {
            _playerInputHandler.ConsumeAttackInput();
            return;
        }

        if (attackStep >= 1 && attackStep <= 3)
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;
            bool isComboInputWindow =
                normalizedTime >= _comboInputStart &&
                normalizedTime <= _comboInputEnd;

            if (!_playerInputHandler.AttackInput)
            {
                _animator_Player.SetBool("ComboAttackSign", false);
            }
            else if (isComboInputWindow)
            {
                _animator_Player.SetBool("ComboAttackSign", true);
            }

            if (isComboInputWindow &&
                _playerInputHandler.AttackInput &&
                Mathf.Abs(_playerInputHandler.MoveInput) > 0.01f)
            {
                _nextAttackDirection =
                    _playerInputHandler.MoveInput < 0f ? -1 : 1;
            }

            _playerInputHandler.ConsumeAttackInput();
            return;
        }

        if (!_playerInputHandler.ConsumeAttackInput())
        {
            return;
        }

        if (stateInfo.IsName("LocoMotion") &&
            !_animator_Player.IsInTransition(0) &&
            _isGrounded)
        {
            _nextAttackDirection = 0;
            _animator_Player.SetBool("ComboAttackSign", false);
            _animator_Player.SetTrigger("ComboAttack");
            _attackRequested = true;
            _currentState = PlayerState.ComboAttack;
        }
        else if (!_isGrounded &&
            !_isDashing &&
            !_isLadder &&
            _jumpAttacksUsed < _airDashesUsed + 1)
        {
            _jumpAttacksUsed++;
            _nextAttackDirection = 0;
            _isJumpAttack = true;
            _jumpAttackAnimationEntered = false;
            _animator_Player.SetTrigger("JumpAttack");
            _currentState = PlayerState.JumpAttack;
        }
    }

    private void HandleComboAttackState()
    {
        AnimatorStateInfo stateInfo = _animator_Player.GetCurrentAnimatorStateInfo(0);
        int currentAttackStep = GetComboAttackStep(stateInfo);

        if (currentAttackStep == _lastAttackStep)
        {
            return;
        }

        if (currentAttackStep != 0)
        {
            _attackRequested = false;
        }

        if (currentAttackStep >= 1 && currentAttackStep <= 3)
        {
            if (_nextAttackDirection != 0)
            {
                _spriteRenderer_Player.flipX = _nextAttackDirection < 0;
                _nextAttackDirection = 0;
            }

            _animator_Player.SetBool("ComboAttackSign", false);
        }
        else
        {
            _nextAttackDirection = 0;
            _animator_Player.SetBool("ComboAttackSign", false);
        }

        _lastAttackStep = currentAttackStep;
    }

    private void HandleJumpAttackMovement()
    {
        _playerInputHandler.ClearJumpInput();
        SetHorizontalSpeed(_playerInputHandler.MoveInput * _walkSpeed);
        UpdateAnimatorParameters(false);
    }

    private void UpdateJumpAttackState()
    {
        if (!_isJumpAttack)
        {
            return;
        }

        AnimatorStateInfo stateInfo = _animator_Player.GetCurrentAnimatorStateInfo(0);
        bool isInJumpAttackState = stateInfo.IsName("PlayerAnim_JumpAttack");

        if (_animator_Player.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _animator_Player.GetNextAnimatorStateInfo(0);
            isInJumpAttackState |= nextState.IsName("PlayerAnim_JumpAttack");
        }

        if (isInJumpAttackState)
        {
            _jumpAttackAnimationEntered = true;
        }
        else if (_jumpAttackAnimationEntered)
        {
            _isJumpAttack = false;
            _jumpAttackAnimationEntered = false;
        }
        else if (_isGrounded)
        {
            _animator_Player.ResetTrigger("JumpAttack");
            _isJumpAttack = false;
        }
    }

    private void HandleDied()
    {
        if (_currentState == PlayerState.Died)
        {
            return;
        }

        _currentState = PlayerState.Died;
        _attackRequested = false;
        _isJumpAttack = false;
        _jumpAttackAnimationEntered = false;
        _nextAttackDirection = 0;

        if (_isDashing)
        {
            EndDash();
        }

        if (_isLadder)
        {
            ExitLadder();
        }

        if (_isBlocking)
        {
            SetBlocking(false);
        }

        _playerInputHandler.ClearInput();
        _rigidbody2D_Player.linearVelocity = Vector2.zero;
        _animator_Player.SetBool("ComboAttackSign", false);
        _animator_Player.ResetTrigger("JumpAttack");
        _animator_Player.SetBool("IsDead", true);
    }

    private int GetComboAttackStep(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("ComboAtk1"))
        {
            return 1;
        }

        if (stateInfo.IsName("ComboAtk2"))
        {
            return 2;
        }

        if (stateInfo.IsName("ComboAtk3"))
        {
            return 3;
        }

        if (stateInfo.IsName("ComboAtkFinish"))
        {
            return 4;
        }

        return 0;
    }

    private bool IsAttackStepChange()
    {
        AnimatorStateInfo currentState = _animator_Player.GetCurrentAnimatorStateInfo(0);

        if (GetComboAttackStep(currentState) != 0)
        {
            return true;
        }

        if (_animator_Player.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _animator_Player.GetNextAnimatorStateInfo(0);

            return GetComboAttackStep(nextState) != 0;
        }

        return false;
    }

    private void HandleAttackMovement()
    {
        _playerInputHandler.ClearJumpInput();
        _rigidbody2D_Player.linearVelocity = new Vector2(
            0f,
            _rigidbody2D_Player.linearVelocity.y);
        UpdateAnimatorParameters(false);
    }

    private void HandleJump(bool isStartingJump)
    {
        if (isStartingJump)
        {
            _rigidbody2D_Player.linearVelocity = new Vector2(
                _rigidbody2D_Player.linearVelocity.x,_jumpValue);

            _isGrounded = false;
        }

        UpdateAnimatorParameters(false);
    }

    private void HandleFall()
    {
        UpdateAnimatorParameters(false);
    }

    private void SetHorizontalSpeed(float targetSpeed)
    {
        _rigidbody2D_Player.linearVelocity = new Vector2(
            targetSpeed,
            _rigidbody2D_Player.linearVelocity.y);
    }

    private void UpdateAnimatorParameters(bool isCrouching)
    {
        float normalizedMoveSpeed = _runSpeed > 0f
            ? Mathf.Abs(_rigidbody2D_Player.linearVelocity.x) / _runSpeed
            : 0f;

        _animator_Player.SetFloat(
            "MoveSpeed",
            normalizedMoveSpeed,
            0.08f,
            Time.fixedDeltaTime);

        _animator_Player.SetBool("IsGrounded", _isGrounded);
        _animator_Player.SetBool("IsCrouching", isCrouching);
        _animator_Player.SetBool("IsLadder", _isLadder);
        _animator_Player.SetFloat(
            "LadderDirection",
            _ladderDirection);
        _animator_Player.SetFloat(
            "VerticalSpeed",
            _rigidbody2D_Player.linearVelocity.y);
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