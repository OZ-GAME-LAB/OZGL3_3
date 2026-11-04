using UnityEngine;

public class HurtState : PlayerStateBase
{
    private readonly float _KnockbackX;
    private readonly float _KnockbackY;
    private readonly float _maxHurtTime;

    private float _knockbackDirection;
    private float _hurtEndTime;
    private Vector2 _KnockbackVelocity;
    private bool _useCustomKnockbackVelocity;

    public bool IsFinished
    {
        get
        {
            if (Time.time >= _hurtEndTime)
            {
                return true;
            }

            AnimatorStateInfo stateInfo =
                PlayerContext.Animator.GetCurrentAnimatorStateInfo(0);

            return stateInfo.IsName("PlayerAnim_Hurt(NoFX)") &&
                   stateInfo.normalizedTime >= 1f;
        }
    }

    public HurtState(
        PlayerStateContext playerContext,
        float horizontalKnockbackSpeed,
        float verticalKnockbackSpeed,
        float fallbackDuration)
        : base(playerContext)
    {
        _KnockbackX = horizontalKnockbackSpeed;
        _KnockbackY = verticalKnockbackSpeed;
        _maxHurtTime = fallbackDuration;
    }

    public void SetKnockbackDirection(float direction)
    {
        _knockbackDirection = Mathf.Sign(direction);
        _useCustomKnockbackVelocity = false;
    }

    public void SetKnockbackVelocity(Vector2 velocity)
    {
        _KnockbackVelocity = velocity;
        _useCustomKnockbackVelocity = true;
    }

    public void ApplyKnockback()
    {
        if (_useCustomKnockbackVelocity)
        {
            PlayerContext.Rigidbody.linearVelocity = _KnockbackVelocity;
            return;
        }

        PlayerContext.Rigidbody.linearVelocity = new Vector2(
            _knockbackDirection * _KnockbackX,
            _KnockbackY);
    }

    public override void StateEnter()
    {
        _hurtEndTime = Time.time + _maxHurtTime;
        PlayerContext.Input.ClearInput();
        PlayerContext.Animator.SetBool("IsHurt", true);
        PlayerContext.Animator.Play("PlayerAnim_Hurt(NoFX)", 0, 0f);
        ApplyKnockback();
    }

    public override void StateExit()
    {
        PlayerContext.Animator.SetBool("IsHurt", false);
    }

    public override void StateFixedUpdate()
    {
        PlayerContext.Input.ClearInput();
    }
}
