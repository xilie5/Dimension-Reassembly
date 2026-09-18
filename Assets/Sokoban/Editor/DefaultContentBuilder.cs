using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CompoundBox.Editor
{
    public static class DefaultContentBuilder
    {
        private const string DataRoot = "Assets/Sokoban/Data";
        private const string LevelsRoot = DataRoot + "/Levels";
        private const string CatalogPath = DataRoot + "/CompoundBoxLevelCatalog.asset";

        [MenuItem("Tools/Compound Box/Create or Refresh Default Catalog")]
        public static void CreateOrRefreshDefaultCatalog()
        {
            Directory.CreateDirectory(DataRoot);
            Directory.CreateDirectory(LevelsRoot);

            var levelAssets = new List<LevelDefinitionAsset>();
            for (var index = 0; index < BuiltInLevels.All.Count; index++)
            {
                var definition = BuiltInLevels.All[index];
                var path = $"{LevelsRoot}/{index + 1:00}-{definition.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<LevelDefinitionAsset>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<LevelDefinitionAsset>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.Configure(
                    definition.Id,
                    definition.DisplayName,
                    definition.Subtitle,
                    string.Join("\n", definition.Rows),
                    definition.MaterialRows == null ? null : string.Join("\n", definition.MaterialRows),
                    definition.KnownSolution);
                EditorUtility.SetDirty(asset);
                levelAssets.Add(asset);
            }

            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalogAsset>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalogAsset>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            if (catalog.LevelAssets.Count > 0)
            {
                var builtInIds = new HashSet<string>(levelAssets.Select(asset => asset.LevelId));
                foreach (var customAsset in catalog.LevelAssets)
                {
                    if (customAsset != null && !builtInIds.Contains(customAsset.LevelId))
                    {
                        levelAssets.Add(customAsset);
                    }
                }
            }

            catalog.SetLevels(levelAssets);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            Debug.Log($"Compound Box default catalog refreshed with {levelAssets.Count} levels.");
        }

        [MenuItem("Tools/Compound Box/Reset Local Save")]
        public static void ResetLocalSave()
        {
            SaveService.Reset();
            Debug.Log($"Compound Box save reset at {SaveService.SavePath}");
        }
    }
}
