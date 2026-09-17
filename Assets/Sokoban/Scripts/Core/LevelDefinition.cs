using System;

namespace CompoundBox
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public LevelDefinition(string id, string displayName, string subtitle, string[] rows, string knownSolution)
        {
            Id = id;
            DisplayName = displayName;
            Subtitle = subtitle;
            Rows = rows;
            KnownSolution = knownSolution;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Subtitle { get; }
        public string[] Rows { get; }
        public string KnownSolution { get; }
    }
}
