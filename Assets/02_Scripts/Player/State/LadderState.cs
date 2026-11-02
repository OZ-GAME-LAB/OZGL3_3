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
        PlayerStateContext playerContext,
        LayerMask ladderLayerMask,
        float coolDown
        ): base(playerContext)
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

        bool upPressed = PlayerContext.Input.UpInput;
        bool downPressed = PlayerContext.Input.DownInput;

        bool hasLadderInput = PlayerContext.IsGrounded
            ? upPressed != downPressed
            : upPressed && !downPressed;

        if (!hasLadderInput)
        {
            return;
        }

        if (downPressed &&
            !upPressed &&
            _currentCollider != null &&
            _currentCollider.TryGetComponent(out LadderZone ladderZone))
        {
            ladderZone.DropThrough();
        }

        IsClimbing = true;

        if (_currentCollider != null)
        {
            Vector2 position = PlayerContext.Rigidbody.position;
            position.x = _currentCollider.bounds.center.x;
            PlayerContext.Rigidbody.position = position;
        }

        _originalGravityScale = PlayerContext.Rigidbody.gravityScale;
        PlayerContext.Rigidbody.gravityScale = 0f;
        PlayerContext.Rigidbody.linearVelocity = Vector2.zero;
        Direction = upPressed ? 1f : -1f;

        PlayerContext.Input.ClearJumpInput();
        PlayerContext.Animator.SetBool("IsLadder", true);
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
        float horizontalInput = PlayerContext.Input.MoveInput;
        bool isTryingLadderJump =
            PlayerContext.Input.JumpQueued &&
            Mathf.Abs(horizontalInput) > 0.01f;

        PlayerContext.Input.ClearJumpInput();

        if (isTryingLadderJump)
        {
            JumpOutLadder(horizontalInput);
            return;
        }

        float verticalInput =
            (PlayerContext.Input.UpInput ? 1f : 0f) -
            (PlayerContext.Input.DownInput ? 1f : 0f);

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            Direction = Mathf.Sign(verticalInput);

            PlayerContext.Rigidbody.linearVelocity = new Vector2(
                0f,
                verticalInput * PlayerStatSet.Instance.LadderSpeed);

            PlayerContext.Animator.SetFloat("LadderAnimSpeed", 1f);
        }
        else
        {
            PlayerContext.Rigidbody.linearVelocity = Vector2.zero;
            PlayerContext.Animator.SetFloat("LadderAnimSpeed", 0f);
        }

        PlayerContext.Animator.SetFloat("LadderDirection", Direction);
    }

    private void JumpOutLadder(float horizontalInput)
    {
        float jumpDirection = Mathf.Sign(horizontalInput);

        IsClimbing = false;
        PlayerContext.Rigidbody.gravityScale = _originalGravityScale;
        _coolDownEndTime = Time.time + _coolDown;

        PlayerContext.Rigidbody.linearVelocity = new Vector2(
            jumpDirection * LadderJumpSpeedX,
            PlayerStatSet.Instance.JumpValue);

        PlayerContext.SpriteRenderer.flipX = jumpDirection < 0f;
        PlayerContext.IsGrounded = false;

        PlayerContext.Animator.SetFloat("LadderAnimSpeed", 0f);
        PlayerContext.Animator.SetBool("IsLadder", false);
        PlayerContext.Input.ClearJumpInput();
    }

    private void Release()
    {
        IsClimbing = false;
        PlayerContext.Rigidbody.gravityScale = _originalGravityScale;
        PlayerContext.Rigidbody.linearVelocity = new Vector2(PlayerContext.Rigidbody.linearVelocity.x, 0f);

        PlayerContext.Animator.SetFloat("LadderAnimSpeed", 0f);
        PlayerContext.Animator.SetBool("IsLadder", false);
    }

    private void RefreshCurrentCollider()
    {
        _currentCollider = null;
        float nearestDistance = float.PositiveInfinity;

        _triggers.RemoveWhere(c => c == null);

        foreach (Collider2D ladderCollider in _triggers)
        {
            Vector2 offset = (Vector2)ladderCollider.bounds.center - PlayerContext.Rigidbody.position;
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