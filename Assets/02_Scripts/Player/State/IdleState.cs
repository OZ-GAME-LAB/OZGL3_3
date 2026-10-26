public class IdleState : GroundMoveState
{
    public IdleState(PlayerStateContext ctx) : base(ctx) { }

    protected override float GetTargetSpeed() => 0f;
}