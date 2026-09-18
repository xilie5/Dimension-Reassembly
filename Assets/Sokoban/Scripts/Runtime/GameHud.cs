using System;
using UnityEngine;

namespace CompoundBox
{
    public sealed class GameHud : MonoBehaviour
    {
        private const float CanvasWidth = 1920f;
        private const float CanvasHeight = 1080f;

        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle centreStyle;
        private GUIStyle completeStyle;
        private GUIStyle chapterButtonStyle;
        private bool stylesReady;

        private LevelDefinition level;
        private int levelIndex;
        private int levelCount;
        private GridSession session;
        private Action<int> levelSelected;
        private Action continueAction;
        private Action newGameAction;
        private bool levelSelectOpen;
        private ArtThemeAsset theme;
        private bool mainMenuOpen;

        public void SetState(
            LevelDefinition definition,
            int index,
            int count,
            GridSession gridSession,
            Action<int> selectLevel)
        {
            level = definition;
            levelIndex = index;
            levelCount = count;
            session = gridSession;
            levelSelected = selectLevel;
        }

        public bool IsLevelSelectOpen => levelSelectOpen;
        public bool IsMainMenuOpen => mainMenuOpen;

        public void SetMainMenuCallbacks(Action onContinue, Action onNewGame)
        {
            continueAction = onContinue;
            newGameAction = onNewGame;
        }

        public void SetMainMenuVisible(bool visible)
        {
            mainMenuOpen = visible;
        }

        public void SetTheme(ArtThemeAsset artTheme)
        {
            theme = artTheme;
            stylesReady = false;
        }

        public void ToggleLevelSelect()
        {
            levelSelectOpen = !levelSelectOpen;
        }

        private void OnGUI()
        {
            if (level == null || session == null)
            {
                return;
            }

            EnsureStyles();
            var scale = Mathf.Min(Screen.width / CanvasWidth, Screen.height / CanvasHeight);
            var offset = new Vector3(
                (Screen.width - CanvasWidth * scale) * 0.5f,
                (Screen.height - CanvasHeight * scale) * 0.5f,
                0f);
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale);

            if (mainMenuOpen)
            {
                DrawMainMenu();
            }
            else
            {
                DrawTopBar();
                DrawBottomBar();

                if (session.State.IsSolved())
                {
                    DrawCompletion();
                }
            }

            if (levelSelectOpen)
            {
                DrawLevelSelect();
            }

            GUI.matrix = previousMatrix;
        }

        private void DrawMainMenu()
        {
            Fill(new Rect(0f, 0f, CanvasWidth, CanvasHeight), new Color(0.02f, 0.03f, 0.05f, 0.94f));
            var panel = new Rect(560f, 220f, 800f, 600f);
            DrawCard(panel, new Color(0.3f, 0.82f, 0.9f, 1f));
            GUI.Label(new Rect(panel.x, panel.y + 42f, panel.width, 56f), "COMPOUND BOX", completeStyle);
            GUI.Label(
                new Rect(panel.x, panel.y + 104f, panel.width, 30f),
                "PHASE FOUNDRY  /  MATTER ENGINEERING",
                centreStyle);

            var button = new Rect(panel.x + 180f, panel.y + 190f, panel.width - 360f, 64f);
            if (GUI.Button(button, "CONTINUE", chapterButtonStyle))
            {
                mainMenuOpen = false;
                continueAction?.Invoke();
            }

            button.y += 82f;
            if (GUI.Button(button, "CHAPTER SELECT", chapterButtonStyle))
            {
                levelSelectOpen = true;
            }

            button.y += 82f;
            if (GUI.Button(button, "NEW GAME", chapterButtonStyle))
            {
                mainMenuOpen = false;
                newGameAction?.Invoke();
            }

            GUI.Label(
                new Rect(panel.x, panel.yMax - 52f, panel.width, 28f),
                "WASD MOVE  /  X SPLIT  /  V CUT  /  Q E ROTATE  /  C FUSE  /  L CHAPTERS",
                centreStyle);
        }

        private void DrawTopBar()
        {
            var levelRect = new Rect(28f, 24f, 560f, 140f);
            var statsRect = new Rect(608f, 24f, 380f, 140f);
            var archiveRect = new Rect(1008f, 24f, 380f, 140f);
            var chapterRect = new Rect(1408f, 24f, 484f, 140f);

            DrawCard(levelRect, new Color(0.2f, 0.78f, 0.88f, 1f));
            GUI.Label(
                new Rect(levelRect.x + 24f, levelRect.y + 16f, levelRect.width - 48f, 24f),
                $"CHAPTER {levelIndex + 1:00}  /  {ChapterUtility.GetChapterName(levelIndex).ToUpperInvariant()}",
                smallStyle);
            GUI.Label(
                new Rect(levelRect.x + 24f, levelRect.y + 42f, levelRect.width - 48f, 42f),
                level.DisplayName,
                titleStyle);
            GUI.Label(
                new Rect(levelRect.x + 24f, levelRect.y + 88f, levelRect.width - 48f, 38f),
                level.Subtitle,
                smallStyle);

            DrawCard(statsRect, new Color(0.36f, 0.56f, 0.95f, 1f));
            GUI.Label(new Rect(statsRect.x + 24f, statsRect.y + 18f, 300f, 28f), "CHAMBER STATUS", smallStyle);
            GUI.Label(
                new Rect(statsRect.x + 24f, statsRect.y + 48f, 330f, 34f),
                $"GOALS  {session.State.SatisfiedGoalCount():00} / {session.State.Goals.Count:00}",
                headingStyle);
            GUI.Label(
                new Rect(statsRect.x + 24f, statsRect.y + 86f, 330f, 28f),
                $"MOVES  {session.State.MoveCount:000}     PUSHES  {session.State.PushCount:000}",
                bodyStyle);

            DrawCard(archiveRect, new Color(0.95f, 0.72f, 0.34f, 1f));
            var unlocked = Mathf.Clamp(SaveService.Data.highestUnlockedLevel, 1, levelCount);
            GUI.Label(new Rect(archiveRect.x + 24f, archiveRect.y + 18f, 320f, 28f), "LOCAL ARCHIVE", smallStyle);
            GUI.Label(
                new Rect(archiveRect.x + 24f, archiveRect.y + 48f, 330f, 34f),
                $"UNLOCKED  {unlocked:00} / {levelCount:00}",
                headingStyle);
            var recordText = SaveService.Data.TryGetRecord(level.Id, out var levelProgress) && levelProgress.completed
                ? $"BEST  {levelProgress.bestMoves:000} MOVES  /  {levelProgress.bestPushes:000} PUSHES"
                : "NO COMPLETION RECORD";
            GUI.Label(
                new Rect(archiveRect.x + 24f, archiveRect.y + 86f, 330f, 28f),
                recordText,
                smallStyle);

            if (GUI.Button(
                    new Rect(chapterRect.x + 20f, chapterRect.y + 18f, chapterRect.width - 40f, chapterRect.height - 36f),
                    $"CHAPTER SELECT\n{unlocked:00} CHAMBERS AVAILABLE    [L]",
                    chapterButtonStyle))
            {
                levelSelectOpen = true;
            }

            var progress = levelCount <= 1 ? 0f : levelIndex / (float)(levelCount - 1);
            var track = new Rect(28f, 180f, CanvasWidth - 56f, 8f);
            Fill(track, new Color(0.12f, 0.17f, 0.23f, 0.9f));
            Fill(new Rect(track.x, track.y, track.width * progress, track.height), new Color(0.34f, 0.82f, 0.9f, 1f));
        }

        private void DrawBottomBar()
        {
            var rect = new Rect(28f, 984f, CanvasWidth - 56f, 72f);
            DrawCard(rect, new Color(0.25f, 0.72f, 0.86f, 0.8f));
            GUI.Label(
                new Rect(rect.x + 22f, rect.y + 8f, rect.width - 44f, 28f),
                "MOVE  WASD / ARROWS      SPLIT  X      PRECISION CUT  V      ROTATE  Q / E      FUSE  C",
                bodyStyle);
            GUI.Label(
                new Rect(rect.x + 22f, rect.y + 38f, rect.width - 44f, 24f),
                "UNDO  Z / CTRL+Z      REDO  CTRL+Y / CTRL+SHIFT+Z      RESTART  R      CHAPTERS  L      NEXT  N      MUTE  M",
                smallStyle);
        }

        private void DrawCompletion()
        {
            var rect = new Rect(580f, 388f, 760f, 286f);
            DrawCard(rect, new Color(0.28f, 0.86f, 0.68f, 1f));
            GUI.Label(new Rect(rect.x, rect.y + 34f, rect.width, 54f), "CHAMBER STABILIZED", completeStyle);
            GUI.Label(
                new Rect(rect.x, rect.y + 102f, rect.width, 34f),
                "The matter configuration matches the machine specification.",
                centreStyle);
            GUI.Label(
                new Rect(rect.x, rect.y + 154f, rect.width, 30f),
                $"MOVES  {session.State.MoveCount:000}     PUSHES  {session.State.PushCount:000}",
                headingStyle);
            GUI.Label(
                new Rect(rect.x, rect.y + 220f, rect.width, 28f),
                levelIndex + 1 < levelCount
                    ? "Press N for the next chamber."
                    : "All chambers stabilized. Press N to return to the first.",
                bodyStyle);
        }

        private void DrawLevelSelect()
        {
            var rect = new Rect(300f, 176f, 1320f, 700f);
            Fill(new Rect(0f, 0f, CanvasWidth, CanvasHeight), new Color(0.02f, 0.03f, 0.05f, 0.72f));
            DrawCard(rect, new Color(0.3f, 0.78f, 0.9f, 1f));
            GUI.Label(new Rect(rect.x + 36f, rect.y + 28f, rect.width - 72f, 46f), "CHAMBER SELECT", titleStyle);
            GUI.Label(
                new Rect(rect.x + 36f, rect.y + 76f, rect.width - 72f, 28f),
                "Completed chapters and the next available chamber are unlocked.",
                bodyStyle);

            var unlocked = Mathf.Clamp(SaveService.Data.highestUnlockedLevel, 1, levelCount);
            const int columns = 4;
            const float gap = 14f;
            var startX = rect.x + 36f;
            var startY = rect.y + 126f;
            var cardWidth = (rect.width - 72f - (columns - 1) * gap) / columns;
            const float cardHeight = 138f;

            for (var index = 0; index < levelCount; index++)
            {
                var row = index / columns;
                var column = index % columns;
                var card = new Rect(
                    startX + column * (cardWidth + gap),
                    startY + row * (cardHeight + gap),
                    cardWidth,
                    cardHeight);
                var available = index < unlocked;
                var previousEnabled = GUI.enabled;
                GUI.enabled = available;
                var label =
                    $"{index + 1:00}  {ChapterUtility.GetChapterName(index)}\n" +
                    $"{(available ? "READY" : "LOCKED")}";
                if (GUI.Button(card, label, chapterButtonStyle))
                {
                    levelSelected?.Invoke(index);
                    levelSelectOpen = false;
                }

                GUI.enabled = previousEnabled;
            }

            GUI.Label(
                new Rect(rect.x + 36f, rect.yMax - 46f, rect.width - 72f, 26f),
                "Press L or click a chamber to close.",
                smallStyle);
        }

        private void EnsureStyles()
        {
            if (stylesReady)
            {
                return;
            }

            var font = Font.CreateDynamicFontFromOSFont(
                new[] { "Bahnschrift SemiBold", "Bahnschrift", "Arial", "Segoe UI" },
                24);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.96f, 0.98f, 1f) }
            };
            headingStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.96f, 0.98f, 1f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.72f, 0.82f, 0.92f) }
            };
            smallStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.62f, 0.74f, 0.86f) }
            };
            centreStyle = new GUIStyle(bodyStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            completeStyle = new GUIStyle(titleStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 38
            };
            chapterButtonStyle = new GUIStyle(GUI.skin.button)
            {
                font = font,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.92f, 0.97f, 1f) },
                hover = { textColor = Color.white },
                active = { textColor = Color.white }
            };
            if (theme != null)
            {
                chapterButtonStyle.normal.background = theme.ButtonTexture;
                chapterButtonStyle.hover.background =
                    theme.ButtonHoverTexture != null ? theme.ButtonHoverTexture : theme.ButtonTexture;
                chapterButtonStyle.active.background =
                    theme.ButtonHoverTexture != null ? theme.ButtonHoverTexture : theme.ButtonTexture;
            }

            stylesReady = true;
        }

        private static void DrawCard(Rect rect, Color accent)
        {
            Fill(new Rect(rect.x + 3f, rect.y + 4f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.22f));
            Fill(rect, new Color(0.045f, 0.065f, 0.09f, 0.97f));
            Fill(new Rect(rect.x, rect.y, rect.width, 5f), accent);
            Fill(new Rect(rect.x, rect.y, 2f, rect.height), new Color(accent.r, accent.g, accent.b, 0.45f));
        }

        private static void Fill(Rect rect, Color colour)
        {
            var previous = GUI.color;
            GUI.color = colour;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
