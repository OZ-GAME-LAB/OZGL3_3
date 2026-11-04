public class WalkState : GroundMoveState
{
    public WalkState(PlayerStateContext playerContext) : base(playerContext) { }

    protected override float GetTargetSpeed()
    {
        return PlayerContext.Input.MoveInput * PlayerStatSet.Instance.WalkSpeed;
    }
}