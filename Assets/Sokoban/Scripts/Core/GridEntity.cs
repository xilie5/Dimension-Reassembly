using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public enum EntityKind
    {
        Player,
        Matter,
        PortalNode
    }

    public sealed class GridEntity
    {
        private readonly List<EntityCellState> cellStates = new List<EntityCellState>();
        private readonly List<EntityConnection> connections = new List<EntityConnection>();
        private readonly List<Vector2Int> positions = new List<Vector2Int>();

        public GridEntity(int id, EntityKind kind, MatterType matter, IEnumerable<Vector2Int> cells)
        {
            Id = id;
            Kind = kind;
            foreach (var cell in cells)
            {
                cellStates.Add(new EntityCellState(cell, matter));
            }

            SortAndRefresh();
            RebuildAutoConnections();
        }

        public GridEntity(
            int id,
            EntityKind kind,
            IEnumerable<EntityCellState> cells,
            IEnumerable<EntityConnection> entityConnections = null)
        {
            Id = id;
            Kind = kind;
            foreach (var cell in cells)
            {
                cellStates.Add(cell.Clone());
            }

            if (entityConnections != null)
            {
                foreach (var connection in entityConnections)
                {
                    if (Contains(connection.First) && Contains(connection.Second) &&
                        !connections.Contains(connection))
                    {
                        connections.Add(connection);
                    }
                }
            }

            SortAndRefresh();
            if (entityConnections == null)
            {
                RebuildAutoConnections();
            }
        }

        public int Id { get; private set; }
        public EntityKind Kind { get; private set; }
        public IReadOnlyList<EntityCellState> CellStates => cellStates;
        public IReadOnlyList<EntityConnection> Connections => connections;
        public IReadOnlyList<Vector2Int> Cells => positions;

        public MatterType Matter
        {
            get { return PrimaryMatter; }
        }

        public MatterType PrimaryMatter
        {
            get { return cellStates.Count == 0 ? MatterType.None : cellStates[0].Matter; }
        }

        public Vector2Int Anchor
        {
            get
            {
                var anchor = positions[0];
                for (var i = 1; i < positions.Count; i++)
                {
                    var cell = positions[i];
                    if (cell.y < anchor.y || (cell.y == anchor.y && cell.x < anchor.x))
                    {
                        anchor = cell;
                    }
                }

                return anchor;
            }
        }

        public bool IsUniformMatter(out MatterType matter)
        {
            matter = PrimaryMatter;
            for (var i = 1; i < cellStates.Count; i++)
            {
                if (cellStates[i].Matter != matter)
                {
                    matter = MatterType.None;
                    return false;
                }
            }

            return matter != MatterType.None;
        }

        public bool Occupies(Vector2Int cell)
        {
            for (var i = 0; i < positions.Count; i++)
            {
                if (positions[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCell(Vector2Int position, out EntityCellState cell)
        {
            for (var i = 0; i < cellStates.Count; i++)
            {
                if (cellStates[i].Position == position)
                {
                    cell = cellStates[i];
                    return true;
                }
            }

            cell = null;
            return false;
        }

        public MatterType GetMatterAt(Vector2Int position)
        {
            return TryGetCell(position, out var cell) ? cell.Matter : MatterType.None;
        }

        public bool HasConnection(Vector2Int first, Vector2Int second)
        {
            for (var i = 0; i < connections.Count; i++)
            {
                if (connections[i].Connects(first, second))
                {
                    return true;
                }
            }

            return false;
        }

        public GridEntity Clone()
        {
            return new GridEntity(Id, Kind, cellStates, connections);
        }

        public void SetCells(IReadOnlyList<Vector2Int> updatedCells)
        {
            var translation = new Dictionary<Vector2Int, Vector2Int>();
            for (var i = 0; i < cellStates.Count; i++)
            {
                var oldPosition = cellStates[i].Position;
                var newPosition = i < updatedCells.Count ? updatedCells[i] : oldPosition;
                translation[oldPosition] = newPosition;
                cellStates[i].Position = newPosition;
            }

            for (var i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (translation.TryGetValue(connection.First, out var first) &&
                    translation.TryGetValue(connection.Second, out var second))
                {
                    connections[i] = new EntityConnection(first, second);
                }
            }

            SortAndRefresh();
            DeduplicateConnections();
        }

        public void ReplaceStructure(
            IEnumerable<EntityCellState> updatedCells,
            IEnumerable<EntityConnection> updatedConnections)
        {
            cellStates.Clear();
            connections.Clear();
            foreach (var cell in updatedCells)
            {
                cellStates.Add(cell.Clone());
            }

            foreach (var connection in updatedConnections)
            {
                if (Contains(connection.First) && Contains(connection.Second))
                {
                    connections.Add(connection);
                }
            }

            SortAndRefresh();
            DeduplicateConnections();
        }

        public void AddAutoBondConnections()
        {
            for (var firstIndex = 0; firstIndex < cellStates.Count; firstIndex++)
            {
                for (var secondIndex = firstIndex + 1; secondIndex < cellStates.Count; secondIndex++)
                {
                    var first = cellStates[firstIndex];
                    var second = cellStates[secondIndex];
                    if (!AreAdjacent(first.Position, second.Position) ||
                        !MatterTypeUtility.CanBond(first.Matter, second.Matter) ||
                        ContainsConnection(first.Position, second.Position))
                    {
                        continue;
                    }

                    connections.Add(new EntityConnection(first.Position, second.Position));
                }
            }
        }

        private void RebuildAutoConnections()
        {
            connections.Clear();
            for (var firstIndex = 0; firstIndex < cellStates.Count; firstIndex++)
            {
                for (var secondIndex = firstIndex + 1; secondIndex < cellStates.Count; secondIndex++)
                {
                    if (AreAdjacent(cellStates[firstIndex].Position, cellStates[secondIndex].Position))
                    {
                        connections.Add(new EntityConnection(
                            cellStates[firstIndex].Position,
                            cellStates[secondIndex].Position));
                    }
                }
            }
        }

        private bool Contains(Vector2Int position)
        {
            for (var i = 0; i < cellStates.Count; i++)
            {
                if (cellStates[i].Position == position)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsConnection(Vector2Int first, Vector2Int second)
        {
            return HasConnection(first, second);
        }

        private void SortAndRefresh()
        {
            cellStates.Sort((left, right) =>
            {
                var yComparison = left.Position.y.CompareTo(right.Position.y);
                return yComparison != 0 ? yComparison : left.Position.x.CompareTo(right.Position.x);
            });

            positions.Clear();
            for (var i = 0; i < cellStates.Count; i++)
            {
                positions.Add(cellStates[i].Position);
            }
        }

        private void DeduplicateConnections()
        {
            for (var i = connections.Count - 1; i >= 0; i--)
            {
                if (!Contains(connections[i].First) || !Contains(connections[i].Second))
                {
                    connections.RemoveAt(i);
                    continue;
                }

                for (var j = 0; j < i; j++)
                {
                    if (!connections[i].Equals(connections[j]))
                    {
                        continue;
                    }

                    connections.RemoveAt(i);
                    break;
                }
            }
        }

        private static bool AreAdjacent(Vector2Int first, Vector2Int second)
        {
            var delta = first - second;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;
        }
    }
}
