using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerCombatHandler))]
[RequireComponent(typeof(PlayerStats))]
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

    [Header("MovementValue")]
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 14f;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.24f;

    [Header("Ladder")]
    [SerializeField] private float _ladderCoolDown = 1f;

    [Header("InputTime")]
    [SerializeField, Range(0f, 0.4f)] private float _comboInputStart = 0.2f;
    [SerializeField, Range(0.4f, 0.8f)] private float _comboInputEnd = 0.78f;

    private float _jumpValue => PlayerStats.Instance.JumpValue;
    private float _walkSpeed => PlayerStats.Instance.WalkSpeed;
    private float _runSpeed => PlayerStats.Instance.RunSpeed;
    private float _crouchSpeed => PlayerStats.Instance.CrouchSpeed;
    private float _ladderSpeed => PlayerStats.Instance.LadderSpeed;

    private HashSet<Collider2D> _ladderTriggers;
    private Collider2D _currentLadderCollider;

    private bool _isGrounded;
    private bool _isLadder;
    private bool _isNearLadeer;
    private bool _attackRequested;
    private float _originalGravityScale;
    private float _ladderDirection = 1f;
    private float _ladderCoolDownTime;
    private float _ladderJumpValue = 3f;
    private int _nextAttackDirection;
    private int _lastAttackStep;
    private PlayerState _currentState = PlayerState.Idle;
    private bool _isDead;

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
        PlayerStatus.Instance.OnDied += HandleDied;

        if (PlayerStatus.Instance.IsDead)
        {
            HandleDied();
        }
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        _playerInputHandler.ReadInput();
        HandleComboAttackState();
        UpdateFacingDirection();
        HandleComboAttackInput();
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            return;
        }

        CheckGrounded();
        TryEnterLadder();

        _currentState = GetNextState();
        HandleState();
    }

    private void OnDisable()
    {
        PlayerStatus.Instance.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        _currentState = PlayerState.Died;
        _attackRequested = false;
        _nextAttackDirection = 0;

        if (_isLadder)
        {
            ExitLadder();
        }

        _playerInputHandler.ClearInput();
        _rigidbody2D_Player.linearVelocity = Vector2.zero;
        _animator_Player.SetBool("ComboAttackSign", false);
        _animator_Player.SetBool("IsDead", true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger || !IsLadderLayer(other.gameObject.layer))
        {
            return;
        }

        _ladderTriggers.Add(other);

        if (_currentLadderCollider == null)
        {
            _currentLadderCollider = other;
        }

        _isNearLadeer = _ladderTriggers.Count > 0;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || _ladderTriggers == null ||
            !_ladderTriggers.Remove(other))
        {
            return;
        }

        _isNearLadeer = _ladderTriggers.Count > 0;

        if (!_isNearLadeer && _isLadder)
        {
            ExitLadder();
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
        _isGrounded = Physics2D.OverlapCircle(
            _transform_GroundCheck.position,
            _groundCheckRadius,
            _layerMask_Ground) != null;
    }

    private void TryEnterLadder()
    {
        if (_isLadder ||
            !_isNearLadeer ||
            Time.time < _ladderCoolDownTime ||
            _attackRequested ||
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
        if (_attackRequested || IsAttackStepChange())
        {
            return PlayerState.ComboAttack;
        }

        if (_isLadder)
        {
            return PlayerState.Ladder;
        }

        if (!_isGrounded)
        {
            return GetAirState();
        }

        return GetGroundedState();
    }

    private PlayerState GetGroundedState()
    {
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
            case PlayerState.Crouch:
                _playerInputHandler.ClearJumpInput();
                HandleCrouch();
                break;
            case PlayerState.Ladder:
                HandleLadder();
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