public class DiedState : PlayerStateBase
{
    public DiedState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateEnter()
    {
        PlayerContext.Input.ClearInput();
        PlayerContext.Rigidbody.linearVelocity = UnityEngine.Vector2.zero;
        PlayerContext.Animator.SetBool("ComboAttackSign", false);
        PlayerContext.Animator.ResetTrigger("JumpAttack");
        PlayerContext.Animator.SetBool("IsDead", true);
    }
}