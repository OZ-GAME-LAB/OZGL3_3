public abstract class PlayerStateBase
{
    protected readonly PlayerStateContext Ctx;

    protected PlayerStateBase(PlayerStateContext ctx)
    {
        Ctx = ctx;
    }

    public virtual void StateEnter() { }
    public virtual void StateExit() { }
    public virtual void StateUpdate() { }
    public virtual void StateFixedUpdate() { }
}