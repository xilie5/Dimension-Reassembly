using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public enum TileKind
    {
        Void,
        Floor,
        Wall,
        Portal,
        Goal,
        Exit
    }

    public sealed class PortalPair
    {
        public PortalPair(
            char id,
            Vector2Int entry,
            Vector2Int exit,
            int entryEntityId = -1,
            int exitEntityId = -1)
        {
            Id = id;
            FixedEntry = entry;
            FixedExit = exit;
            EntryEntityId = entryEntityId;
            ExitEntityId = exitEntityId;
        }

        public char Id { get; }
        public Vector2Int FixedEntry { get; }
        public Vector2Int FixedExit { get; }
        public int EntryEntityId { get; }
        public int ExitEntityId { get; }
        public Vector2Int Entry => FixedEntry;
        public Vector2Int Exit => FixedExit;

        public Vector2Int ResolveEntry(GridBoardState state)
        {
            return Resolve(state, EntryEntityId, FixedEntry);
        }

        public Vector2Int ResolveExit(GridBoardState state)
        {
            return Resolve(state, ExitEntityId, FixedExit);
        }

        public PortalPair Clone()
        {
            return new PortalPair(Id, FixedEntry, FixedExit, EntryEntityId, ExitEntityId);
        }

        private static Vector2Int Resolve(GridBoardState state, int entityId, Vector2Int fallback)
        {
            if (entityId < 0)
            {
                return fallback;
            }

            var entity = state.GetEntity(entityId);
            return entity == null ? fallback : entity.Anchor;
        }
    }

    public sealed class GoalDefinition
    {
        public GoalDefinition(Vector2Int cell, MatterType matter, bool requiresCompound)
        {
            Cell = cell;
            Matter = matter;
            RequiresCompound = requiresCompound;
        }

        public Vector2Int Cell { get; }
        public MatterType Matter { get; }
        public bool RequiresCompound { get; }

        public GoalDefinition Clone()
        {
            return new GoalDefinition(Cell, Matter, RequiresCompound);
        }
    }

    public sealed class GridBoardState
    {
        private TileKind[,] tiles;

        public GridBoardState(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new TileKind[width, height];
            Entities = new List<GridEntity>();
            Goals = new List<GoalDefinition>();
            PortalPairs = new List<PortalPair>();
            Portals = new Dictionary<Vector2Int, PortalPair>();
            Facing = GridDirection.Right;
            PlayerId = -1;
            NextEntityId = 1;
            HasExit = false;
            ExitCell = new Vector2Int(-1, -1);
        }

        public int Width { get; }
        public int Height { get; }
        public List<GridEntity> Entities { get; private set; }
        public List<GoalDefinition> Goals { get; private set; }
        public List<PortalPair> PortalPairs { get; private set; }
        public Dictionary<Vector2Int, PortalPair> Portals { get; private set; }
        public int PlayerId { get; set; }
        public int NextEntityId { get; set; }
        public GridDirection Facing { get; set; }
        public bool HasExit { get; set; }
        public Vector2Int ExitCell { get; set; }
        public int MoveCount { get; set; }
        public int PushCount { get; set; }
        public int ActionCount { get; set; }

        public GridEntity Player
        {
            get { return GetEntity(PlayerId); }
        }

        public GridBoardState Clone()
        {
            var clone = new GridBoardState(Width, Height)
            {
                PlayerId = PlayerId,
                NextEntityId = NextEntityId,
                Facing = Facing,
                HasExit = HasExit,
                ExitCell = ExitCell,
                MoveCount = MoveCount,
                PushCount = PushCount,
                ActionCount = ActionCount
            };

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    clone.tiles[x, y] = tiles[x, y];
                }
            }

            for (var i = 0; i < Entities.Count; i++)
            {
                clone.Entities.Add(Entities[i].Clone());
            }

            for (var i = 0; i < Goals.Count; i++)
            {
                clone.Goals.Add(Goals[i].Clone());
            }

            for (var i = 0; i < PortalPairs.Count; i++)
            {
                clone.PortalPairs.Add(PortalPairs[i].Clone());
            }

            clone.RefreshPortals();
            return clone;
        }

        public void RestoreFrom(GridBoardState source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Width != Width || source.Height != Height)
            {
                throw new InvalidOperationException("Cannot restore a board into a different grid size.");
            }

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    tiles[x, y] = source.tiles[x, y];
                }
            }

            Entities.Clear();
            for (var i = 0; i < source.Entities.Count; i++)
            {
                Entities.Add(source.Entities[i].Clone());
            }

            Goals.Clear();
            for (var i = 0; i < source.Goals.Count; i++)
            {
                Goals.Add(source.Goals[i].Clone());
            }

            Portals.Clear();
            PortalPairs.Clear();
            for (var i = 0; i < source.PortalPairs.Count; i++)
            {
                PortalPairs.Add(source.PortalPairs[i].Clone());
            }

            PlayerId = source.PlayerId;
            NextEntityId = source.NextEntityId;
            Facing = source.Facing;
            HasExit = source.HasExit;
            ExitCell = source.ExitCell;
            MoveCount = source.MoveCount;
            PushCount = source.PushCount;
            ActionCount = source.ActionCount;
            RefreshPortals();
        }

        public void RefreshPortals()
        {
            Portals.Clear();
            for (var i = 0; i < PortalPairs.Count; i++)
            {
                var pair = PortalPairs[i];
                var entry = pair.ResolveEntry(this);
                var exit = pair.ResolveExit(this);
                if (InBounds(entry) && InBounds(exit))
                {
                    Portals[entry] = pair;
                }
            }
        }

        public bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public TileKind GetTile(Vector2Int cell)
        {
            return InBounds(cell) ? tiles[cell.x, cell.y] : TileKind.Void;
        }

        public void SetTile(Vector2Int cell, TileKind tile)
        {
            if (InBounds(cell))
            {
                tiles[cell.x, cell.y] = tile;
            }
        }

        public bool IsWalkable(Vector2Int cell)
        {
            var tile = GetTile(cell);
            return tile != TileKind.Void && tile != TileKind.Wall;
        }

        public GridEntity GetEntity(int id)
        {
            for (var i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Id == id)
                {
                    return Entities[i];
                }
            }

            return null;
        }

        public GridEntity FindEntityAt(Vector2Int cell)
        {
            for (var i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Occupies(cell))
                {
                    return Entities[i];
                }
            }

            return null;
        }

        public bool IsSolved()
        {
            for (var i = 0; i < Goals.Count; i++)
            {
                var goal = Goals[i];
                var entity = FindEntityAt(goal.Cell);
                if (entity == null ||
                    entity.Kind != EntityKind.Matter ||
                    entity.GetMatterAt(goal.Cell) != goal.Matter)
                {
                    return false;
                }

                if (goal.RequiresCompound && entity.Cells.Count < 2)
                {
                    return false;
                }
            }

            return !HasExit || Player.Occupies(ExitCell);
        }

        public int SatisfiedGoalCount()
        {
            var count = 0;
            for (var i = 0; i < Goals.Count; i++)
            {
                var goal = Goals[i];
                var entity = FindEntityAt(goal.Cell);
                if (entity != null &&
                    entity.Kind == EntityKind.Matter &&
                    entity.GetMatterAt(goal.Cell) == goal.Matter &&
                    (!goal.RequiresCompound || entity.Cells.Count >= 2))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
