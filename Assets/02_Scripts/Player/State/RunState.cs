public class RunState : GroundMoveState
{
    public RunState(PlayerStateContext playerContext) : base(playerContext) { }

    protected override float GetTargetSpeed()
    {
        return PlayerContext.Input.MoveInput * PlayerStatSet.Instance.RunSpeed;
    }
}