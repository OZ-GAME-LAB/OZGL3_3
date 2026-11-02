using UnityEngine;

public abstract class GroundMoveState : PlayerStateBase
{
    protected GroundMoveState(PlayerStateContext playerContext) : base(playerContext) { }

    protected abstract float GetTargetSpeed();

    public override void StateFixedUpdate()
    {
        PlayerContext.Rigidbody.linearVelocity = new Vector2(
            GetTargetSpeed(),
            PlayerContext.Rigidbody.linearVelocity.y);
    }
}