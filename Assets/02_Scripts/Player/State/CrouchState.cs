using UnityEngine;

public class CrouchState : GroundMoveState
{
    private readonly CapsuleCollider2D _collider;
    private readonly Vector2 _standingColliderOffset;
    private readonly Vector2 _standingColliderSize;
    private readonly float _crouchColliderOffsetY;
    private readonly float _crouchColliderHeight;

    public CrouchState(
        PlayerStateContext playerContext,
        CapsuleCollider2D collider,
        float crouchColliderOffsetY,
        float crouchColliderHeight) : base(playerContext)
    {
        _collider = collider;
        _standingColliderOffset = collider.offset;
        _standingColliderSize = collider.size;
        _crouchColliderOffsetY = crouchColliderOffsetY;
        _crouchColliderHeight = crouchColliderHeight;
    }

    public override void StateEnter()
    {
        PlayerContext.Rigidbody.linearVelocity = new Vector2(
            0f,
            PlayerContext.Rigidbody.linearVelocity.y);
        _collider.offset = new Vector2(_standingColliderOffset.x, _crouchColliderOffsetY);
        _collider.size = new Vector2(_standingColliderSize.x, _crouchColliderHeight);
    }

    public override void StateExit()
    {
        PlayerContext.Visual?.SetOffsetY(0f);
        _collider.offset = _standingColliderOffset;
        _collider.size = _standingColliderSize;
    }

    protected override float GetTargetSpeed()
    {
        return 0f;
    }

    public override void StateFixedUpdate()
    {
        PlayerContext.Input.ClearJumpInput();
        base.StateFixedUpdate();
    }
}