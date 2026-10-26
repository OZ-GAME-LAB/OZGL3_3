using UnityEngine;

public class JumpState : PlayerStateBase
{
    public JumpState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        bool isStartingJump = Ctx.IsGrounded && Ctx.Input.ComsumeJumpInput();

        Ctx.Input.ClearJumpInput();

        if (isStartingJump)
        {
            Ctx.Rigidbody.linearVelocity = new Vector2(
                Ctx.Rigidbody.linearVelocity.x,
                PlayerStatSet.Instance.JumpValue);

            Ctx.IsGrounded = false;
        }
    }
}