using UnityEngine;

namespace CompoundBox
{
    public static class GridPalette
    {
        public static readonly Color Background = Hex("#10151D");
        public static readonly Color FloorA = Hex("#1A2230");
        public static readonly Color FloorB = Hex("#161E2A");
        public static readonly Color Wall = Hex("#35445A");
        public static readonly Color WallEdge = Hex("#52667F");
        public static readonly Color Player = Hex("#F4F7FB");
        public static readonly Color PlayerCore = Hex("#10151D");
        public static readonly Color Goal = Hex("#263545");
        public static readonly Color Exit = Hex("#76E0B0");
        public static readonly Color Shadow = new Color(0f, 0f, 0f, 0.28f);

        public static Color Matter(MatterType matter)
        {
            switch (matter)
            {
                case MatterType.Cyan:
                    return Hex("#57D6E8");
                case MatterType.Amber:
                    return Hex("#F4B95F");
                case MatterType.Rose:
                    return Hex("#EF7388");
                case MatterType.Violet:
                    return Hex("#A88BEA");
                default:
                    return Color.white;
            }
        }

        public static Color MatterDark(MatterType matter)
        {
            return Color.Lerp(Matter(matter), Background, 0.38f);
        }

        public static Color Portal(char id)
        {
            switch (id)
            {
                case 'a':
                    return Hex("#49D2C5");
                case 'b':
                    return Hex("#E47BC8");
                case 'c':
                    return Hex("#E5CF65");
                case 'd':
                    return Hex("#79A8EC");
                default:
                    return Color.white;
            }
        }

        private static Color Hex(string value)
        {
            if (ColorUtility.TryParseHtmlString(value, out var color))
            {
                return color;
            }

            return Color.magenta;
        }
    }
}
