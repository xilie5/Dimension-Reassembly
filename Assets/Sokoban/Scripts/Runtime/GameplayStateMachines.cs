namespace CompoundBox
{
    public enum LevelFlowState
    {
        Loading,
        Playing,
        Completed
    }

    public enum PlayerActionState
    {
        Idle,
        Moving,
        Interacting
    }

    public enum MechanismState
    {
        Closed,
        Open,
        Locked
    }

    public sealed class LevelFlowStateMachine : StateMachine<LevelFlowState>
    {
        public LevelFlowStateMachine()
            : base(LevelFlowState.Loading)
        {
        }
    }

    public sealed class PlayerActionStateMachine : StateMachine<PlayerActionState>
    {
        public PlayerActionStateMachine()
            : base(PlayerActionState.Idle)
        {
        }
    }
}
