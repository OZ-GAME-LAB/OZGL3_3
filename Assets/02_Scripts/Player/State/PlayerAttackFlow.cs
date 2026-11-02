using UnityEngine;

public class PlayerAttackFlow
{
    private readonly PlayerStateContext _playerContext;
    private readonly DashState _dashState;
    private readonly LadderState _ladderState;
    private readonly float _comboInputStart;
    private readonly float _comboInputEnd;

    private bool _attackRequested;
    private bool _isJumpAttack;
    private bool _jumpAttackAnimationEntered;
    private int _nextAttackDirection;
    private int _lastAttackStep;

    public bool IsJumpAttack => _isJumpAttack;
    public bool IsComboAttack => _attackRequested || IsAttackStepChange();

    public PlayerAttackFlow(
        PlayerStateContext playerContext,
        DashState dashState,
        LadderState ladderState,
        float comboInputStart,
        float comboInputEnd)
    {
        _playerContext = playerContext;
        _dashState = dashState;
        _ladderState = ladderState;
        _comboInputStart = comboInputStart;
        _comboInputEnd = comboInputEnd;
    }

    public void UpdateState()
    {
        UpdateJumpAttackState();
        HandleComboAttackState();
    }

    public void HandleInput()
    {
        AnimatorStateInfo stateInfo = _playerContext.Animator.GetCurrentAnimatorStateInfo(0);
        int attackStep = GetComboAttackStep(stateInfo);

        if (_isJumpAttack)
        {
            _playerContext.Input.ConsumeAttackInput();
            return;
        }

        if (attackStep >= 1 && attackStep <= 3)
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;
            bool isComboInputWindow =
                normalizedTime >= _comboInputStart &&
                normalizedTime <= _comboInputEnd;

            if (!_playerContext.Input.AttackInput)
            {
                _playerContext.Animator.SetBool("ComboAttackSign", false);
            }
            else if (isComboInputWindow)
            {
                _playerContext.Animator.SetBool("ComboAttackSign", true);
            }

            if (isComboInputWindow &&
                _playerContext.Input.AttackInput &&
                Mathf.Abs(_playerContext.Input.MoveInput) > 0.01f)
            {
                _nextAttackDirection = _playerContext.Input.MoveInput < 0f ? -1 : 1;
            }

            _playerContext.Input.ConsumeAttackInput();
            return;
        }

        if (!_playerContext.Input.ConsumeAttackInput())
        {
            return;
        }

        if (stateInfo.IsName("LocoMotion") &&
            !_playerContext.Animator.IsInTransition(0) &&
            _playerContext.IsGrounded)
        {
            _nextAttackDirection = 0;
            _playerContext.Animator.SetBool("ComboAttackSign", false);
            _playerContext.Animator.SetTrigger("ComboAttack");
            _attackRequested = true;
        }
        else if (!_playerContext.IsGrounded &&
            !_dashState.IsDashing &&
            !_ladderState.IsClimbing &&
            _playerContext.JumpAttacksUsed < _playerContext.AirDashesUsed + 1)
        {
            _playerContext.JumpAttacksUsed++;
            _nextAttackDirection = 0;
            _isJumpAttack = true;
            _jumpAttackAnimationEntered = false;
            _playerContext.Animator.SetTrigger("JumpAttack");
        }
    }

    public void ResetFlags()
    {
        _attackRequested = false;
        _isJumpAttack = false;
        _jumpAttackAnimationEntered = false;
        _nextAttackDirection = 0;
    }

    public bool IsAttackStepChange()
    {
        AnimatorStateInfo currentState = _playerContext.Animator.GetCurrentAnimatorStateInfo(0);

        if (GetComboAttackStep(currentState) != 0)
        {
            return true;
        }

        if (_playerContext.Animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _playerContext.Animator.GetNextAnimatorStateInfo(0);
            return GetComboAttackStep(nextState) != 0;
        }

        return false;
    }

    private void HandleComboAttackState()
    {
        AnimatorStateInfo stateInfo = _playerContext.Animator.GetCurrentAnimatorStateInfo(0);
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
                _playerContext.SpriteRenderer.flipX = _nextAttackDirection < 0;
                _nextAttackDirection = 0;
            }

            _playerContext.Animator.SetBool("ComboAttackSign", false);
        }
        else
        {
            _nextAttackDirection = 0;
            _playerContext.Animator.SetBool("ComboAttackSign", false);
        }

        _lastAttackStep = currentAttackStep;
    }

    private void UpdateJumpAttackState()
    {
        if (!_isJumpAttack)
        {
            return;
        }

        AnimatorStateInfo stateInfo = _playerContext.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInJumpAttackState = stateInfo.IsName("PlayerAnim_JumpAttack");

        if (_playerContext.Animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _playerContext.Animator.GetNextAnimatorStateInfo(0);
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
        else if (_playerContext.IsGrounded)
        {
            _playerContext.Animator.ResetTrigger("JumpAttack");
            _isJumpAttack = false;
        }
    }

    private int GetComboAttackStep(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("ComboAtk1")) return 1;
        if (stateInfo.IsName("ComboAtk2")) return 2;
        if (stateInfo.IsName("ComboAtk3")) return 3;
        if (stateInfo.IsName("ComboAtkFinish")) return 4;
        return 0;
    }
}