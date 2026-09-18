using UnityEngine;

namespace CompoundBox
{
    public static class GridPalette
    {
        private static readonly Color DefaultBackground = Hex("#10151D");
        private static readonly Color DefaultFloorA = Hex("#1A2230");
        private static readonly Color DefaultFloorB = Hex("#161E2A");
        private static readonly Color DefaultWall = Hex("#35445A");
        private static readonly Color DefaultWallEdge = Hex("#52667F");
        private static readonly Color DefaultPlayer = Hex("#F4F7FB");
        private static readonly Color DefaultPlayerCore = Hex("#10151D");
        private static readonly Color DefaultGoal = Hex("#263545");
        private static readonly Color DefaultExit = Hex("#76E0B0");
        private static readonly Color Shadow = new Color(0f, 0f, 0f, 0.28f);

        public static ArtThemeAsset Theme { get; set; }

        public static Color Background => Theme == null ? DefaultBackground : Theme.Background;
        public static Color FloorA => Theme == null ? DefaultFloorA : Theme.FloorA;
        public static Color FloorB => Theme == null ? DefaultFloorB : Theme.FloorB;
        public static Color Wall => Theme == null ? DefaultWall : Theme.Wall;
        public static Color WallEdge => Theme == null ? DefaultWallEdge : Theme.WallEdge;
        public static Color Player => Theme == null ? DefaultPlayer : Theme.Player;
        public static Color PlayerCore => Theme == null ? DefaultPlayerCore : Theme.PlayerCore;
        public static Color Goal => Theme == null ? DefaultGoal : Theme.Goal;
        public static Color Exit => Theme == null ? DefaultExit : Theme.Exit;
        public static Color ShadowColour => Shadow;

        public static Color Matter(MatterType matter)
        {
            if (Theme != null)
            {
                return Theme.Matter(matter);
            }

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
