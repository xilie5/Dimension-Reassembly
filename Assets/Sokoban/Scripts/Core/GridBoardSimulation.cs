using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public static class GridBoardSimulation
    {
        public static ActionResolution Move(GridBoardState state, GridDirection direction)
        {
            if (direction == GridDirection.None)
            {
                return new ActionResolution(false, GridActionType.Move, "No direction supplied.");
            }

            var offset = GridDirectionUtility.ToOffset(direction);
            if (!TryMove(state, offset, out var changedEntityIds, out var usedPortal, out var pushedEntities, out var reason))
            {
                return new ActionResolution(false, GridActionType.Move, reason);
            }

            state.Facing = direction;
            state.MoveCount++;
            state.ActionCount++;
            state.PushCount += pushedEntities;
            return new ActionResolution(
                true,
                GridActionType.Move,
                state.IsSolved() ? "Puzzle solved." : "Move accepted.",
                changedEntityIds,
                usedPortal,
                pushedEntities);
        }

        public static TransformPreview PreviewMove(GridBoardState state, GridDirection direction)
        {
            if (direction == GridDirection.None)
            {
                return new TransformPreview(false, "No direction supplied.", false, null);
            }

            var clonedState = state.Clone();
            var result = Move(clonedState, direction);
            var previews = new List<EntityTransformPreview>();
            if (result.Success)
            {
                for (var i = 0; i < result.ChangedEntityIds.Count; i++)
                {
                    var entityId = result.ChangedEntityIds[i];
                    var source = state.GetEntity(entityId);
                    var target = clonedState.GetEntity(entityId);
                    if (source == null || target == null)
                    {
                        continue;
                    }

                    previews.Add(new EntityTransformPreview(
                        entityId,
                        source.Kind,
                        source.Cells,
                        target.CellStates));
                }

                return new TransformPreview(true, string.Empty, result.UsedPortal, previews);
            }

            var player = state.Player;
            var rawTargets = new List<EntityCellState>(player.CellStates.Count);
            var offset = GridDirectionUtility.ToOffset(direction);
            for (var i = 0; i < player.CellStates.Count; i++)
            {
                var cell = player.CellStates[i];
                rawTargets.Add(new EntityCellState(cell.Position + offset, cell.Matter));
            }

            previews.Add(new EntityTransformPreview(
                player.Id,
                player.Kind,
                player.Cells,
                rawTargets));
            return new TransformPreview(false, result.Message, false, previews);
        }

        public static ActionResolution SplitFacingEntity(GridBoardState state)
        {
            var target = state.Player.Anchor + GridDirectionUtility.ToOffset(state.Facing);
            var entity = state.FindEntityAt(target);
            if (entity == null || entity.Kind != EntityKind.Matter)
            {
                return new ActionResolution(false, GridActionType.Split, "No matter is aligned with the splitter.");
            }

            if (entity.Cells.Count < 2)
            {
                return new ActionResolution(false, GridActionType.Split, "A single-cell mass cannot be split.");
            }

            var changed = new List<int> { entity.Id };
            state.Entities.Remove(entity);
            for (var i = 0; i < entity.CellStates.Count; i++)
            {
                var shard = new GridEntity(
                    state.NextEntityId++,
                    EntityKind.Matter,
                    new[] { entity.CellStates[i] });
                state.Entities.Add(shard);
                changed.Add(shard.Id);
            }

            state.ActionCount++;
            return new ActionResolution(
                true,
                GridActionType.Split,
                "Compound mass split into independent cells.",
                changed);
        }

        public static ActionResolution RecombineAdjacentMatter(GridBoardState state)
        {
            var matterEntities = state.Entities.FindAll(entity => entity.Kind == EntityKind.Matter);
            var adjacency = new Dictionary<int, List<int>>();
            for (var i = 0; i < matterEntities.Count; i++)
            {
                adjacency[matterEntities[i].Id] = new List<int>();
            }

            for (var i = 0; i < matterEntities.Count; i++)
            {
                for (var j = i + 1; j < matterEntities.Count; j++)
                {
                    var left = matterEntities[i];
                    var right = matterEntities[j];
                    if (!left.IsUniformMatter(out var leftMatter) ||
                        !right.IsUniformMatter(out var rightMatter) ||
                        leftMatter != rightMatter ||
                        !AreAdjacent(left, right))
                    {
                        continue;
                    }

                    adjacency[left.Id].Add(right.Id);
                    adjacency[right.Id].Add(left.Id);
                }
            }

            var visited = new HashSet<int>();
            var groups = new List<List<GridEntity>>();
            for (var i = 0; i < matterEntities.Count; i++)
            {
                var seed = matterEntities[i];
                if (!visited.Add(seed.Id))
                {
                    continue;
                }

                var group = new List<GridEntity>();
                var queue = new Queue<GridEntity>();
                queue.Enqueue(seed);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    group.Add(current);
                    var neighbours = adjacency[current.Id];
                    for (var neighbourIndex = 0; neighbourIndex < neighbours.Count; neighbourIndex++)
                    {
                        var neighbour = state.GetEntity(neighbours[neighbourIndex]);
                        if (visited.Add(neighbour.Id))
                        {
                            queue.Enqueue(neighbour);
                        }
                    }
                }

                if (group.Count > 1)
                {
                    groups.Add(group);
                }
            }

            if (groups.Count == 0)
            {
                return new ActionResolution(false, GridActionType.Recombine, "No matching masses are touching.");
            }

            var changed = new List<int>();
            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];
                group[0].IsUniformMatter(out var matter);
                var cells = new List<EntityCellState>();
                var entityConnections = new List<EntityConnection>();
                for (var entityIndex = 0; entityIndex < group.Count; entityIndex++)
                {
                    var entity = group[entityIndex];
                    changed.Add(entity.Id);
                    for (var cellIndex = 0; cellIndex < entity.CellStates.Count; cellIndex++)
                    {
                        cells.Add(entity.CellStates[cellIndex]);
                    }

                    for (var connectionIndex = 0; connectionIndex < entity.Connections.Count; connectionIndex++)
                    {
                        entityConnections.Add(entity.Connections[connectionIndex]);
                    }

                    state.Entities.Remove(entity);
                }

                var merged = new GridEntity(
                    state.NextEntityId++,
                    EntityKind.Matter,
                    cells,
                    entityConnections);
                merged.AddAutoBondConnections();
                state.Entities.Add(merged);
                changed.Add(merged.Id);
            }

            state.ActionCount++;
            return new ActionResolution(
                true,
                GridActionType.Recombine,
                "Adjacent matching matter recombined.",
                changed);
        }

        private static bool TryMove(
            GridBoardState state,
            Vector2Int offset,
            out List<int> changedEntityIds,
            out bool usedPortal,
            out int pushedEntities,
            out string reason)
        {
            changedEntityIds = new List<int>();
            usedPortal = false;
            pushedEntities = 0;

            var movingIds = new HashSet<int> { state.PlayerId };
            var proposals = new Dictionary<int, List<Vector2Int>>();
            var maxIterations = state.Entities.Count + 4;

            for (var iteration = 0; iteration < maxIterations; iteration++)
            {
                proposals.Clear();
                foreach (var entityId in movingIds)
                {
                    var entity = state.GetEntity(entityId);
                    if (entity == null || !TryResolvePortalPlacement(state, entity, offset, out var proposal, out var traversed))
                    {
                        reason = "A portal loop prevented movement.";
                        return false;
                    }

                    proposals[entity.Id] = proposal;
                    usedPortal |= traversed;
                }

                var addedEntity = false;
                for (var entityIndex = 0; entityIndex < state.Entities.Count; entityIndex++)
                {
                    var entity = state.Entities[entityIndex];
                    if (movingIds.Contains(entity.Id))
                    {
                        continue;
                    }

                    var overlaps = false;
                    foreach (var movingProposal in proposals.Values)
                    {
                        if (!CellsOverlap(movingProposal, entity.Cells))
                        {
                            continue;
                        }

                        overlaps = true;
                        break;
                    }

                    if (overlaps && movingIds.Add(entity.Id))
                    {
                        addedEntity = true;
                    }
                }

                if (!addedEntity)
                {
                    break;
                }
            }

            var occupied = new HashSet<Vector2Int>();
            for (var entityIndex = 0; entityIndex < state.Entities.Count; entityIndex++)
            {
                var entity = state.Entities[entityIndex];
                IReadOnlyList<Vector2Int> cells = movingIds.Contains(entity.Id) ? proposals[entity.Id] : entity.Cells;

                for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                {
                    var cell = cells[cellIndex];
                    if (!state.IsWalkable(cell))
                    {
                        reason = "The route is blocked.";
                        return false;
                    }

                    if (!occupied.Add(cell))
                    {
                        reason = "The move would compress two masses into one cell.";
                        return false;
                    }
                }
            }

            for (var entityIndex = 0; entityIndex < state.Entities.Count; entityIndex++)
            {
                var entity = state.Entities[entityIndex];
                if (!movingIds.Contains(entity.Id))
                {
                    continue;
                }

                var proposal = proposals[entity.Id];
                if (!CellsEqual(entity.Cells, proposal))
                {
                    changedEntityIds.Add(entity.Id);
                    if (entity.Kind == EntityKind.Matter)
                    {
                        pushedEntities++;
                    }
                }

                entity.SetCells(proposal);
            }

            reason = string.Empty;
            return true;
        }

        private static bool TryResolvePortalPlacement(
            GridBoardState state,
            GridEntity entity,
            Vector2Int offset,
            out List<Vector2Int> proposal,
            out bool traversed)
        {
            proposal = new List<Vector2Int>(entity.Cells.Count);
            for (var i = 0; i < entity.Cells.Count; i++)
            {
                proposal.Add(entity.Cells[i] + offset);
            }

            traversed = false;
            for (var jumpIndex = 0; jumpIndex < 4; jumpIndex++)
            {
                PortalPair pair = null;
                for (var cellIndex = 0; cellIndex < proposal.Count; cellIndex++)
                {
                    if (state.Portals.TryGetValue(proposal[cellIndex], out pair))
                    {
                        break;
                    }
                }

                if (pair == null)
                {
                    return true;
                }

                var translation = pair.Exit - GetAnchor(proposal);
                for (var cellIndex = 0; cellIndex < proposal.Count; cellIndex++)
                {
                    proposal[cellIndex] += translation;
                }

                traversed = true;
            }

            return false;
        }

        private static Vector2Int GetAnchor(IReadOnlyList<Vector2Int> cells)
        {
            var anchor = cells[0];
            for (var i = 1; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.y < anchor.y || (cell.y == anchor.y && cell.x < anchor.x))
                {
                    anchor = cell;
                }
            }

            return anchor;
        }

        private static bool AreAdjacent(GridEntity left, GridEntity right)
        {
            for (var leftIndex = 0; leftIndex < left.Cells.Count; leftIndex++)
            {
                for (var rightIndex = 0; rightIndex < right.Cells.Count; rightIndex++)
                {
                    var delta = left.Cells[leftIndex] - right.Cells[rightIndex];
                    if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CellsOverlap(IReadOnlyList<Vector2Int> left, IReadOnlyList<Vector2Int> right)
        {
            for (var leftIndex = 0; leftIndex < left.Count; leftIndex++)
            {
                for (var rightIndex = 0; rightIndex < right.Count; rightIndex++)
                {
                    if (left[leftIndex] == right[rightIndex])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CellsEqual(IReadOnlyList<Vector2Int> left, IReadOnlyList<Vector2Int> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (var i = 0; i < left.Count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
