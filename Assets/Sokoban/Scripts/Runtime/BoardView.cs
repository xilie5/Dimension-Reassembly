using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public sealed class BoardView : MonoBehaviour
    {
        private const float CustomFloorWorldScale = 1.24f;
        private const float CustomWallWorldScale = 1.15f;
        private const float CustomCellPieceWorldScale = 1.24f;
        private static readonly Color CompoundGoalMarkerColour = new Color(1f, 0.82f, 0.35f, 1f);

        private sealed class EntityVisual
        {
            public int Id;
            public EntityKind Kind;
            public MatterType Matter;
            public Transform Root;
            public List<Vector2Int> LocalCells;
            public List<MatterType> LocalMatter;
            public List<EntityCellView> CellViews;
        }

        private readonly Dictionary<int, EntityVisual> entityVisuals = new Dictionary<int, EntityVisual>();
        private readonly List<Coroutine> runningAnimations = new List<Coroutine>();
        private Transform tileRoot;
        private Transform entityRoot;
        private Transform previewRoot;
        private ObjectPool<Transform> entityRootPool;
        private ObjectPool<EntityCellView> cellViewPool;
        private ObjectPool<SpriteRenderer> previewCellPool;
        private readonly List<SpriteRenderer> previewCells = new List<SpriteRenderer>();
        private Camera gameCamera;
        private IBoardVisualTheme visualTheme;
        private GridBoardState currentState;
        private GridBoardState previewState;
        private GridDirection previewDirection;
        private int previewActionCount = -1;
        private int builtWidth = -1;
        private int builtHeight = -1;

        public void Initialize(Camera camera, IBoardVisualTheme theme = null)
        {
            gameCamera = camera;
            visualTheme = theme;
            tileRoot = new GameObject("Tiles").transform;
            tileRoot.SetParent(transform, false);
            entityRoot = new GameObject("Entities").transform;
            entityRoot.SetParent(transform, false);
            previewRoot = new GameObject("Move Preview").transform;
            previewRoot.SetParent(transform, false);
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
            previewCellPool = new ObjectPool<SpriteRenderer>(
                () => new GameObject("Preview Cell").AddComponent<SpriteRenderer>(),
                renderer =>
                {
                    renderer.gameObject.SetActive(true);
                    renderer.transform.localScale = Vector3.one;
                    renderer.transform.localRotation = Quaternion.identity;
                },
                renderer =>
                {
                    renderer.gameObject.SetActive(false);
                    renderer.transform.SetParent(null, false);
                });
            previewCellPool.Prewarm(24);
        }

        public void SetState(GridBoardState state, ActionResolution resolution)
        {
            ClearMovePreview();
            currentState = state;
            if (builtWidth != state.Width || builtHeight != state.Height)
            {
                RebuildTiles(state);
                ClearEntities();
            }

            ReconcileEntities(state, resolution);
            FrameCamera(state);
        }

        public void ShowMovePreview(GridBoardState state, GridDirection direction)
        {
            if (previewState == state &&
                previewDirection == direction &&
                previewActionCount == state.ActionCount)
            {
                return;
            }

            ClearMovePreview();
            if (direction == GridDirection.None)
            {
                return;
            }

            previewState = state;
            previewDirection = direction;
            previewActionCount = state.ActionCount;

            var preview = GridBoardSimulation.PreviewMove(state, direction);
            var invalidColour = new Color(1f, 0.25f, 0.28f, 0.3f);
            for (var entityIndex = 0; entityIndex < preview.Entities.Count; entityIndex++)
            {
                var entityPreview = preview.Entities[entityIndex];
                for (var cellIndex = 0; cellIndex < entityPreview.TargetCells.Count; cellIndex++)
                {
                    var cell = entityPreview.TargetCells[cellIndex];
                    var renderer = previewCellPool.Get();
                    renderer.transform.SetParent(previewRoot, false);
                    renderer.transform.position = CellCentre(cell.Position);
                    renderer.sprite = entityPreview.Kind == EntityKind.Player
                        ? WhiteboxSprites.Circle
                        : WhiteboxSprites.RoundedSquare;
                    renderer.color = preview.IsValid
                        ? WithAlpha(GridPalette.Matter(cell.Matter), 0.32f)
                        : invalidColour;
                    renderer.sortingOrder = 20 + cellIndex;
                    renderer.transform.localScale = entityPreview.Kind == EntityKind.Player
                        ? new Vector3(0.72f, 0.72f, 1f)
                        : new Vector3(0.78f, 0.78f, 1f);
                    previewCells.Add(renderer);
                }
            }
        }

        public void ClearMovePreview()
        {
            for (var i = 0; i < previewCells.Count; i++)
            {
                previewCellPool.Release(previewCells[i]);
            }

            previewCells.Clear();
            previewState = null;
            previewDirection = GridDirection.None;
            previewActionCount = -1;
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

                    var baseTile = CreateRenderer(
                        $"Base Tile {x},{y}",
                        tileRoot,
                        WhiteboxSprites.Square,
                        CellCentre(cell),
                        (x + y) % 2 == 0 ? GridPalette.FloorA : GridPalette.FloorB,
                        -1);
                    baseTile.transform.localScale = new Vector3(1.02f, 1.02f, 1f);

                    var floor = CreateRenderer(
                        $"Tile {x},{y}",
                        tileRoot,
                        FloorSprite,
                        CellCentre(cell),
                        (x + y) % 2 == 0 ? GridPalette.FloorA : GridPalette.FloorB,
                        0);
                    var floorScale = visualTheme?.FloorSprite != null ? CustomFloorWorldScale : 0.96f;
                    floor.transform.localScale = new Vector3(floorScale, floorScale, 1f);

                    switch (tile)
                    {
                        case TileKind.Wall:
                            var wall = CreateRenderer(
                                "Wall",
                                tileRoot,
                                WallSprite,
                                CellCentre(cell) + new Vector3(0f, 0.08f, 0f),
                                GridPalette.Wall,
                                2);
                            var wallScale = visualTheme?.WallSprite != null ? CustomWallWorldScale : 0.94f;
                            wall.transform.localScale = new Vector3(wallScale, wallScale, 1f);
                            if (visualTheme?.WallSprite == null)
                            {
                                var edge = CreateRenderer(
                                    "Edge",
                                    wall.transform,
                                    WhiteboxSprites.Square,
                                    new Vector3(0f, 0.29f, 0f),
                                    GridPalette.WallEdge,
                                    1);
                                edge.transform.localScale = new Vector3(0.72f, 0.08f, 1f);
                            }
                            break;
                        case TileKind.Goal:
                            DrawGoal(tileRoot, state, cell);
                            break;
                        case TileKind.Exit:
                            var exitRing = CreateRenderer(
                                "Exit",
                                tileRoot,
                                ExitSprite,
                                CellCentre(cell),
                                GridPalette.Exit,
                                3);
                            var exitScale = visualTheme?.ExitSprite != null
                                ? CustomCellPieceWorldScale
                                : 0.58f;
                            exitRing.transform.localScale = new Vector3(exitScale, exitScale, 1f);
                            var chevron = CreateRenderer(
                                "Exit Direction",
                                tileRoot,
                                WhiteboxSprites.Chevron,
                                CellCentre(cell) + new Vector3(0f, 0.02f, 0f),
                                GridPalette.Exit,
                                4);
                            chevron.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                            chevron.gameObject.SetActive(visualTheme?.ExitSprite == null);
                            break;
                        case TileKind.Portal:
                            DrawPortal(tileRoot, state, cell);
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
                GoalSprite,
                CellCentre(cell),
                GridPalette.Matter(goal.Matter),
                3);
            baseRing.transform.localScale = goal.RequiresCompound
                ? visualTheme?.GoalSprite != null
                    ? new Vector3(CustomCellPieceWorldScale, CustomCellPieceWorldScale, 1f)
                    : new Vector3(0.66f, 0.66f, 1f)
                : visualTheme?.GoalSprite != null
                    ? new Vector3(CustomCellPieceWorldScale, CustomCellPieceWorldScale, 1f)
                    : new Vector3(0.52f, 0.52f, 1f);

            var core = CreateRenderer(
                "Goal Core",
                parent,
                goal.RequiresCompound ? WhiteboxSprites.Diamond : WhiteboxSprites.Circle,
                CellCentre(cell),
                GridPalette.MatterDark(goal.Matter),
                2);
            core.transform.localScale = goal.RequiresCompound
                ? new Vector3(0.24f, 0.24f, 1f)
                : new Vector3(0.18f, 0.18f, 1f);
            core.gameObject.SetActive(visualTheme?.GoalSprite == null);

            if (!goal.RequiresCompound)
            {
                return;
            }

            var markerCentre = CellCentre(cell);
            var markerBack = CreateRenderer(
                "Goal Requirement",
                parent,
                WhiteboxSprites.RoundedSquare,
                markerCentre,
                new Color(0.02f, 0.035f, 0.05f, 0.9f),
                5);
            markerBack.transform.localScale = new Vector3(0.48f, 0.48f, 1f);

            var bond = CreateRenderer(
                "Goal Requirement Bond",
                parent,
                WhiteboxSprites.Square,
                markerCentre,
                CompoundGoalMarkerColour,
                6);
            bond.transform.localScale = new Vector3(0.24f, 0.065f, 1f);

            var leftNode = CreateRenderer(
                "Goal Requirement Node A",
                parent,
                WhiteboxSprites.Circle,
                markerCentre + new Vector3(-0.11f, 0f, 0f),
                CompoundGoalMarkerColour,
                7);
            leftNode.transform.localScale = new Vector3(0.16f, 0.16f, 1f);

            var rightNode = CreateRenderer(
                "Goal Requirement Node B",
                parent,
                WhiteboxSprites.Circle,
                markerCentre + new Vector3(0.11f, 0f, 0f),
                CompoundGoalMarkerColour,
                7);
            rightNode.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
        }

        private void DrawPortal(Transform parent, GridBoardState state, Vector2Int cell)
        {
            var colour = GridPalette.Portal('a');
            var isExit = false;
            for (var pairIndex = 0; pairIndex < state.PortalPairs.Count; pairIndex++)
            {
                var pair = state.PortalPairs[pairIndex];
                colour = GridPalette.Portal(pair.Id);
                if (pair.ResolveEntry(state) == cell)
                {
                    isExit = false;
                    break;
                }

                if (pair.ResolveExit(state) == cell)
                {
                    isExit = true;
                    break;
                }
            }

            var outer = CreateRenderer(
                "Portal",
                parent,
                PortalSprite,
                CellCentre(cell),
                colour,
                3);
            outer.transform.localScale = visualTheme?.PortalSprite != null
                ? new Vector3(CustomCellPieceWorldScale, CustomCellPieceWorldScale, 1f)
                : new Vector3(0.72f, 0.72f, 1f);

            var inner = CreateRenderer(
                isExit ? "Exit Core" : "Entry Core",
                parent,
                isExit ? WhiteboxSprites.Diamond : WhiteboxSprites.Circle,
                CellCentre(cell),
                colour,
                4);
            inner.transform.localScale = isExit
                ? new Vector3(0.25f, 0.25f, 1f)
                : new Vector3(0.2f, 0.2f, 1f);
            inner.gameObject.SetActive(visualTheme?.PortalSprite == null);
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
                LocalMatter = new List<MatterType>(entity.Cells.Count),
                CellViews = new List<EntityCellView>(entity.Cells.Count)
            };

            for (var i = 0; i < entity.Cells.Count; i++)
            {
                var local = entity.Cells[i] - entity.Anchor;
                var matter = entity.CellStates[i].Matter;
                visual.LocalCells.Add(local);
                visual.LocalMatter.Add(matter);
                var cellView = cellViewPool.Get();
                cellView.transform.SetParent(root, false);
                cellView.Configure(entity.Kind, matter, entity.Cells.Count > 1, local, visualTheme);
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
            if (visual.Kind != entity.Kind ||
                visual.LocalCells.Count != entity.Cells.Count ||
                visual.LocalMatter.Count != entity.CellStates.Count)
            {
                return false;
            }

            for (var i = 0; i < entity.Cells.Count; i++)
            {
                if (visual.LocalCells[i] != entity.Cells[i] - entity.Anchor ||
                    visual.LocalMatter[i] != entity.CellStates[i].Matter)
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
            var topInset = Mathf.Clamp(192f / Mathf.Max(1f, Screen.height), 0.11f, 0.23f);
            var bottomInset = Mathf.Clamp(108f / Mathf.Max(1f, Screen.height), 0.07f, 0.16f);
            var availableVertical = Mathf.Max(0.55f, 1f - topInset - bottomInset);
            var verticalSize = (state.Height * 0.5f + 0.8f) / availableVertical;
            var horizontalSize = (state.Width * 0.5f + 0.8f) / Mathf.Max(0.1f, gameCamera.aspect);
            gameCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize, 4f);
            var verticalOffset = gameCamera.orthographicSize * (topInset - bottomInset);
            gameCamera.transform.position = centre + new Vector3(0f, verticalOffset, 0f);
        }

        private static Vector3 CellCentre(Vector2Int cell)
        {
            return new Vector3(cell.x, cell.y, 0f);
        }

        private Sprite FloorSprite => visualTheme?.FloorSprite ?? WhiteboxSprites.Square;
        private Sprite WallSprite => visualTheme?.WallSprite ?? WhiteboxSprites.RoundedSquare;
        private Sprite GoalSprite => visualTheme?.GoalSprite ?? WhiteboxSprites.Ring;
        private Sprite ExitSprite => visualTheme?.ExitSprite ?? WhiteboxSprites.Ring;
        private Sprite PortalSprite => visualTheme?.PortalSprite ?? WhiteboxSprites.Ring;

        private static Color WithAlpha(Color colour, float alpha)
        {
            colour.a = alpha;
            return colour;
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
