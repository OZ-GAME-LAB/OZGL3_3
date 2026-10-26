using System.Collections.Generic;
using UnityEngine;

public class LadderState : PlayerStateBase
{
    private const float LadderJumpSpeedX = 3f;

    private readonly LayerMask _ladderLayerMask;
    private readonly HashSet<Collider2D> _triggers = new HashSet<Collider2D>();

    private Collider2D _currentCollider;

    private readonly float _coolDown;

    private float _originalGravityScale;
    private float _coolDownEndTime;

    public bool IsClimbing { get; private set; }
    public bool IsNearLadder { get; private set; }
    public float Direction { get; private set; } = 1f;

    public LadderState(
        PlayerStateContext ctx,
        LayerMask ladderLayerMask,
        float coolDown
        ): base(ctx)
    {
        _ladderLayerMask = ladderLayerMask;
        _coolDown = coolDown;
    }

    public void OnTriggerEnter(Collider2D other)
    {
        if (!other.isTrigger || !IsLadderLayer(other.gameObject.layer))
        {
            return;
        }

        _triggers.Add(other);
        RefreshCurrentCollider();
        IsNearLadder = _triggers.Count > 0;
    }

    public void OnTriggerExit(Collider2D other)
    {
        _triggers.Remove(other);
        RefreshCurrentCollider();
        IsNearLadder = _triggers.Count > 0;

        if (!IsNearLadder && IsClimbing)
        {
            Release();
        }
    }

    public void TryBegin()
    {
        if (IsClimbing || !IsNearLadder || Time.time < _coolDownEndTime)
        {
            return;
        }

        bool upPressed = Ctx.Input.UpInput;
        bool downPressed = Ctx.Input.DownInput;

        bool hasLadderInput = Ctx.IsGrounded
            ? upPressed != downPressed
            : upPressed && !downPressed;

        if (!hasLadderInput)
        {
            return;
        }

        IsClimbing = true;

        if (_currentCollider != null)
        {
            Vector2 position = Ctx.Rigidbody.position;
            position.x = _currentCollider.bounds.center.x;
            Ctx.Rigidbody.position = position;
        }

        _originalGravityScale = Ctx.Rigidbody.gravityScale;
        Ctx.Rigidbody.gravityScale = 0f;
        Ctx.Rigidbody.linearVelocity = Vector2.zero;
        Direction = upPressed ? 1f : -1f;

        Ctx.Input.ClearJumpInput();
        Ctx.Animator.SetBool("IsLadder", true);
    }

    public override void StateExit()
    {
        if (IsClimbing)
        {
            Release();
        }
    }

    public override void StateFixedUpdate()
    {
        float horizontalInput = Ctx.Input.MoveInput;
        bool isTryingLadderJump =
            Ctx.Input.JumpInput &&
            Mathf.Abs(horizontalInput) > 0.01f;

        Ctx.Input.ClearJumpInput();

        if (isTryingLadderJump)
        {
            JumpOutLadder(horizontalInput);
            return;
        }

        float verticalInput =
            (Ctx.Input.UpInput ? 1f : 0f) -
            (Ctx.Input.DownInput ? 1f : 0f);

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            Direction = Mathf.Sign(verticalInput);

            Ctx.Rigidbody.linearVelocity = new Vector2(
                0f,
                verticalInput * PlayerStatSet.Instance.LadderSpeed);

            Ctx.Animator.SetFloat("LadderAnimSpeed", 1f);
        }
        else
        {
            Ctx.Rigidbody.linearVelocity = Vector2.zero;
            Ctx.Animator.SetFloat("LadderAnimSpeed", 0f);
        }

        Ctx.Animator.SetFloat("LadderDirection", Direction);
    }

    private void JumpOutLadder(float horizontalInput)
    {
        float jumpDirection = Mathf.Sign(horizontalInput);

        IsClimbing = false;
        Ctx.Rigidbody.gravityScale = _originalGravityScale;
        _coolDownEndTime = Time.time + _coolDown;

        Ctx.Rigidbody.linearVelocity = new Vector2(
            jumpDirection * LadderJumpSpeedX,
            PlayerStatSet.Instance.JumpValue);

        Ctx.SpriteRenderer.flipX = jumpDirection < 0f;
        Ctx.IsGrounded = false;

        Ctx.Animator.SetFloat("LadderAnimSpeed", 0f);
        Ctx.Animator.SetBool("IsLadder", false);
        Ctx.Input.ClearJumpInput();
    }

    private void Release()
    {
        IsClimbing = false;
        Ctx.Rigidbody.gravityScale = _originalGravityScale;
        Ctx.Rigidbody.linearVelocity = new Vector2(Ctx.Rigidbody.linearVelocity.x, 0f);

        Ctx.Animator.SetFloat("LadderAnimSpeed", 0f);
        Ctx.Animator.SetBool("IsLadder", false);
    }

    private void RefreshCurrentCollider()
    {
        _currentCollider = null;
        float nearestDistance = float.PositiveInfinity;

        _triggers.RemoveWhere(c => c == null);

        foreach (Collider2D ladderCollider in _triggers)
        {
            Vector2 offset = (Vector2)ladderCollider.bounds.center - Ctx.Rigidbody.position;
            float distance = offset.sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                _currentCollider = ladderCollider;
            }
        }
    }

    private bool IsLadderLayer(int layer)
    {
        return (_ladderLayerMask.value & (1 << layer)) != 0;
    }
}