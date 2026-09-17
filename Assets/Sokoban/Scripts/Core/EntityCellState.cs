using System;
using UnityEngine;

namespace CompoundBox
{
    [Serializable]
    public sealed class EntityCellState
    {
        public EntityCellState(Vector2Int position, MatterType matter)
        {
            Position = position;
            Matter = matter;
        }

        public Vector2Int Position;
        public MatterType Matter;

        public EntityCellState Clone()
        {
            return new EntityCellState(Position, Matter);
        }
    }
}
