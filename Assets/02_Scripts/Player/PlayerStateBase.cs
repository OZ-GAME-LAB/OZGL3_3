public abstract class PlayerStateBase
{
    protected readonly PlayerStateContext PlayerContext;

    protected PlayerStateBase(PlayerStateContext playerContext)
    {
        PlayerContext = playerContext;
    }

    public virtual void StateEnter() { }
    public virtual void StateExit() { }
    public virtual void StateUpdate() { }
    public virtual void StateFixedUpdate() { }
}