using UnityEngine;

namespace CompoundBox
{
    public enum GridDirection
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4
    }

    public static class GridDirectionUtility
    {
        public static Vector2Int ToOffset(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return Vector2Int.up;
                case GridDirection.Right:
                    return Vector2Int.right;
                case GridDirection.Down:
                    return Vector2Int.down;
                case GridDirection.Left:
                    return Vector2Int.left;
                default:
                    return Vector2Int.zero;
            }
        }

        public static char ToSolutionCode(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return 'U';
                case GridDirection.Right:
                    return 'R';
                case GridDirection.Down:
                    return 'D';
                case GridDirection.Left:
                    return 'L';
                default:
                    return '?';
            }
        }

        public static bool TryParseSolutionCode(char code, out GridDirection direction)
        {
            switch (char.ToUpperInvariant(code))
            {
                case 'U':
                    direction = GridDirection.Up;
                    return true;
                case 'R':
                    direction = GridDirection.Right;
                    return true;
                case 'D':
                    direction = GridDirection.Down;
                    return true;
                case 'L':
                    direction = GridDirection.Left;
                    return true;
                default:
                    direction = GridDirection.None;
                    return false;
            }
        }
    }
}
