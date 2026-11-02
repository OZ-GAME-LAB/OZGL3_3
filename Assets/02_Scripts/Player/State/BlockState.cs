using UnityEngine;

public class BlockState : PlayerStateBase
{
    public BlockState(PlayerStateContext playerContext) : base(playerContext) { }

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
        PlayerContext.Input.ClearJumpInput();
        PlayerContext.Rigidbody.linearVelocity = new Vector2(0f, PlayerContext.Rigidbody.linearVelocity.y);
    }

    private void SetBlocking(bool isBlocking)
    {
        PlayerContext.Animator.SetBool("IsBlocking", isBlocking);

        if (PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.SetBlocking(isBlocking);
        }
    }
}