public class CrouchState : GroundMoveState
{
    private readonly float _visualOffsetY;

    public CrouchState(PlayerStateContext ctx, float visualOffsetY) : base(ctx)
    {
        _visualOffsetY = visualOffsetY;
    }

    public override void StateEnter()
    {
        Ctx.Visual?.SetOffsetY(_visualOffsetY);
    }

    public override void StateExit()
    {
        Ctx.Visual?.SetOffsetY(0f);
    }

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