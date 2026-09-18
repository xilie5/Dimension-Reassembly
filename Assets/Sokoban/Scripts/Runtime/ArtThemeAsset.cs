using UnityEngine;

namespace CompoundBox
{
    [CreateAssetMenu(fileName = "Foundry Theme", menuName = "Compound Box/Art Theme")]
    public sealed class ArtThemeAsset : ScriptableObject, IBoardVisualTheme
    {
        [Header("Identity")]
        [SerializeField] private string themeId = "foundry";

        [Header("Palette")]
        [SerializeField] private Color background = new Color(0.06f, 0.08f, 0.11f, 1f);
        [SerializeField] private Color floorA = new Color(0.10f, 0.13f, 0.19f, 1f);
        [SerializeField] private Color floorB = new Color(0.08f, 0.11f, 0.16f, 1f);
        [SerializeField] private Color wall = new Color(0.21f, 0.27f, 0.35f, 1f);
        [SerializeField] private Color wallEdge = new Color(0.32f, 0.40f, 0.50f, 1f);
        [SerializeField] private Color player = new Color(0.96f, 0.97f, 0.99f, 1f);
        [SerializeField] private Color playerCore = new Color(0.06f, 0.08f, 0.11f, 1f);
        [SerializeField] private Color exit = new Color(0.46f, 0.88f, 0.69f, 1f);
        [SerializeField] private Color goal = new Color(0.15f, 0.21f, 0.27f, 1f);
        [SerializeField] private Color[] matter = new Color[5]
        {
            Color.white,
            new Color(0.34f, 0.84f, 0.91f, 1f),
            new Color(0.96f, 0.73f, 0.37f, 1f),
            new Color(0.94f, 0.45f, 0.53f, 1f),
            new Color(0.66f, 0.55f, 0.92f, 1f)
        };

        [Header("UI")]
        [SerializeField] private Texture2D panelTexture;
        [SerializeField] private Texture2D buttonTexture;
        [SerializeField] private Texture2D buttonHoverTexture;
        [SerializeField] private Sprite iconChapter;
        [SerializeField] private Sprite iconLocked;
        [SerializeField] private Sprite iconUndo;
        [SerializeField] private Sprite iconRedo;
        [SerializeField] private Sprite iconAudioOn;
        [SerializeField] private Sprite iconAudioOff;

        [Header("Board")]
        [SerializeField] private Sprite floorSprite;
        [SerializeField] private Sprite wallSprite;
        [SerializeField] private Sprite goalSprite;
        [SerializeField] private Sprite exitSprite;
        [SerializeField] private Sprite portalSprite;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private Sprite matterSprite;

        [Header("Audio")]
        [SerializeField] private AudioClip uiClick;
        [SerializeField] private AudioClip uiConfirm;
        [SerializeField] private AudioClip uiError;
        [SerializeField] private AudioClip uiToggle;

        public string ThemeId => themeId;
        public Color Background => background;
        public Color FloorA => floorA;
        public Color FloorB => floorB;
        public Color Wall => wall;
        public Color WallEdge => wallEdge;
        public Color Player => player;
        public Color PlayerCore => playerCore;
        public Color Exit => exit;
        public Color Goal => goal;
        public Texture2D PanelTexture => panelTexture;
        public Texture2D ButtonTexture => buttonTexture;
        public Texture2D ButtonHoverTexture => buttonHoverTexture;
        public Sprite IconChapter => iconChapter;
        public Sprite IconLocked => iconLocked;
        public Sprite IconUndo => iconUndo;
        public Sprite IconRedo => iconRedo;
        public Sprite IconAudioOn => iconAudioOn;
        public Sprite IconAudioOff => iconAudioOff;
        public Sprite FloorSprite => floorSprite;
        public Sprite WallSprite => wallSprite;
        public Sprite GoalSprite => goalSprite;
        public Sprite ExitSprite => exitSprite;
        public Sprite PortalSprite => portalSprite;
        public Sprite PlayerSprite => playerSprite;
        public Sprite MatterSprite => matterSprite;
        public AudioClip UiClick => uiClick;
        public AudioClip UiConfirm => uiConfirm;
        public AudioClip UiError => uiError;
        public AudioClip UiToggle => uiToggle;

        public Color Matter(MatterType type)
        {
            var index = Mathf.Clamp((int)type, 0, matter.Length - 1);
            return matter[index];
        }

        public void Configure(
            string id,
            Color backgroundColour,
            Color floorAColour,
            Color floorBColour,
            Color wallColour,
            Color wallEdgeColour,
            Color playerColour,
            Color playerCoreColour,
            Color exitColour,
            Color goalColour,
            Color[] matterColours,
            Texture2D panel,
            Texture2D button,
            Texture2D buttonHover,
            Sprite chapterIcon,
            Sprite lockedIcon,
            Sprite undoIcon,
            Sprite redoIcon,
            Sprite audioOnIcon,
            Sprite audioOffIcon,
            AudioClip click,
            AudioClip confirm,
            AudioClip error,
            AudioClip toggle,
            Sprite floorTileSprite = null,
            Sprite wallTileSprite = null,
            Sprite matterTileSprite = null)
        {
            themeId = id;
            background = backgroundColour;
            floorA = floorAColour;
            floorB = floorBColour;
            wall = wallColour;
            wallEdge = wallEdgeColour;
            player = playerColour;
            playerCore = playerCoreColour;
            exit = exitColour;
            goal = goalColour;
            matter = matterColours;
            panelTexture = panel;
            buttonTexture = button;
            buttonHoverTexture = buttonHover;
            iconChapter = chapterIcon;
            iconLocked = lockedIcon;
            iconUndo = undoIcon;
            iconRedo = redoIcon;
            iconAudioOn = audioOnIcon;
            iconAudioOff = audioOffIcon;
            uiClick = click;
            uiConfirm = confirm;
            uiError = error;
            uiToggle = toggle;
            floorSprite = floorTileSprite;
            wallSprite = wallTileSprite;
            matterSprite = matterTileSprite;
        }
    }
}
