using System.Collections.Generic;

namespace CompoundBox
{
    public sealed class GridSession
    {
        private const int HistoryLimit = 120;
        private readonly GridBoardState initialState;
        private readonly List<IGridCommand> undoCommands = new List<IGridCommand>();
        private readonly List<IGridCommand> redoCommands = new List<IGridCommand>();

        public GridSession(GridBoardState board)
        {
            initialState = board.Clone();
            State = board.Clone();
        }

        public GridBoardState State { get; private set; }
        public bool CanUndo => undoCommands.Count > 0;
        public bool CanRedo => redoCommands.Count > 0;
        public int UndoDepth => undoCommands.Count;
        public int RedoDepth => redoCommands.Count;

        public IReadOnlyList<string> UndoHistory
        {
            get
            {
                var labels = new List<string>(undoCommands.Count);
                for (var i = 0; i < undoCommands.Count; i++)
                {
                    labels.Add(undoCommands[i].HistoryLabel);
                }

                return labels;
            }
        }

        public ActionResolution Move(GridDirection direction)
        {
            return ExecuteCommand(new MoveGridCommand(direction));
        }

        public ActionResolution SplitFacingEntity()
        {
            return ExecuteCommand(new SplitGridCommand());
        }

        public ActionResolution RecombineAdjacentMatter()
        {
            return ExecuteCommand(new RecombineGridCommand());
        }

        public ActionResolution Undo()
        {
            if (!CanUndo)
            {
                return new ActionResolution(false, GridActionType.Undo, "Nothing to undo.");
            }

            var command = undoCommands[undoCommands.Count - 1];
            undoCommands.RemoveAt(undoCommands.Count - 1);
            var result = command.Undo(State);
            if (!result.Success)
            {
                undoCommands.Add(command);
                return result;
            }

            AddBounded(redoCommands, command);
            return new ActionResolution(
                true,
                GridActionType.Undo,
                result.Message,
                result.ChangedEntityIds,
                result.UsedPortal,
                result.PushedEntities);
        }

        public ActionResolution Redo()
        {
            if (!CanRedo)
            {
                return new ActionResolution(false, GridActionType.Redo, "Nothing to redo.");
            }

            var command = redoCommands[redoCommands.Count - 1];
            redoCommands.RemoveAt(redoCommands.Count - 1);
            var result = command.Redo(State);
            if (!result.Success)
            {
                redoCommands.Add(command);
                return result;
            }

            AddBounded(undoCommands, command);
            return new ActionResolution(
                true,
                GridActionType.Redo,
                result.Message,
                result.ChangedEntityIds,
                result.UsedPortal,
                result.PushedEntities);
        }

        public ActionResolution Restart()
        {
            State.RestoreFrom(initialState);
            undoCommands.Clear();
            redoCommands.Clear();
            return new ActionResolution(true, GridActionType.Restart, "Level restarted.");
        }

        public ActionResolution ReplaySolution(LevelDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.KnownSolution))
            {
                return new ActionResolution(false, GridActionType.Move, "No known solution was supplied.");
            }

            var solution = definition.KnownSolution.Trim().ToUpperInvariant();
            for (var i = 0; i < solution.Length; i++)
            {
                var code = solution[i];
                if (char.IsWhiteSpace(code) || code == '-')
                {
                    continue;
                }

                ActionResolution resolution;
                if (GridDirectionUtility.TryParseSolutionCode(code, out var direction))
                {
                    resolution = Move(direction);
                }
                else if (code == 'S')
                {
                    resolution = SplitFacingEntity();
                }
                else if (code == 'C')
                {
                    resolution = RecombineAdjacentMatter();
                }
                else
                {
                    return new ActionResolution(
                        false,
                        GridActionType.Move,
                        $"Unsupported solution code '{code}' at index {i}.");
                }

                if (!resolution.Success)
                {
                    return new ActionResolution(
                        false,
                        resolution.ActionType,
                        $"Known solution failed at '{code}' ({i + 1}/{solution.Length}): {resolution.Message}");
                }
            }

            return new ActionResolution(
                State.IsSolved(),
                GridActionType.Move,
                State.IsSolved() ? "Known solution completed the level." : "Known solution ended before solving the level.");
        }

        private ActionResolution ExecuteCommand(IGridCommand command)
        {
            var result = command.Execute(State);
            if (!result.Success)
            {
                return result;
            }

            AddBounded(undoCommands, command);
            redoCommands.Clear();
            return result;
        }

        private static void AddBounded(List<IGridCommand> commands, IGridCommand command)
        {
            commands.Add(command);
            if (commands.Count > HistoryLimit)
            {
                commands.RemoveAt(0);
            }
        }
    }
}
