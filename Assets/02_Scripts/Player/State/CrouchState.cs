public class CrouchState : GroundMoveState
{
    private readonly float _visualOffsetY;

    public CrouchState(PlayerStateContext playerContext, float visualOffsetY) : base(playerContext)
    {
        _visualOffsetY = visualOffsetY;
    }

    public override void StateEnter()
    {
        PlayerContext.Visual?.SetOffsetY(_visualOffsetY);
    }

    public override void StateExit()
    {
        PlayerContext.Visual?.SetOffsetY(0f);
    }

    protected override float GetTargetSpeed()
    {
        return PlayerContext.Input.MoveInput * PlayerStatSet.Instance.CrouchSpeed;
    }

    public override void StateFixedUpdate()
    {
        PlayerContext.Input.ClearJumpInput();
        base.StateFixedUpdate();
    }
}