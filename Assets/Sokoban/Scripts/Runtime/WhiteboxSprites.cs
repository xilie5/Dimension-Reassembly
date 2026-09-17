using UnityEngine;

namespace CompoundBox
{
    public static class WhiteboxSprites
    {
        private static Sprite square;
        private static Sprite roundedSquare;
        private static Sprite circle;
        private static Sprite ring;
        private static Sprite diamond;
        private static Sprite chevron;

        public static Sprite Square => square ?? (square = CreateSquare(false));
        public static Sprite RoundedSquare => roundedSquare ?? (roundedSquare = CreateSquare(true));
        public static Sprite Circle => circle ?? (circle = CreateCircle(false));
        public static Sprite Ring => ring ?? (ring = CreateCircle(true));
        public static Sprite Diamond => diamond ?? (diamond = CreateDiamond());
        public static Sprite Chevron => chevron ?? (chevron = CreateChevron());

        private static Sprite CreateSquare(bool rounded)
        {
            const int size = 64;
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var edgeX = Mathf.Min(x, size - 1 - x);
                    var edgeY = Mathf.Min(y, size - 1 - y);
                    var edge = Mathf.Min(edgeX, edgeY);
                    var alpha = edge < 2 ? (byte)0 : edge < 4 ? (byte)150 : (byte)255;
                    if (rounded)
                    {
                        var inset = 3f;
                        var dx = Mathf.Max(0f, inset - x, x - (size - 1 - inset));
                        var dy = Mathf.Max(0f, inset - y, y - (size - 1 - inset));
                        if (dx * dx + dy * dy > inset * inset)
                        {
                            alpha = 0;
                        }
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateCircle(bool onlyRing)
        {
            const int size = 64;
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            var centre = (size - 1) * 0.5f;
            var outer = size * 0.46f;
            var inner = size * (onlyRing ? 0.31f : 0f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(centre, centre));
                    var alpha = distance <= outer && distance >= inner ? (byte)255 : (byte)0;
                    if (alpha > 0 && outer - distance < 2f)
                    {
                        alpha = 180;
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateDiamond()
        {
            const int size = 64;
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            var centre = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Mathf.Abs(x - centre) + Mathf.Abs(y - centre);
                    var alpha = distance <= size * 0.43f ? (byte)255 : (byte)0;
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateChevron()
        {
            const int size = 64;
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var normalizedX = (x - 32f) / 32f;
                    var normalizedY = (y - 32f) / 32f;
                    var inLeft = normalizedX > -0.6f && normalizedX < 0.1f &&
                                 Mathf.Abs((normalizedX + 0.25f) - normalizedY * 0.7f) < 0.13f;
                    var inRight = normalizedX > -0.1f && normalizedX < 0.6f &&
                                  Mathf.Abs((normalizedX - 0.25f) + normalizedY * 0.7f) < 0.13f;
                    pixels[y * size + x] = new Color32(255, 255, 255, inLeft || inRight ? (byte)255 : (byte)0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "GeneratedWhitebox"
            };
        }
    }
}
