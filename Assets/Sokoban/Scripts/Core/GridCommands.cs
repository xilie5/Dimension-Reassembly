using System;
using UnityEngine;

namespace CompoundBox
{
    public interface IGridCommand
    {
        GridActionType ActionType { get; }
        string HistoryLabel { get; }
        ActionResolution Execute(GridBoardState state);
        ActionResolution Undo(GridBoardState state);
        ActionResolution Redo(GridBoardState state);
    }

    public abstract class GridCommand : IGridCommand
    {
        private GridBoardState before;
        private GridBoardState after;

        public abstract GridActionType ActionType { get; }
        public abstract string HistoryLabel { get; }

        public ActionResolution Execute(GridBoardState state)
        {
            before = state.Clone();
            var result = Apply(state);
            if (result.Success)
            {
                after = state.Clone();
            }

            return result;
        }

        public ActionResolution Undo(GridBoardState state)
        {
            if (before == null)
            {
                return new ActionResolution(false, ActionType, "Command has no undo state.");
            }

            state.RestoreFrom(before);
            return new ActionResolution(true, ActionType, $"Undo: {HistoryLabel}");
        }

        public ActionResolution Redo(GridBoardState state)
        {
            if (after == null)
            {
                return new ActionResolution(false, ActionType, "Command has no redo state.");
            }

            state.RestoreFrom(after);
            return new ActionResolution(true, ActionType, $"Redo: {HistoryLabel}");
        }

        protected abstract ActionResolution Apply(GridBoardState state);
    }

    public sealed class MoveGridCommand : GridCommand
    {
        private readonly GridDirection direction;

        public MoveGridCommand(GridDirection direction)
        {
            this.direction = direction;
        }

        public override GridActionType ActionType => GridActionType.Move;
        public override string HistoryLabel => $"Move {GridDirectionUtility.ToSolutionCode(direction)}";

        protected override ActionResolution Apply(GridBoardState state)
        {
            return GridBoardSimulation.Move(state, direction);
        }
    }

    public sealed class SplitGridCommand : GridCommand
    {
        public override GridActionType ActionType => GridActionType.Split;
        public override string HistoryLabel => "Split";

        protected override ActionResolution Apply(GridBoardState state)
        {
            return GridBoardSimulation.SplitFacingEntity(state);
        }
    }

    public sealed class RecombineGridCommand : GridCommand
    {
        public override GridActionType ActionType => GridActionType.Recombine;
        public override string HistoryLabel => "Recombine";

        protected override ActionResolution Apply(GridBoardState state)
        {
            return GridBoardSimulation.RecombineAdjacentMatter(state);
        }
    }
}
