using UnityEngine;

namespace CompoundBox
{
    public sealed class EntityCellView : MonoBehaviour
    {
        private const float CustomCellPieceScale = 1.24f;
        private static readonly Color PortalMoveMarkerColour = new Color(1f, 0.84f, 0.35f, 1f);

        private SpriteRenderer shadow;
        private SpriteRenderer shell;
        private SpriteRenderer inner;
        private SpriteRenderer accent;
        private SpriteRenderer portalMoveUp;
        private SpriteRenderer portalMoveRight;
        private SpriteRenderer portalMoveDown;
        private SpriteRenderer portalMoveLeft;

        public void Configure(
            EntityKind kind,
            MatterType matter,
            bool compound,
            Vector2Int localCell,
            IBoardVisualTheme theme = null)
        {
            EnsureHierarchy();
            transform.localPosition = new Vector3(localCell.x, localCell.y, 0f);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (kind == EntityKind.Player)
            {
                ConfigurePlayer(theme);
                return;
            }

            if (kind == EntityKind.PortalNode)
            {
                ConfigurePortalNode(theme);
                return;
            }

                ConfigureMatter(matter, compound, theme);
        }

        public void ResetForPool()
        {
            gameObject.SetActive(false);
        }

        private void ConfigureMatter(MatterType matter, bool compound, IBoardVisualTheme theme)
        {
            SetPortalMoveMarkers(false);
            var customSprite = theme?.MatterSprite != null;
            shadow.sprite = WhiteboxSprites.RoundedSquare;
            shadow.color = GridPalette.ShadowColour;
            shadow.transform.localPosition = new Vector3(-0.045f, -0.06f, 0f);
            shadow.transform.localScale = new Vector3(0.82f, 0.82f, 1f);
            shadow.sortingOrder = 5;

            shell.sprite = theme?.MatterSprite ?? WhiteboxSprites.RoundedSquare;
            shell.color = GridPalette.Matter(matter);
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = customSprite
                ? new Vector3(CustomCellPieceScale, CustomCellPieceScale, 1f)
                : new Vector3(0.8f, 0.8f, 1f);
            shell.sortingOrder = 6;

            inner.gameObject.SetActive(!customSprite);
            inner.sprite = WhiteboxSprites.Square;
            inner.color = GridPalette.MatterDark(matter);
            inner.transform.localPosition = Vector3.zero;
            inner.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            inner.transform.localScale = new Vector3(0.46f, 0.46f, 1f);
            inner.sortingOrder = 7;

            accent.gameObject.SetActive(compound);
            accent.sprite = WhiteboxSprites.Square;
            accent.color = Color.Lerp(GridPalette.Matter(matter), Color.white, 0.55f);
            accent.transform.localPosition = Vector3.zero;
            accent.transform.localRotation = Quaternion.identity;
            accent.transform.localScale = new Vector3(0.1f, 0.48f, 1f);
            accent.sortingOrder = 8;
        }

        private void ConfigurePlayer(IBoardVisualTheme theme)
        {
            SetPortalMoveMarkers(false);
            var customSprite = theme?.PlayerSprite != null;
            shadow.sprite = WhiteboxSprites.Circle;
            shadow.color = GridPalette.ShadowColour;
            shadow.transform.localPosition = new Vector3(-0.045f, -0.07f, 0f);
            shadow.transform.localScale = new Vector3(0.76f, 0.76f, 1f);
            shadow.sortingOrder = 6;

            shell.sprite = theme?.PlayerSprite ?? WhiteboxSprites.Circle;
            shell.color = GridPalette.Player;
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = customSprite
                ? new Vector3(CustomCellPieceScale, CustomCellPieceScale, 1f)
                : new Vector3(0.73f, 0.73f, 1f);
            shell.sortingOrder = 7;

            inner.gameObject.SetActive(!customSprite);
            inner.sprite = WhiteboxSprites.Circle;
            inner.color = GridPalette.PlayerCore;
            inner.transform.localPosition = Vector3.zero;
            inner.transform.localRotation = Quaternion.identity;
            inner.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            inner.sortingOrder = 8;

            accent.gameObject.SetActive(!customSprite);
            accent.sprite = WhiteboxSprites.Chevron;
            accent.color = GridPalette.PlayerCore;
            accent.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            accent.transform.localRotation = Quaternion.identity;
            accent.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            accent.sortingOrder = 9;
        }

        private void ConfigurePortalNode(IBoardVisualTheme theme)
        {
            SetPortalMoveMarkers(true);
            var customSprite = theme?.PortalSprite != null;
            shadow.sprite = WhiteboxSprites.Circle;
            shadow.color = GridPalette.ShadowColour;
            shadow.transform.localPosition = new Vector3(-0.04f, -0.06f, 0f);
            shadow.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            shadow.sortingOrder = 3;

            shell.sprite = theme?.PortalSprite ?? WhiteboxSprites.Ring;
            shell.color = GridPalette.Portal('a');
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = customSprite
                ? new Vector3(CustomCellPieceScale, CustomCellPieceScale, 1f)
                : new Vector3(0.72f, 0.72f, 1f);
            shell.sortingOrder = 4;

            inner.gameObject.SetActive(!customSprite);
            inner.sprite = WhiteboxSprites.Diamond;
            inner.color = GridPalette.Portal('a');
            inner.transform.localPosition = Vector3.zero;
            inner.transform.localRotation = Quaternion.identity;
            inner.transform.localScale = new Vector3(0.26f, 0.26f, 1f);
            inner.sortingOrder = 5;

            accent.gameObject.SetActive(false);
        }

        private void SetPortalMoveMarkers(bool visible)
        {
            portalMoveUp.gameObject.SetActive(visible);
            portalMoveRight.gameObject.SetActive(visible);
            portalMoveDown.gameObject.SetActive(visible);
            portalMoveLeft.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            ConfigurePortalMoveMarker(
                portalMoveUp,
                new Vector3(0f, 0.32f, 0f),
                90f);
            ConfigurePortalMoveMarker(
                portalMoveRight,
                new Vector3(0.32f, 0f, 0f),
                0f);
            ConfigurePortalMoveMarker(
                portalMoveDown,
                new Vector3(0f, -0.32f, 0f),
                -90f);
            ConfigurePortalMoveMarker(
                portalMoveLeft,
                new Vector3(-0.32f, 0f, 0f),
                180f);
        }

        private static void ConfigurePortalMoveMarker(
            SpriteRenderer marker,
            Vector3 localPosition,
            float rotation)
        {
            marker.sprite = WhiteboxSprites.Chevron;
            marker.color = PortalMoveMarkerColour;
            marker.transform.localPosition = localPosition;
            marker.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            marker.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            marker.sortingOrder = 10;
        }

        private void EnsureHierarchy()
        {
            if (shadow != null)
            {
                return;
            }

            shadow = CreateRenderer("Shadow", transform);
            shell = CreateRenderer("Shell", transform);
            inner = CreateRenderer("Inner", shell.transform);
            accent = CreateRenderer("Accent", shell.transform);
            portalMoveUp = CreateRenderer("Portal Move Marker Up", transform);
            portalMoveRight = CreateRenderer("Portal Move Marker Right", transform);
            portalMoveDown = CreateRenderer("Portal Move Marker Down", transform);
            portalMoveLeft = CreateRenderer("Portal Move Marker Left", transform);
        }

        private static SpriteRenderer CreateRenderer(string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject.AddComponent<SpriteRenderer>();
        }
    }
}
