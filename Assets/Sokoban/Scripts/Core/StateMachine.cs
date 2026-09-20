using System;
using System.Collections.Generic;

namespace CompoundBox
{
    public class StateMachine<TState>
    {
        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        public TState CurrentState { get; private set; }
        public event Action<TState, TState> StateChanged;

        public bool Change(TState nextState)
        {
            if (EqualityComparer<TState>.Default.Equals(CurrentState, nextState))
            {
                return false;
            }

            var previous = CurrentState;
            CurrentState = nextState;
            StateChanged?.Invoke(previous, nextState);
            return true;
        }
    }
}
