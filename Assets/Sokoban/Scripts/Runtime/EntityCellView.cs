using UnityEngine;

namespace CompoundBox
{
    public sealed class EntityCellView : MonoBehaviour
    {
        private SpriteRenderer shadow;
        private SpriteRenderer shell;
        private SpriteRenderer inner;
        private SpriteRenderer accent;

        public void Configure(EntityKind kind, MatterType matter, bool compound, Vector2Int localCell)
        {
            EnsureHierarchy();
            transform.localPosition = new Vector3(localCell.x, localCell.y, 0f);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (kind == EntityKind.Player)
            {
                ConfigurePlayer();
                return;
            }

            if (kind == EntityKind.PortalNode)
            {
                ConfigurePortalNode();
                return;
            }

            ConfigureMatter(matter, compound);
        }

        public void ResetForPool()
        {
            gameObject.SetActive(false);
        }

        private void ConfigureMatter(MatterType matter, bool compound)
        {
            shadow.sprite = WhiteboxSprites.RoundedSquare;
            shadow.color = GridPalette.ShadowColour;
            shadow.transform.localPosition = new Vector3(-0.045f, -0.06f, 0f);
            shadow.transform.localScale = new Vector3(0.82f, 0.82f, 1f);
            shadow.sortingOrder = 5;

            shell.sprite = WhiteboxSprites.RoundedSquare;
            shell.color = GridPalette.Matter(matter);
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            shell.sortingOrder = 6;

            inner.gameObject.SetActive(true);
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

        private void ConfigurePlayer()
        {
            shadow.sprite = WhiteboxSprites.Circle;
            shadow.color = GridPalette.ShadowColour;
            shadow.transform.localPosition = new Vector3(-0.045f, -0.07f, 0f);
            shadow.transform.localScale = new Vector3(0.76f, 0.76f, 1f);
            shadow.sortingOrder = 6;

            shell.sprite = WhiteboxSprites.Circle;
            shell.color = GridPalette.Player;
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = new Vector3(0.73f, 0.73f, 1f);
            shell.sortingOrder = 7;

            inner.gameObject.SetActive(true);
            inner.sprite = WhiteboxSprites.Circle;
            inner.color = GridPalette.PlayerCore;
            inner.transform.localPosition = Vector3.zero;
            inner.transform.localRotation = Quaternion.identity;
            inner.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            inner.sortingOrder = 8;

            accent.gameObject.SetActive(true);
            accent.sprite = WhiteboxSprites.Chevron;
            accent.color = GridPalette.PlayerCore;
            accent.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            accent.transform.localRotation = Quaternion.identity;
            accent.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            accent.sortingOrder = 9;
        }

        private void ConfigurePortalNode()
        {
            shadow.sprite = WhiteboxSprites.Circle;
            shadow.color = GridPalette.ShadowColour;
            shadow.transform.localPosition = new Vector3(-0.04f, -0.06f, 0f);
            shadow.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            shadow.sortingOrder = 7;

            shell.sprite = WhiteboxSprites.Ring;
            shell.color = GridPalette.Portal('a');
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            shell.sortingOrder = 8;

            inner.gameObject.SetActive(true);
            inner.sprite = WhiteboxSprites.Diamond;
            inner.color = GridPalette.Portal('a');
            inner.transform.localPosition = Vector3.zero;
            inner.transform.localRotation = Quaternion.identity;
            inner.transform.localScale = new Vector3(0.26f, 0.26f, 1f);
            inner.sortingOrder = 9;

            accent.gameObject.SetActive(false);
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
        }

        private static SpriteRenderer CreateRenderer(string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject.AddComponent<SpriteRenderer>();
        }
    }
}
