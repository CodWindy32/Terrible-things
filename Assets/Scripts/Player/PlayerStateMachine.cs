using System;
using UnityEngine;

public enum PlayerState
{
    Normal,
    IdleBreathing,
    Climbing,
    FallImpact,
    InUI
}

public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField] private PlayerState initialState = PlayerState.Normal;

    private PlayerState currentState;

    public PlayerState CurrentState => currentState;

    public event Action<PlayerState, PlayerState> OnStateChanged;

    private void Awake()
    {
        currentState = initialState;
    }

    public bool TryEnterState(PlayerState newState)
    {
        if (!CanTransitionTo(newState))
            return false;

        PlayerState oldState = currentState;
        currentState = newState;
        OnStateChanged?.Invoke(oldState, newState);
        return true;
    }

    public bool IsInState(PlayerState state) => currentState == state;

    public bool HasControl => currentState is PlayerState.Normal or PlayerState.IdleBreathing or PlayerState.Climbing;
    public bool HasMovement => currentState is PlayerState.Normal or PlayerState.IdleBreathing;
    public bool HasLook => HasControl;
    public bool CanOpenUI => HasMovement;

    private bool CanTransitionTo(PlayerState target)
    {
        switch (currentState)
        {
            case PlayerState.Normal:
                return target == PlayerState.IdleBreathing || target == PlayerState.Climbing || target == PlayerState.FallImpact || target == PlayerState.InUI;
            case PlayerState.IdleBreathing:
                return target == PlayerState.Normal || target == PlayerState.Climbing || target == PlayerState.FallImpact || target == PlayerState.InUI;
            case PlayerState.Climbing:
            case PlayerState.FallImpact:
                return target == PlayerState.Normal;
            case PlayerState.InUI:
                return target == PlayerState.Normal;
            default:
                return false;
        }
    }
}
