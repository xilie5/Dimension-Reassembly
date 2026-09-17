using System;
using UnityEngine;

namespace CompoundBox
{
    [Serializable]
    public struct EntityConnection : IEquatable<EntityConnection>
    {
        public EntityConnection(Vector2Int first, Vector2Int second)
        {
            if (Compare(first, second) <= 0)
            {
                First = first;
                Second = second;
            }
            else
            {
                First = second;
                Second = first;
            }
        }

        public Vector2Int First;
        public Vector2Int Second;

        public bool Connects(Vector2Int first, Vector2Int second)
        {
            return (First == first && Second == second) || (First == second && Second == first);
        }

        public bool Contains(Vector2Int cell)
        {
            return First == cell || Second == cell;
        }

        public EntityConnection Translated(Vector2Int offset)
        {
            return new EntityConnection(First + offset, Second + offset);
        }

        public bool Equals(EntityConnection other)
        {
            return First == other.First && Second == other.Second;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityConnection other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (First.GetHashCode() * 397) ^ Second.GetHashCode();
            }
        }

        private static int Compare(Vector2Int left, Vector2Int right)
        {
            var yComparison = left.y.CompareTo(right.y);
            return yComparison != 0 ? yComparison : left.x.CompareTo(right.x);
        }
    }
}
