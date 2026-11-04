public class FallState : PlayerStateBase
{
    public FallState(PlayerStateContext playerContext) : base(playerContext) { }

    public override void StateFixedUpdate()
    {
        PlayerContext.TryJump();
    }
}