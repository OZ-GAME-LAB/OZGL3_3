using UnityEngine;

public class PlayerAttackFlow
{
    private readonly PlayerStateContext _ctx;
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
        PlayerStateContext ctx,
        DashState dashState,
        LadderState ladderState,
        float comboInputStart,
        float comboInputEnd)
    {
        _ctx = ctx;
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
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        int attackStep = GetComboAttackStep(stateInfo);

        if (_isJumpAttack)
        {
            _ctx.Input.ConsumeAttackInput();
            return;
        }

        if (attackStep >= 1 && attackStep <= 3)
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;
            bool isComboInputWindow =
                normalizedTime >= _comboInputStart &&
                normalizedTime <= _comboInputEnd;

            if (!_ctx.Input.AttackInput)
            {
                _ctx.Animator.SetBool("ComboAttackSign", false);
            }
            else if (isComboInputWindow)
            {
                _ctx.Animator.SetBool("ComboAttackSign", true);
            }

            if (isComboInputWindow &&
                _ctx.Input.AttackInput &&
                Mathf.Abs(_ctx.Input.MoveInput) > 0.01f)
            {
                _nextAttackDirection = _ctx.Input.MoveInput < 0f ? -1 : 1;
            }

            _ctx.Input.ConsumeAttackInput();
            return;
        }

        if (!_ctx.Input.ConsumeAttackInput())
        {
            return;
        }

        if (stateInfo.IsName("LocoMotion") &&
            !_ctx.Animator.IsInTransition(0) &&
            _ctx.IsGrounded)
        {
            _nextAttackDirection = 0;
            _ctx.Animator.SetBool("ComboAttackSign", false);
            _ctx.Animator.SetTrigger("ComboAttack");
            _attackRequested = true;
        }
        else if (!_ctx.IsGrounded &&
            !_dashState.IsDashing &&
            !_ladderState.IsClimbing &&
            _ctx.JumpAttacksUsed < _ctx.AirDashesUsed + 1)
        {
            _ctx.JumpAttacksUsed++;
            _nextAttackDirection = 0;
            _isJumpAttack = true;
            _jumpAttackAnimationEntered = false;
            _ctx.Animator.SetTrigger("JumpAttack");
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
        AnimatorStateInfo currentState = _ctx.Animator.GetCurrentAnimatorStateInfo(0);

        if (GetComboAttackStep(currentState) != 0)
        {
            return true;
        }

        if (_ctx.Animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _ctx.Animator.GetNextAnimatorStateInfo(0);
            return GetComboAttackStep(nextState) != 0;
        }

        return false;
    }

    private void HandleComboAttackState()
    {
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
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
                _ctx.SpriteRenderer.flipX = _nextAttackDirection < 0;
                _nextAttackDirection = 0;
            }

            _ctx.Animator.SetBool("ComboAttackSign", false);
        }
        else
        {
            _nextAttackDirection = 0;
            _ctx.Animator.SetBool("ComboAttackSign", false);
        }

        _lastAttackStep = currentAttackStep;
    }

    private void UpdateJumpAttackState()
    {
        if (!_isJumpAttack)
        {
            return;
        }

        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInJumpAttackState = stateInfo.IsName("PlayerAnim_JumpAttack");

        if (_ctx.Animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _ctx.Animator.GetNextAnimatorStateInfo(0);
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
        else if (_ctx.IsGrounded)
        {
            _ctx.Animator.ResetTrigger("JumpAttack");
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