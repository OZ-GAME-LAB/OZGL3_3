using UnityEngine;

public abstract class GroundMoveState : PlayerStateBase
{
    protected GroundMoveState(PlayerStateContext ctx) : base(ctx) { }

    protected abstract float GetTargetSpeed();

    public override void StateFixedUpdate()
    {
        Ctx.Rigidbody.linearVelocity = new Vector2(
            GetTargetSpeed(),
            Ctx.Rigidbody.linearVelocity.y);
    }
}