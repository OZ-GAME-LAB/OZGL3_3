using UnityEngine;

public class HealState : PlayerStateBase
{
    public HealState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        Ctx.Input.ClearJumpInput();
        Ctx.Rigidbody.linearVelocity = new Vector2(0f, Ctx.Rigidbody.linearVelocity.y);
    }
}
