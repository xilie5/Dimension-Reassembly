using UnityEditor;
using UnityEngine;

namespace CompoundBox.Editor
{
    public sealed class LevelWorkbenchWindow : EditorWindow
    {
        private Vector2 scroll;

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
            var errors = 0;
            foreach (var level in BuiltInLevels.All)
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
                ? $"Compound Box validation complete. {BuiltInLevels.All.Count} levels passed."
                : $"Compound Box validation found {errors} error(s).");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Compound Box - Level Workbench", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Every level below is parsed through the production LevelParser and replayed through GridSession. " +
                "The editor uses the same rules as runtime, so this is a fast content smoke test.",
                MessageType.Info);

            if (GUILayout.Button("Validate All Levels"))
            {
                ValidateAllLevels();
            }

            EditorGUILayout.Space(8f);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (var index = 0; index < BuiltInLevels.All.Count; index++)
            {
                var level = BuiltInLevels.All[index];
                var report = LevelValidation.Validate(level);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"{index + 1:00}  {level.DisplayName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(level.Subtitle, EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(
                        report.IsValid ? "VALID" : $"INVALID - {string.Join(" | ", report.Errors)}",
                        EditorStyles.miniLabel);

                    if (GUILayout.Button("Play This Level"))
                    {
                        CompoundBoxApp.RequestedStartLevel = index;
                        EditorApplication.EnterPlaymode();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
