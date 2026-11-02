public class DiedState : PlayerStateBase
{
    public DiedState(PlayerStateContext ctx) : base(ctx) { }

    public override void StateEnter()
    {
        Ctx.Input.ClearInput();
        Ctx.Rigidbody.linearVelocity = UnityEngine.Vector2.zero;
        Ctx.Animator.SetBool("ComboAttackSign", false);
        Ctx.Animator.ResetTrigger("JumpAttack");
        Ctx.Animator.SetBool("IsDead", true);
    }
}