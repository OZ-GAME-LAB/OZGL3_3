using UnityEngine;

public class JumpAttackState : PlayerStateBase
{
    public JumpAttackState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        Ctx.Input.ClearJumpInput();
        Ctx.Rigidbody.linearVelocity = new Vector2(
            Ctx.Input.MoveInput * PlayerStatSet.Instance.WalkSpeed,
            Ctx.Rigidbody.linearVelocity.y);
    }
}