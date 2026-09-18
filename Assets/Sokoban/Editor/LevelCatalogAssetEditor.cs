using UnityEditor;
using UnityEngine;

namespace CompoundBox.Editor
{
    [CustomEditor(typeof(LevelCatalogAsset))]
    public sealed class LevelCatalogAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(10f);

            if (GUILayout.Button("Validate Entire Catalog"))
            {
                LevelWorkbenchWindow.ValidateAllLevels();
            }

            if (GUILayout.Button("Open Level Workbench"))
            {
                LevelWorkbenchWindow.Open();
            }

            EditorGUILayout.HelpBox(
                "This catalog is the runtime source of level order. " +
                "Add custom LevelDefinitionAsset entries here and do not " +
                "replace the catalog with a seed-only rebuild.",
                MessageType.Info);
        }
    }
}
