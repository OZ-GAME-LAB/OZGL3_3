public class RunState : GroundMoveState
{
    public RunState(PlayerStateContext ctx) : base(ctx) { }

    protected override float GetTargetSpeed()
    {
        return Ctx.Input.MoveInput * PlayerStatSet.Instance.RunSpeed;
    }
}