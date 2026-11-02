using UnityEngine;

public class HealState : PlayerStateBase
{
    public HealState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        PlayerContext.Input.ClearJumpInput();
        PlayerContext.Rigidbody.linearVelocity = new Vector2(0f, PlayerContext.Rigidbody.linearVelocity.y);
    }
}
