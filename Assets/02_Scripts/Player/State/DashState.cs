using UnityEngine;

public class DashState : PlayerStateBase
{
    private readonly PlayerDashCharge _charge;
    private readonly float _dashDistance;
    private readonly float _dashDuration;

    private float _dashDirection;
    private float _dashSpeed;
    private float _dashEndTime;

    public bool IsDashing { get; private set; }

    public DashState(
        PlayerStateContext ctx,
        PlayerDashCharge charge,
        float dashDistance,
        float dashDuration
        ) : base(ctx)
    {
        _charge = charge;
        _dashDistance = dashDistance;
        _dashDuration = dashDuration;
    }

    public override void StateExit()
    {
        if (IsDashing)
        {
            End();
        }
    }

    public override void StateFixedUpdate()
    {
        if (!IsDashing)
        {
            Begin();
        }

        if (Time.time >= _dashEndTime)
        {
            End();
            return;
        }

        Ctx.Rigidbody.linearVelocity = new Vector2(_dashDirection * _dashSpeed, 0f);
    }

    private void Begin()
    {
        IsDashing = true;

        float moveInput = Ctx.Input.MoveInput;
        _dashDirection = Mathf.Abs(moveInput) > 0.01f
            ? Mathf.Sign(moveInput)
            : Ctx.Facing;

        float duration = Mathf.Max(0.01f, _dashDuration);
        _dashSpeed = Mathf.Max(0f, _dashDistance) / duration;
        _dashEndTime = Time.time + duration;

        _charge.Consume();

        if (!Ctx.IsGrounded)
        {
            Ctx.AirDashesUsed++;
        }

        Ctx.Rigidbody.linearVelocity = new Vector2(Ctx.Rigidbody.linearVelocity.x, 0f);
        Ctx.Animator.SetBool("IsDash", true);
    }

    private void End()
    {
        IsDashing = false;
        Ctx.Rigidbody.linearVelocity = Vector2.zero;
        Ctx.Animator.SetBool("IsDash", false);
    }
}