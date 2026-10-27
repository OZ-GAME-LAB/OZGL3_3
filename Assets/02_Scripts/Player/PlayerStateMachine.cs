using System.Collections.Generic;

public class PlayerStateMachine
{
    public PlayerStateBase CurrentState { get; private set; }
    public PlayerState CurrentType { get; private set; }

    private readonly Dictionary<PlayerState, PlayerStateBase> _states =
        new Dictionary<PlayerState, PlayerStateBase>();

    public void RegisterState(PlayerState type, PlayerStateBase state)
    {
        _states[type] = state;
    }

    public void ChangeStateSign(PlayerState next)
    {
        if (CurrentState != null && CurrentType == next)
        {
            return;
        }

        if (!_states.TryGetValue(next, out PlayerStateBase nextState))
        {
            Logger.LogError($"등록되지 않은 상태입니다. ({next})");
            return;
        }

        CurrentState?.StateExit();
        CurrentType = next;
        CurrentState = nextState;
        CurrentState.StateEnter();
    }

    public void StateFixedUpdateSign()
    {
        CurrentState?.StateFixedUpdate();
    }
}