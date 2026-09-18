using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CompoundBox.Editor
{
    public sealed class LevelWorkbenchWindow : EditorWindow
    {
        private Vector2 scroll;
        private LevelCatalogAsset catalog;

        [MenuItem("Tools/Compound Box/Level Workbench")]
        public static void Open()
        {
            var window = GetWindow<LevelWorkbenchWindow>("Level Workbench");
            window.minSize = new Vector2(520f, 420f);
            window.Show();
        }

        [MenuItem("Tools/Compound Box/Validate All Levels")]
        public static void ValidateAllLevels()
        {
            var definitions = BuiltInLevels.All;
            var guids = AssetDatabase.FindAssets("t:LevelCatalogAsset");
            if (guids.Length > 0)
            {
                var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalogAsset>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
                if (catalog != null && catalog.LevelAssets.Count > 0)
                {
                    definitions = catalog.ToDefinitions();
                }
            }

            var errors = 0;
            foreach (var level in definitions)
            {
                var report = LevelValidation.Validate(level);
                errors += report.Errors.Count;
                if (report.IsValid)
                {
                    Debug.Log($"Level '{level.Id}' validated. Solution replay completed.");
                }
                else
                {
                    Debug.LogError($"Level '{level.Id}' failed validation: {string.Join(" | ", report.Errors)}");
                }
            }

            Debug.Log(errors == 0
                ? $"Compound Box validation complete. {definitions.Count} levels passed."
                : $"Compound Box validation found {errors} error(s).");
        }

        private void OnGUI()
        {
            EnsureCatalog();
            EditorGUILayout.LabelField("Compound Box - Level Workbench", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Every level below is parsed through the production LevelParser and replayed through GridSession. " +
                "The editor uses the same rules as runtime, so this is a fast content smoke test.",
                MessageType.Info);

            if (catalog == null)
            {
                EditorGUILayout.HelpBox(
                    "No Level Catalog was found. The Workbench is using BuiltInLevels until a catalog is created.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"Catalog: {AssetDatabase.GetAssetPath(catalog)}", EditorStyles.miniLabel);
            }

            if (GUILayout.Button("Validate Catalog Levels"))
            {
                ValidateAllLevels();
            }

            if (GUILayout.Button("Create New Level Asset"))
            {
                CreateNewLevelAsset();
            }

            EditorGUILayout.Space(8f);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            var definitions = GetDefinitions();
            for (var index = 0; index < definitions.Count; index++)
            {
                var level = definitions[index];
                var report = LevelValidation.Validate(level);
                var audit = LevelAudit.Analyze(level, index);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"{index + 1:00}  {level.DisplayName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(level.Subtitle, EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(
                        $"{audit.ChapterName} | {audit.Width}x{audit.Height} | " +
                        $"entities {audit.MatterEntityCount} | goals {audit.GoalCount} | " +
                        $"solution {audit.SolutionActionCount} | moves {audit.MoveCount} | pushes {audit.PushCount} | " +
                        $"focus {audit.PrimaryMechanic}",
                        EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(
                        report.IsValid ? "VALID" : $"INVALID - {string.Join(" | ", report.Errors)}",
                        EditorStyles.miniLabel);
                    if (audit.Errors.Count > 0)
                    {
                        EditorGUILayout.LabelField(
                            $"AUDIT - {string.Join(" | ", audit.Errors)}",
                            EditorStyles.miniLabel);
                    }

                    if (GUILayout.Button("Play This Level"))
                    {
                        CompoundBoxApp.RequestedStartLevel = index;
                        EditorApplication.EnterPlaymode();
                    }

                    if (catalog != null && index < catalog.LevelAssets.Count &&
                        GUILayout.Button("Select Level Asset"))
                    {
                        Selection.activeObject = catalog.LevelAssets[index];
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void EnsureCatalog()
        {
            if (catalog != null)
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:LevelCatalogAsset");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                catalog = AssetDatabase.LoadAssetAtPath<LevelCatalogAsset>(path);
            }
        }

        private IReadOnlyList<LevelDefinition> GetDefinitions()
        {
            if (catalog != null && catalog.LevelAssets.Count > 0)
            {
                return catalog.ToDefinitions();
            }

            return BuiltInLevels.All;
        }

        private void CreateNewLevelAsset()
        {
            EnsureCatalog();
            if (catalog == null || catalog.LevelAssets.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Catalog required",
                    "Create or refresh the default catalog before creating a custom level.",
                    "OK");
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject(
                "Create Level Definition",
                "new-level",
                "asset",
                "Create a new Compound Box level asset.",
                AssetDatabase.GetAssetPath(catalog).Replace("/CompoundBoxLevelCatalog.asset", "/Levels"));
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<LevelDefinitionAsset>();
            asset.Configure(
                "new-level",
                "New Level",
                "Describe the chamber's teaching goal.",
                "#########\n#.@1.x..#\n#.......#\n#########",
                null,
                "RR");
            AssetDatabase.CreateAsset(asset, path);
            catalog.AddLevelAsset(asset);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
}
