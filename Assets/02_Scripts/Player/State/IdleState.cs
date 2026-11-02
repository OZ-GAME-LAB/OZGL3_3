public class IdleState : GroundMoveState
{
    public IdleState(PlayerStateContext playerContext) : base(playerContext) { }

    protected override float GetTargetSpeed() => 0f;
}