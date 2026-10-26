public class WalkState : GroundMoveState
{
    public WalkState(PlayerStateContext ctx) : base(ctx) { }

    protected override float GetTargetSpeed()
    {
        return Ctx.Input.MoveInput * PlayerStatSet.Instance.WalkSpeed;
    }
}