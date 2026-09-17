using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public sealed class EntityTransformPreview
    {
        public EntityTransformPreview(
            int entityId,
            EntityKind kind,
            IReadOnlyList<Vector2Int> sourceCells,
            IReadOnlyList<EntityCellState> targetCells)
        {
            EntityId = entityId;
            Kind = kind;
            SourceCells = sourceCells;
            TargetCells = targetCells;
        }

        public int EntityId { get; }
        public EntityKind Kind { get; }
        public IReadOnlyList<Vector2Int> SourceCells { get; }
        public IReadOnlyList<EntityCellState> TargetCells { get; }
    }

    public sealed class TransformPreview
    {
        public TransformPreview(
            bool isValid,
            string failureReason,
            bool usedPortal,
            IReadOnlyList<EntityTransformPreview> entities)
        {
            IsValid = isValid;
            FailureReason = failureReason;
            UsedPortal = usedPortal;
            Entities = entities ?? new List<EntityTransformPreview>();
        }

        public bool IsValid { get; }
        public string FailureReason { get; }
        public bool UsedPortal { get; }
        public IReadOnlyList<EntityTransformPreview> Entities { get; }
    }
}
