using System;
using System.Collections.Generic;

namespace CompoundBox
{
    public interface IStateBehaviour<in TState>
    {
        void Enter(TState state);
        void Exit(TState state);
        void Tick(TState state, float deltaTime);
    }

    public class StateMachine<TState>
    {
        private readonly Dictionary<TState, IStateBehaviour<TState>> behaviours =
            new Dictionary<TState, IStateBehaviour<TState>>();
        private IStateBehaviour<TState> currentBehaviour;

        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        public TState CurrentState { get; private set; }
        public event Action<TState, TState> StateChanged;

        public StateMachine<TState> Add(TState state, IStateBehaviour<TState> behaviour)
        {
            behaviours[state] = behaviour;
            return this;
        }

        public bool Change(TState nextState)
        {
            if (EqualityComparer<TState>.Default.Equals(CurrentState, nextState))
            {
                return false;
            }

            var previous = CurrentState;
            currentBehaviour?.Exit(previous);
            CurrentState = nextState;
            behaviours.TryGetValue(nextState, out currentBehaviour);
            currentBehaviour?.Enter(nextState);
            StateChanged?.Invoke(previous, nextState);
            return true;
        }

        public void Tick(float deltaTime)
        {
            currentBehaviour?.Tick(CurrentState, deltaTime);
        }
    }
}
