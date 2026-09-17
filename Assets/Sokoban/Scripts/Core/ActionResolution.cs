using System.Collections.Generic;

namespace CompoundBox
{
    public enum GridActionType
    {
        Move,
        Split,
        PrecisionCut,
        Recombine,
        Rotate,
        Undo,
        Redo,
        Restart
    }

    public sealed class ActionResolution
    {
        public ActionResolution(
            bool success,
            GridActionType actionType,
            string message,
            IReadOnlyList<int> changedEntityIds = null,
            bool usedPortal = false,
            int pushedEntities = 0)
        {
            Success = success;
            ActionType = actionType;
            Message = message;
            ChangedEntityIds = changedEntityIds ?? new List<int>();
            UsedPortal = usedPortal;
            PushedEntities = pushedEntities;
        }

        public bool Success { get; }
        public GridActionType ActionType { get; }
        public string Message { get; }
        public IReadOnlyList<int> ChangedEntityIds { get; }
        public bool UsedPortal { get; }
        public int PushedEntities { get; }
    }
}
