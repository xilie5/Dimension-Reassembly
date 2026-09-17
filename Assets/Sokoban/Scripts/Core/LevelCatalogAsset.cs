using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    [CreateAssetMenu(fileName = "Level Catalog", menuName = "Compound Box/Level Catalog")]
    public sealed class LevelCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<LevelDefinitionAsset> levels = new List<LevelDefinitionAsset>();

        public IReadOnlyList<LevelDefinitionAsset> LevelAssets => levels;

        public List<LevelDefinition> ToDefinitions()
        {
            var definitions = new List<LevelDefinition>(levels.Count);
            for (var i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null)
                {
                    definitions.Add(levels[i].ToDefinition());
                }
            }

            return definitions;
        }

        public void SetLevels(List<LevelDefinitionAsset> levelAssets)
        {
            levels = levelAssets ?? new List<LevelDefinitionAsset>();
        }
    }
}
