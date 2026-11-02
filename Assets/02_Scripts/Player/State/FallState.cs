public class FallState : PlayerStateBase
{
    public FallState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        Ctx.Input.ClearJumpInput();
    }
}