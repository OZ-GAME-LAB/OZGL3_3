using UnityEngine;

public class JumpState : PlayerStateBase
{
    public JumpState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        bool isStartingJump = Ctx.IsGrounded && Ctx.Input.ComsumeJumpInput();
        Ctx.Input.ClearJumpInput();

        float horizontalSpeed = Ctx.Rigidbody.linearVelocity.x;

        if (Mathf.Abs(Ctx.Input.MoveInput) > 0.01f)
        {
            float moveSpeed = Ctx.Input.RunInput
                ? PlayerStatSet.Instance.RunSpeed
                : PlayerStatSet.Instance.WalkSpeed;

            horizontalSpeed = Ctx.Input.MoveInput * moveSpeed;
        }

        float verticalSpeed = isStartingJump
            ? PlayerStatSet.Instance.JumpValue
            : Ctx.Rigidbody.linearVelocity.y;

        Ctx.Rigidbody.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);

        if (isStartingJump)
        {
            Ctx.IsGrounded = false;
        }
    }
}