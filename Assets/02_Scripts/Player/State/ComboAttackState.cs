using UnityEngine;

public class ComboAttackState : PlayerStateBase
{
    public ComboAttackState(PlayerStateContext playerContext) : base(playerContext) { }

    public override void StateFixedUpdate()
    {
        PlayerContext.Input.ClearJumpInput();
        PlayerContext.Rigidbody.linearVelocity = new Vector2(0f, PlayerContext.Rigidbody.linearVelocity.y);
    }
}