namespace CompoundBox
{
    public enum MatterType
    {
        None = 0,
        Cyan = 1,
        Amber = 2,
        Rose = 3,
        Violet = 4
    }

    public static class MatterTypeUtility
    {
        public static bool TryParseBlock(char value, out MatterType matter)
        {
            switch (value)
            {
                case '1':
                    matter = MatterType.Cyan;
                    return true;
                case '2':
                    matter = MatterType.Amber;
                    return true;
                case '3':
                    matter = MatterType.Rose;
                    return true;
                case '4':
                    matter = MatterType.Violet;
                    return true;
                default:
                    matter = MatterType.None;
                    return false;
            }
        }

        public static bool TryParseGoal(char value, out MatterType matter, out bool requiresCompound)
        {
            requiresCompound = char.IsUpper(value);
            switch (char.ToLowerInvariant(value))
            {
                case 'x':
                    matter = MatterType.Cyan;
                    return true;
                case 'y':
                    matter = MatterType.Amber;
                    return true;
                case 'z':
                    matter = MatterType.Rose;
                    return true;
                case 'w':
                    matter = MatterType.Violet;
                    return true;
                default:
                    matter = MatterType.None;
                    requiresCompound = false;
                    return false;
            }
        }

        public static char ToBlockChar(MatterType matter)
        {
            return ((int)matter).ToString()[0];
        }

        public static string ToDisplayName(MatterType matter)
        {
            switch (matter)
            {
                case MatterType.Cyan:
                    return "Cyan";
                case MatterType.Amber:
                    return "Amber";
                case MatterType.Rose:
                    return "Rose";
                case MatterType.Violet:
                    return "Violet";
                default:
                    return "Empty";
            }
        }
    }
}
