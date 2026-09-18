using System.IO;
using UnityEditor;
using UnityEngine;

namespace CompoundBox.Editor
{
    public static class DefaultArtThemeBuilder
    {
        private const string ResourcesRoot = "Assets/Sokoban/Resources/Art";
        private const string ThemePath = ResourcesRoot + "/FoundryTheme.asset";
        private const string KenneyRoot = "Assets/Sokoban/Art/ThirdParty/Kenney";

        [MenuItem("Tools/Compound Box/Create or Refresh Foundry Theme")]
        public static void CreateOrRefreshTheme()
        {
            Directory.CreateDirectory(ResourcesRoot);

            var panel = LoadTexture(KenneyRoot + "/UI/Blue/button_rectangle_border.png");
            var button = LoadTexture(KenneyRoot + "/UI/Blue/button_rectangle_depth_flat.png");
            var buttonHover = LoadTexture(KenneyRoot + "/UI/Blue/button_rectangle_gradient.png");
            var chapterIcon = LoadSprite(KenneyRoot + "/Icons/White/menuGrid.png");
            var lockedIcon = LoadSprite(KenneyRoot + "/Icons/White/locked.png");
            var undoIcon = LoadSprite(KenneyRoot + "/Icons/White/arrowLeft.png");
            var redoIcon = LoadSprite(KenneyRoot + "/Icons/White/arrowRight.png");
            var audioOnIcon = LoadSprite(KenneyRoot + "/Icons/White/audioOn.png");
            var audioOffIcon = LoadSprite(KenneyRoot + "/Icons/White/audioOff.png");
            var click = AssetDatabase.LoadAssetAtPath<AudioClip>(KenneyRoot + "/Audio/click_001.ogg");
            var confirm = AssetDatabase.LoadAssetAtPath<AudioClip>(KenneyRoot + "/Audio/confirmation_001.ogg");
            var error = AssetDatabase.LoadAssetAtPath<AudioClip>(KenneyRoot + "/Audio/error_001.ogg");
            var toggle = AssetDatabase.LoadAssetAtPath<AudioClip>(KenneyRoot + "/Audio/toggle_001.ogg");

            var theme = AssetDatabase.LoadAssetAtPath<ArtThemeAsset>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ArtThemeAsset>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            theme.Configure(
                "foundry",
                Hex("#0B111A"),
                Hex("#142132"),
                Hex("#0E1928"),
                Hex("#354A63"),
                Hex("#58718C"),
                Hex("#EAF5FF"),
                Hex("#0B111A"),
                Hex("#66E3B3"),
                Hex("#1B2A3A"),
                new[]
                {
                    Color.white,
                    Hex("#55D7EA"),
                    Hex("#F2B85D"),
                    Hex("#EF7388"),
                    Hex("#A88BEA")
                },
                panel,
                button,
                buttonHover,
                chapterIcon,
                lockedIcon,
                undoIcon,
                redoIcon,
                audioOnIcon,
                audioOffIcon,
                click,
                confirm,
                error,
                toggle,
                null,
                null,
                null);

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = theme;
            Debug.Log($"Compound Box art theme refreshed: {ThemePath}");
        }

        private static Texture2D LoadTexture(string path)
        {
            ConfigureSpriteImporter(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Sprite LoadSprite(string path)
        {
            ConfigureSpriteImporter(path);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ConfigureSpriteImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString(value, out var colour) ? colour : Color.magenta;
        }
    }
}
