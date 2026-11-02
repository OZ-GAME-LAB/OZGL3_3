using UnityEngine;

public class PlayerStateContext
{
    public Rigidbody2D Rigidbody { get; }
    public Animator Animator { get; }
    public SpriteRenderer SpriteRenderer { get; }
    public PlayerInputHandler Input { get; }
    public PlayerVisualOffset Visual { get; }

    public float Facing => SpriteRenderer.flipX ? -1f : 1f;

    public bool IsGrounded { get; set; }
    
    public int AirDashesUsed { get; set; }
    public int JumpAttacksUsed { get; set; }


    public PlayerStateContext(
        Rigidbody2D rigidbody,
        Animator animator,
        SpriteRenderer spriteRenderer,
        PlayerInputHandler input,
        PlayerVisualOffset visual)
    {
        Rigidbody = rigidbody;
        Animator = animator;
        SpriteRenderer = spriteRenderer;
        Input = input;
        Visual = visual;
    }
    public void ResetAirActions()
    {
        JumpAttacksUsed = 0;
        AirDashesUsed = 0;
    }
}