using UnityEngine;

namespace CompoundBox
{
    [CreateAssetMenu(fileName = "Level", menuName = "Compound Box/Level Definition")]
    public sealed class LevelDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string levelId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string subtitle;
        [SerializeField, TextArea(6, 16)] private string layout;
        [SerializeField] private string knownSolution;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string Subtitle => subtitle;
        public string KnownSolution => knownSolution;

        public LevelDefinition ToDefinition()
        {
            var rows = layout
                .Replace("\r", string.Empty)
                .Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            return new LevelDefinition(levelId, displayName, subtitle, rows, knownSolution);
        }

        public void Configure(string id, string title, string description, string levelLayout, string solution)
        {
            levelId = id;
            displayName = title;
            subtitle = description;
            layout = levelLayout;
            knownSolution = solution;
        }
    }
}
