using System.Collections.Generic;

namespace CompoundBox
{
    public sealed class LevelValidationReport
    {
        public LevelValidationReport(string levelId)
        {
            LevelId = levelId;
            Errors = new List<string>();
            Warnings = new List<string>();
        }

        public string LevelId { get; }
        public List<string> Errors { get; }
        public List<string> Warnings { get; }
        public bool IsValid => Errors.Count == 0;
    }

    public static class LevelValidation
    {
        public static LevelValidationReport Validate(LevelDefinition definition)
        {
            var report = new LevelValidationReport(definition.Id);
            GridBoardState state;
            try
            {
                state = LevelParser.Parse(definition);
            }
            catch (System.Exception exception)
            {
                report.Errors.Add(exception.Message);
                return report;
            }

            if (state.PlayerId < 0)
            {
                report.Errors.Add("The level has no player spawn.");
            }

            if (state.Goals.Count == 0 && !state.HasExit)
            {
                report.Errors.Add("The level has neither a goal nor an exit.");
            }

            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    var cell = new UnityEngine.Vector2Int(x, y);
                    var tile = state.GetTile(cell);
                    if (tile == TileKind.Goal && state.FindEntityAt(cell) != null)
                    {
                        report.Warnings.Add($"A payload starts on a goal at {cell}.");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(definition.KnownSolution))
            {
                var replay = new GridSession(state).ReplaySolution(definition);
                if (!replay.Success)
                {
                    report.Errors.Add(replay.Message);
                }
            }

            return report;
        }
    }
}
