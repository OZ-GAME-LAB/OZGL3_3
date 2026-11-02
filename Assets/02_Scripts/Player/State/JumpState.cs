using UnityEngine;

public class JumpState : PlayerStateBase
{
    public JumpState(PlayerStateContext playerContext) : base(playerContext) { }

    public override void StateFixedUpdate()
    {
        PlayerContext.TryJump();

        float horizontalSpeed = PlayerContext.Rigidbody.linearVelocity.x;

        if (Mathf.Abs(PlayerContext.Input.MoveInput) > 0.01f)
        {
            float moveSpeed = PlayerContext.Input.RunInput
                ? PlayerStatSet.Instance.RunSpeed
                : PlayerStatSet.Instance.WalkSpeed;

            horizontalSpeed = PlayerContext.Input.MoveInput * moveSpeed;
        }

        PlayerContext.Rigidbody.linearVelocity = new Vector2(
            horizontalSpeed,
            PlayerContext.Rigidbody.linearVelocity.y);
    }
}