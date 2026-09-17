using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public sealed class BoardView : MonoBehaviour
    {
        private sealed class EntityVisual
        {
            public int Id;
            public EntityKind Kind;
            public MatterType Matter;
            public Transform Root;
            public List<Vector2Int> LocalCells;
            public List<EntityCellView> CellViews;
        }

        private readonly Dictionary<int, EntityVisual> entityVisuals = new Dictionary<int, EntityVisual>();
        private readonly List<Coroutine> runningAnimations = new List<Coroutine>();
        private Transform tileRoot;
        private Transform entityRoot;
        private ObjectPool<Transform> entityRootPool;
        private ObjectPool<EntityCellView> cellViewPool;
        private Camera gameCamera;
        private GridBoardState currentState;
        private int builtWidth = -1;
        private int builtHeight = -1;

        public void Initialize(Camera camera)
        {
            gameCamera = camera;
            tileRoot = new GameObject("Tiles").transform;
            tileRoot.SetParent(transform, false);
            entityRoot = new GameObject("Entities").transform;
            entityRoot.SetParent(transform, false);
            entityRootPool = new ObjectPool<Transform>(
                () => new GameObject("Entity Visual").transform,
                root =>
                {
                    root.gameObject.SetActive(true);
                    root.localScale = Vector3.one;
                },
                root =>
                {
                    root.SetParent(null, false);
                    root.gameObject.SetActive(false);
                });
            entityRootPool.Prewarm(8);
            cellViewPool = new ObjectPool<EntityCellView>(
                () => new GameObject("Entity Cell").AddComponent<EntityCellView>(),
                view =>
                {
                    view.gameObject.SetActive(true);
                    view.transform.localPosition = Vector3.zero;
                },
                view =>
                {
                    view.ResetForPool();
                    view.transform.SetParent(null, false);
                });
            cellViewPool.Prewarm(32);
        }

        public void SetState(GridBoardState state, ActionResolution resolution)
        {
            currentState = state;
            if (builtWidth != state.Width || builtHeight != state.Height)
            {
                RebuildTiles(state);
                ClearEntities();
            }

            ReconcileEntities(state, resolution);
            FrameCamera(state);
        }

        public void Rebuild(GridBoardState state)
        {
            builtWidth = -1;
            builtHeight = -1;
            SetState(state, null);
        }

        private void RebuildTiles(GridBoardState state)
        {
            for (var childIndex = tileRoot.childCount - 1; childIndex >= 0; childIndex--)
            {
                Destroy(tileRoot.GetChild(childIndex).gameObject);
            }

            builtWidth = state.Width;
            builtHeight = state.Height;

            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    var tile = state.GetTile(cell);
                    if (tile == TileKind.Void)
                    {
                        continue;
                    }

                    var floor = CreateRenderer(
                        $"Tile {x},{y}",
                        tileRoot,
                        WhiteboxSprites.Square,
                        CellCentre(cell),
                        (x + y) % 2 == 0 ? GridPalette.FloorA : GridPalette.FloorB,
                        0);
                    floor.transform.localScale = new Vector3(0.96f, 0.96f, 1f);

                    switch (tile)
                    {
                        case TileKind.Wall:
                            var wall = CreateRenderer(
                                "Wall",
                                floor.transform,
                                WhiteboxSprites.RoundedSquare,
                                new Vector3(0f, 0.08f, 0f),
                                GridPalette.Wall,
                                2);
                            wall.transform.localScale = new Vector3(0.94f, 0.94f, 1f);
                            var edge = CreateRenderer(
                                "Edge",
                                wall.transform,
                                WhiteboxSprites.Square,
                                new Vector3(0f, 0.29f, 0f),
                                GridPalette.WallEdge,
                                1);
                            edge.transform.localScale = new Vector3(0.72f, 0.08f, 1f);
                            break;
                        case TileKind.Goal:
                            DrawGoal(floor.transform, state, cell);
                            break;
                        case TileKind.Exit:
                            var exitRing = CreateRenderer(
                                "Exit",
                                floor.transform,
                                WhiteboxSprites.Ring,
                                Vector3.zero,
                                GridPalette.Exit,
                                3);
                            exitRing.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
                            var chevron = CreateRenderer(
                                "Exit Direction",
                                floor.transform,
                                WhiteboxSprites.Chevron,
                                new Vector3(0f, 0.02f, 0f),
                                GridPalette.Exit,
                                4);
                            chevron.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                            break;
                        case TileKind.Portal:
                            DrawPortal(floor.transform, state, cell);
                            break;
                    }
                }
            }
        }

        private void DrawGoal(Transform parent, GridBoardState state, Vector2Int cell)
        {
            GoalDefinition goal = null;
            for (var i = 0; i < state.Goals.Count; i++)
            {
                if (state.Goals[i].Cell == cell)
                {
                    goal = state.Goals[i];
                    break;
                }
            }

            if (goal == null)
            {
                return;
            }

            var baseRing = CreateRenderer(
                "Goal",
                parent,
                WhiteboxSprites.Ring,
                Vector3.zero,
                GridPalette.Matter(goal.Matter),
                3);
            baseRing.transform.localScale = goal.RequiresCompound
                ? new Vector3(0.66f, 0.66f, 1f)
                : new Vector3(0.52f, 0.52f, 1f);

            var core = CreateRenderer(
                "Goal Core",
                parent,
                goal.RequiresCompound ? WhiteboxSprites.Diamond : WhiteboxSprites.Circle,
                Vector3.zero,
                GridPalette.MatterDark(goal.Matter),
                2);
            core.transform.localScale = goal.RequiresCompound
                ? new Vector3(0.24f, 0.24f, 1f)
                : new Vector3(0.18f, 0.18f, 1f);
        }

        private void DrawPortal(Transform parent, GridBoardState state, Vector2Int cell)
        {
            var colour = GridPalette.Portal('a');
            var isExit = false;
            foreach (var pair in state.Portals.Values)
            {
                colour = GridPalette.Portal(pair.Id);
                if (pair.Entry == cell)
                {
                    isExit = false;
                    break;
                }

                if (pair.Exit == cell)
                {
                    isExit = true;
                    break;
                }
            }

            var outer = CreateRenderer(
                "Portal",
                parent,
                WhiteboxSprites.Ring,
                Vector3.zero,
                colour,
                3);
            outer.transform.localScale = new Vector3(0.72f, 0.72f, 1f);

            var inner = CreateRenderer(
                isExit ? "Exit Core" : "Entry Core",
                parent,
                isExit ? WhiteboxSprites.Diamond : WhiteboxSprites.Circle,
                Vector3.zero,
                colour,
                4);
            inner.transform.localScale = isExit
                ? new Vector3(0.25f, 0.25f, 1f)
                : new Vector3(0.2f, 0.2f, 1f);
        }

        private void ReconcileEntities(GridBoardState state, ActionResolution resolution)
        {
            var liveIds = new HashSet<int>();
            for (var entityIndex = 0; entityIndex < state.Entities.Count; entityIndex++)
            {
                var entity = state.Entities[entityIndex];
                liveIds.Add(entity.Id);

                if (!entityVisuals.TryGetValue(entity.Id, out var visual) || !Matches(visual, entity))
                {
                    RemoveVisual(entity.Id);
                    visual = CreateEntityVisual(entity);
                    entityVisuals.Add(entity.Id, visual);
                    runningAnimations.Add(StartCoroutine(PopIn(visual.Root)));
                    continue;
                }

                var target = CellCentre(entity.Anchor);
                if ((visual.Root.position - target).sqrMagnitude > 0.0001f)
                {
                    var duration = resolution != null && resolution.UsedPortal ? 0.16f : 0.085f;
                    runningAnimations.Add(StartCoroutine(AnimateTo(visual.Root, target, duration, visual.Kind == EntityKind.Player)));
                }
            }

            var staleIds = new List<int>();
            foreach (var pair in entityVisuals)
            {
                if (!liveIds.Contains(pair.Key))
                {
                    staleIds.Add(pair.Key);
                }
            }

            for (var i = 0; i < staleIds.Count; i++)
            {
                RemoveVisual(staleIds[i]);
            }
        }

        private EntityVisual CreateEntityVisual(GridEntity entity)
        {
            var root = entityRootPool.Get();
            root.name = entity.Kind == EntityKind.Player ? "Player" : $"Matter {entity.Id}";
            root.SetParent(entityRoot, false);
            root.position = CellCentre(entity.Anchor);

            var visual = new EntityVisual
            {
                Id = entity.Id,
                Kind = entity.Kind,
                Matter = entity.Matter,
                Root = root,
                LocalCells = new List<Vector2Int>(),
                CellViews = new List<EntityCellView>(entity.Cells.Count)
            };

            for (var i = 0; i < entity.Cells.Count; i++)
            {
                var local = entity.Cells[i] - entity.Anchor;
                visual.LocalCells.Add(local);
                var cellView = cellViewPool.Get();
                cellView.transform.SetParent(root, false);
                cellView.Configure(entity.Kind, entity.Matter, entity.Cells.Count > 1, local);
                visual.CellViews.Add(cellView);
            }

            return visual;
        }

        private static SpriteRenderer CreateRenderer(
            string name,
            Transform parent,
            Sprite sprite,
            Vector3 localPosition,
            Color colour,
            int sortingOrder)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = colour;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void RemoveVisual(int id)
        {
            if (!entityVisuals.TryGetValue(id, out var visual))
            {
                return;
            }

            if (visual.Root != null)
            {
                if (visual.CellViews != null)
                {
                    for (var i = 0; i < visual.CellViews.Count; i++)
                    {
                        cellViewPool.Release(visual.CellViews[i]);
                    }
                }

                entityRootPool.Release(visual.Root);
            }

            entityVisuals.Remove(id);
        }

        private void ClearEntities()
        {
            foreach (var pair in entityVisuals)
            {
                if (pair.Value.Root != null)
                {
                    for (var i = 0; i < pair.Value.CellViews.Count; i++)
                    {
                        cellViewPool.Release(pair.Value.CellViews[i]);
                    }

                    entityRootPool.Release(pair.Value.Root);
                }
            }

            entityVisuals.Clear();
        }

        private static bool Matches(EntityVisual visual, GridEntity entity)
        {
            if (visual.Kind != entity.Kind || visual.Matter != entity.Matter || visual.LocalCells.Count != entity.Cells.Count)
            {
                return false;
            }

            for (var i = 0; i < entity.Cells.Count; i++)
            {
                if (visual.LocalCells[i] != entity.Cells[i] - entity.Anchor)
                {
                    return false;
                }
            }

            return true;
        }

        private void FrameCamera(GridBoardState state)
        {
            if (gameCamera == null)
            {
                return;
            }

            var centre = new Vector3((state.Width - 1) * 0.5f, (state.Height - 1) * 0.5f, -10f);
            gameCamera.transform.position = centre;
            var verticalSize = state.Height * 0.5f + 1.2f;
            var horizontalSize = (state.Width * 0.5f + 1.2f) / Mathf.Max(0.1f, gameCamera.aspect);
            gameCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize, 4f);
        }

        private static Vector3 CellCentre(Vector2Int cell)
        {
            return new Vector3(cell.x, cell.y, 0f);
        }

        private IEnumerator PopIn(Transform root)
        {
            root.localScale = Vector3.one * 0.82f;
            var elapsed = 0f;
            const float duration = 0.12f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                root.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, 1f - Mathf.Pow(1f - t, 3f));
                yield return null;
            }

            root.localScale = Vector3.one;
        }

        private static IEnumerator AnimateTo(Transform target, Vector3 destination, float duration, bool bob)
        {
            var start = target.position;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);
                var position = Vector3.LerpUnclamped(start, destination, eased);
                if (bob)
                {
                    position.y += Mathf.Sin(t * Mathf.PI) * 0.08f;
                }

                target.position = position;
                yield return null;
            }

            target.position = destination;
        }
    }
}
