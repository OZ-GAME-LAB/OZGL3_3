public class CrouchState : GroundMoveState
{
    public CrouchState(PlayerStateContext ctx) : base(ctx) { }

    protected override float GetTargetSpeed()
    {
        return Ctx.Input.MoveInput * PlayerStatSet.Instance.CrouchSpeed;
    }

    public override void StateFixedUpdate()
    {
        Ctx.Input.ClearJumpInput();
        base.StateFixedUpdate();
    }
}