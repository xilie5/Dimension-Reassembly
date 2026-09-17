using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public static class LevelParser
    {
        public static GridBoardState Parse(LevelDefinition definition)
        {
            var height = definition.Rows.Length;
            var width = 0;
            for (var i = 0; i < height; i++)
            {
                width = Mathf.Max(width, definition.Rows[i].Length);
            }

            var state = new GridBoardState(width, height);
            var payloadCells = new List<KeyValuePair<Vector2Int, MatterType>>();
            var portalEntries = new Dictionary<char, Vector2Int>();
            var portalExits = new Dictionary<char, Vector2Int>();
            var advancedLayout = definition.MaterialRows != null && definition.MaterialRows.Length > 0;

            for (var row = 0; row < height; row++)
            {
                var line = definition.Rows[row];
                for (var x = 0; x < line.Length; x++)
                {
                    var cell = new Vector2Int(x, height - 1 - row);
                    var symbol = line[x];

                    if (MatterTypeUtility.TryParseBlock(symbol, out var blockMatter))
                    {
                        blockMatter = GetMaterialOverride(definition, cell, blockMatter, advancedLayout);
                        state.SetTile(cell, TileKind.Floor);
                        payloadCells.Add(new KeyValuePair<Vector2Int, MatterType>(cell, blockMatter));
                        continue;
                    }

                    var isGenericGoal = symbol == 'g' || symbol == 'G';
                    var goalMatter = MatterType.None;
                    var requiresCompound = false;
                    if (isGenericGoal ||
                        MatterTypeUtility.TryParseGoal(symbol, out goalMatter, out requiresCompound))
                    {
                        if (isGenericGoal)
                        {
                            goalMatter = GetMaterialOverride(definition, cell, MatterType.None, advancedLayout);
                            requiresCompound = symbol == 'G';
                            if (goalMatter == MatterType.None)
                            {
                                throw new System.InvalidOperationException(
                                    $"Generic goal '{symbol}' needs a material digit in the material layer.");
                            }
                        }
                        else
                        {
                            goalMatter = GetMaterialOverride(definition, cell, goalMatter, advancedLayout);
                        }

                        state.SetTile(cell, TileKind.Goal);
                        state.Goals.Add(new GoalDefinition(cell, goalMatter, requiresCompound));
                        continue;
                    }

                    switch (symbol)
                    {
                        case '#':
                            state.SetTile(cell, TileKind.Wall);
                            break;
                        case '.':
                        case '@':
                            state.SetTile(cell, TileKind.Floor);
                            break;
                        case 'E':
                            state.SetTile(cell, TileKind.Exit);
                            state.HasExit = true;
                            state.ExitCell = cell;
                            break;
                        case ' ':
                            break;
                        default:
                            if (symbol >= 'a' && symbol <= 'd')
                            {
                                state.SetTile(cell, TileKind.Portal);
                                portalEntries[symbol] = cell;
                            }
                            else if (symbol >= 'A' && symbol <= 'D')
                            {
                                state.SetTile(cell, TileKind.Portal);
                                portalExits[char.ToLowerInvariant(symbol)] = cell;
                            }
                            else
                            {
                                throw new System.InvalidOperationException(
                                    $"Unsupported level symbol '{symbol}' in level '{definition.Id}'.");
                            }

                            break;
                    }

                    if (symbol == '@')
                    {
                        var player = new GridEntity(state.NextEntityId++, EntityKind.Player, MatterType.None, new[] { cell });
                        state.Entities.Add(player);
                        state.PlayerId = player.Id;
                    }
                }
            }

            BuildMatterEntities(state, payloadCells, advancedLayout);
            BuildPortalPairs(state, portalEntries, portalExits);
            return state;
        }

        private static MatterType GetMaterialOverride(
            LevelDefinition definition,
            Vector2Int cell,
            MatterType fallback,
            bool advancedLayout)
        {
            if (!advancedLayout)
            {
                return fallback;
            }

            var row = definition.Rows.Length - 1 - cell.y;
            if (row < 0 || row >= definition.MaterialRows.Length)
            {
                return fallback;
            }

            var line = definition.MaterialRows[row];
            if (cell.x < 0 || cell.x >= line.Length ||
                !MatterTypeUtility.TryParseBlock(line[cell.x], out var overrideMatter))
            {
                return fallback;
            }

            return overrideMatter;
        }

        private static void BuildMatterEntities(
            GridBoardState state,
            IReadOnlyList<KeyValuePair<Vector2Int, MatterType>> payloadCells,
            bool advancedLayout)
        {
            var unvisited = new HashSet<Vector2Int>();
            var matterByCell = new Dictionary<Vector2Int, MatterType>();
            for (var i = 0; i < payloadCells.Count; i++)
            {
                unvisited.Add(payloadCells[i].Key);
                matterByCell[payloadCells[i].Key] = payloadCells[i].Value;
            }

            var queue = new Queue<Vector2Int>();
            var component = new List<Vector2Int>();
            while (unvisited.Count > 0)
            {
                component.Clear();
                var seed = default(Vector2Int);
                foreach (var cell in unvisited)
                {
                    seed = cell;
                    break;
                }

                var matter = matterByCell[seed];
                queue.Enqueue(seed);
                unvisited.Remove(seed);

                while (queue.Count > 0)
                {
                    var cell = queue.Dequeue();
                    component.Add(cell);
                    if (advancedLayout)
                    {
                        TryVisitAdvanced(cell + Vector2Int.up, unvisited, queue);
                        TryVisitAdvanced(cell + Vector2Int.right, unvisited, queue);
                        TryVisitAdvanced(cell + Vector2Int.down, unvisited, queue);
                        TryVisitAdvanced(cell + Vector2Int.left, unvisited, queue);
                    }
                    else
                    {
                        TryVisit(cell + Vector2Int.up, matter, unvisited, matterByCell, queue);
                        TryVisit(cell + Vector2Int.right, matter, unvisited, matterByCell, queue);
                        TryVisit(cell + Vector2Int.down, matter, unvisited, matterByCell, queue);
                        TryVisit(cell + Vector2Int.left, matter, unvisited, matterByCell, queue);
                    }
                }

                if (advancedLayout)
                {
                    var cells = new List<EntityCellState>(component.Count);
                    for (var cellIndex = 0; cellIndex < component.Count; cellIndex++)
                    {
                        var cell = component[cellIndex];
                        cells.Add(new EntityCellState(cell, matterByCell[cell]));
                    }

                    state.Entities.Add(new GridEntity(state.NextEntityId++, EntityKind.Matter, cells));
                }
                else
                {
                    state.Entities.Add(new GridEntity(state.NextEntityId++, EntityKind.Matter, matter, component));
                }
            }
        }

        private static void TryVisit(
            Vector2Int candidate,
            MatterType matter,
            HashSet<Vector2Int> unvisited,
            Dictionary<Vector2Int, MatterType> matterByCell,
            Queue<Vector2Int> queue)
        {
            if (!unvisited.Contains(candidate) || matterByCell[candidate] != matter)
            {
                return;
            }

            unvisited.Remove(candidate);
            queue.Enqueue(candidate);
        }

        private static void TryVisitAdvanced(
            Vector2Int candidate,
            HashSet<Vector2Int> unvisited,
            Queue<Vector2Int> queue)
        {
            if (!unvisited.Contains(candidate))
            {
                return;
            }

            unvisited.Remove(candidate);
            queue.Enqueue(candidate);
        }

        private static void BuildPortalPairs(
            GridBoardState state,
            IReadOnlyDictionary<char, Vector2Int> portalEntries,
            IReadOnlyDictionary<char, Vector2Int> portalExits)
        {
            foreach (var pair in portalEntries)
            {
                if (!portalExits.TryGetValue(pair.Key, out var exit))
                {
                    throw new System.InvalidOperationException($"Portal '{pair.Key}' has no uppercase exit.");
                }

                var portal = new PortalPair(pair.Key, pair.Value, exit);
                state.Portals[pair.Value] = portal;
            }
        }
    }
}
