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
        PlayerStateContext playerContext,
        PlayerDashCharge charge,
        float dashDistance,
        float dashDuration
        ) : base(playerContext)
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
        PlayerContext.Input.ClearJumpInput();

        if (!IsDashing)
        {
            Begin();
        }

        if (Time.time >= _dashEndTime)
        {
            End();
            return;
        }

        PlayerContext.Rigidbody.linearVelocity = new Vector2(_dashDirection * _dashSpeed, 0f);
    }

    private void Begin()
    {
        IsDashing = true;

        float moveInput = PlayerContext.Input.MoveInput;
        _dashDirection = Mathf.Abs(moveInput) > 0.01f
            ? Mathf.Sign(moveInput)
            : PlayerContext.Facing;

        float duration = Mathf.Max(0.01f, _dashDuration);
        _dashSpeed = Mathf.Max(0f, _dashDistance) / duration;
        _dashEndTime = Time.time + duration;

        _charge.ConsumeDashCharge();

        if (!PlayerContext.IsGrounded)
        {
            PlayerContext.AirDashesUsed++;
        }

        PlayerContext.Rigidbody.linearVelocity = new Vector2(PlayerContext.Rigidbody.linearVelocity.x, 0f);
        PlayerContext.Animator.SetBool("IsDash", true);
    }

    private void End()
    {
        IsDashing = false;
        PlayerContext.Rigidbody.linearVelocity = Vector2.zero;
        PlayerContext.Animator.SetBool("IsDash", false);
    }
}