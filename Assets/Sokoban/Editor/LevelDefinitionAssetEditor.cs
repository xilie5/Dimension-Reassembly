using UnityEditor;
using UnityEngine;

namespace CompoundBox.Editor
{
    [CustomEditor(typeof(LevelDefinitionAsset))]
    public sealed class LevelDefinitionAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(10f);

            var asset = (LevelDefinitionAsset)target;
            if (GUILayout.Button("Validate Level"))
            {
                var report = LevelValidation.Validate(asset.ToDefinition());
                if (report.IsValid)
                {
                    Debug.Log($"Level '{asset.LevelId}' is valid. Solution replay passed.");
                }
                else
                {
                    Debug.LogError(
                        $"Level '{asset.LevelId}' is invalid: {string.Join(" | ", report.Errors)}");
                }
            }

            if (GUILayout.Button("Open Level Workbench"))
            {
                LevelWorkbenchWindow.Open();
            }

            EditorGUILayout.HelpBox(
                "Use layout for terrain, objects, portals and goals. " +
                "Use materialLayout when one compound contains multiple materials. " +
                "KnownSolution accepts U/R/D/L, S, V, C, Q and E.",
                MessageType.Info);
        }
    }
}
