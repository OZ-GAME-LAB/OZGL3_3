using UnityEngine;

public class JumpAttackState : PlayerStateBase
{
    public JumpAttackState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateFixedUpdate()
    {
        Ctx.TryJump();

        float horizontalSpeed = Ctx.Rigidbody.linearVelocity.x;

        if (Mathf.Abs(Ctx.Input.MoveInput) > 0.01f)
        {
            float moveSpeed = Ctx.Input.RunInput
                ? PlayerStatSet.Instance.RunSpeed
                : PlayerStatSet.Instance.WalkSpeed;

            horizontalSpeed = Ctx.Input.MoveInput * moveSpeed;
        }

        Ctx.Rigidbody.linearVelocity = new Vector2(
            horizontalSpeed,
            Ctx.Rigidbody.linearVelocity.y);
    }
}