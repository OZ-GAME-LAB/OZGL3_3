using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerCombatHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D _rigidbody2D_Player;
    [SerializeField] private PlayerInputHandler _playerInputHandler;
    [SerializeField] private PlayerCombatHandler _playerCombatHandler;
    [SerializeField] private Animator _animator_Player;
    [SerializeField] private SpriteRenderer _spriteRenderer_Player;
    [SerializeField] private Transform _transform_GroundCheck;
    [SerializeField] private LayerMask _layerMask_Ground;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 4f;
    [SerializeField] private float _crouchSpeed = 1.2f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 14f;
    [SerializeField] private float _jumpVelocity = 4f;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.12f;

    [Header("Attack")]
    [SerializeField, Range(0f, 1f)] private float _comboInputStart = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _comboInputEnd = 0.78f;

    private bool _isGrounded;
    private bool _attackRequested;
    private int _nextAttackDirection;
    private int _lastAttackStep;
    private PlayerState _currentState = PlayerState.Idle;

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

        if (_playerInputHandler == null ||
            _playerCombatHandler == null ||
            _animator_Player == null ||
            _spriteRenderer_Player == null ||
            _transform_GroundCheck == null)
        {
            Debug.LogError("Inspector참조를 확인하세요.",this);

            enabled = false;
            return;
        }
    }

    private void Update()
    {
        _playerInputHandler.ReadInput();
        HandleComboAttackState();
        UpdateFacingDirection();
        HandleComboAttackInput();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        _currentState = GetNextState();
        HandleState();
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

    private PlayerState GetNextState()
    {
        if (_attackRequested || IsAttackStepChange())
        {
            return PlayerState.ComboAttack;
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

        return _playerInputHandler.RunInput
            ? PlayerState.Run
            : PlayerState.Walk;
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
            //_playerCombatHandler.ApplyAttackDamage();

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
                _rigidbody2D_Player.linearVelocity.x,_jumpVelocity);

            _isGrounded = false;
        }

        HandleAirMovement();
        UpdateAnimatorParameters(false);
    }

    private void HandleFall()
    {
        HandleAirMovement();
        UpdateAnimatorParameters(false);
    }

    private void HandleAirMovement()
    {
        float targetSpeed = 0f;

        if (Mathf.Abs(_playerInputHandler.MoveInput) > 0.01f)
        {
            float speed = _playerInputHandler.RunInput
                ? _runSpeed
                : _walkSpeed;
            targetSpeed = _playerInputHandler.MoveInput * speed;
        }

        SetHorizontalSpeed(targetSpeed);
    }

    private void SetHorizontalSpeed(float targetSpeed)
    {
        bool isStoppingOrReversing =
            Mathf.Abs(targetSpeed) < 0.01f ||
            (Mathf.Abs(_rigidbody2D_Player.linearVelocity.x) > 0.01f &&
             Mathf.Sign(targetSpeed) != Mathf.Sign(_rigidbody2D_Player.linearVelocity.x));

        float changeRate = isStoppingOrReversing
            ? _deceleration
            : _acceleration;

        float nextSpeed = Mathf.MoveTowards(
            _rigidbody2D_Player.linearVelocity.x,
            targetSpeed,
            changeRate * Time.fixedDeltaTime);

        _rigidbody2D_Player.linearVelocity = new Vector2(
            nextSpeed,
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
        _animator_Player.SetFloat("VerticalSpeed", _rigidbody2D_Player.linearVelocity.y);
        _animator_Player.SetBool("IsCrouching", isCrouching);

        AnimatorStateInfo stateInfo = _animator_Player.GetCurrentAnimatorStateInfo(0);
        if (GetComboAttackStep(stateInfo) != 0)
        {
            return;
        }
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