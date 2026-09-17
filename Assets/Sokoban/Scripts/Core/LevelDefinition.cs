using System;

namespace CompoundBox
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public LevelDefinition(string id, string displayName, string subtitle, string[] rows, string knownSolution)
            : this(id, displayName, subtitle, rows, null, knownSolution)
        {
        }

        public LevelDefinition(
            string id,
            string displayName,
            string subtitle,
            string[] rows,
            string[] materialRows,
            string knownSolution)
        {
            Id = id;
            DisplayName = displayName;
            Subtitle = subtitle;
            Rows = rows;
            MaterialRows = materialRows;
            KnownSolution = knownSolution;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Subtitle { get; }
        public string[] Rows { get; }
        public string[] MaterialRows { get; }
        public string KnownSolution { get; }
    }
}
