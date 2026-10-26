using UnityEngine;

public class BlockState : PlayerStateBase
{
    public BlockState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateEnter()
    {
        SetBlocking(true);
    }

    public override void StateExit()
    {
        SetBlocking(false);
    }

    public override void StateFixedUpdate()
    {
        Ctx.Input.ClearJumpInput();
        Ctx.Rigidbody.linearVelocity = new Vector2(0f, Ctx.Rigidbody.linearVelocity.y);
    }

    private void SetBlocking(bool isBlocking)
    {
        Ctx.Animator.SetBool("IsBlocking", isBlocking);

        if (PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.SetBlocking(isBlocking);
        }
    }
}