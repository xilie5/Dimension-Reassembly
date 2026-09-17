using UnityEngine;

namespace CompoundBox
{
    public sealed class GameHud : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle centreStyle;
        private GUIStyle completeStyle;
        private bool stylesReady;
        private LevelDefinition level;
        private int levelIndex;
        private int levelCount;
        private GridSession session;

        public void SetState(LevelDefinition definition, int index, int count, GridSession gridSession)
        {
            level = definition;
            levelIndex = index;
            levelCount = count;
            session = gridSession;
        }

        private void OnGUI()
        {
            if (level == null || session == null)
            {
                return;
            }

            EnsureStyles();
            DrawPanel(new Rect(22f, 20f, 440f, 96f), new Color(0.055f, 0.075f, 0.105f, 0.94f));
            GUI.Label(new Rect(42f, 34f, 300f, 30f), $"{levelIndex + 1:00}  {level.DisplayName}", titleStyle);
            GUI.Label(new Rect(42f, 68f, 390f, 28f), level.Subtitle, smallStyle);
            GUI.Label(
                new Rect(488f, 34f, 280f, 26f),
                $"GOALS  {session.State.SatisfiedGoalCount():00}/{session.State.Goals.Count:00}",
                bodyStyle);
            GUI.Label(
                new Rect(488f, 66f, 280f, 26f),
                $"MOVES  {session.State.MoveCount:000}   PUSHES  {session.State.PushCount:000}",
                smallStyle);
            GUI.Label(
                new Rect(792f, 34f, 240f, 26f),
                $"ARCHIVE  {SaveService.Data.highestUnlockedLevel:00}/{levelCount:00}",
                bodyStyle);
            GUI.Label(
                new Rect(792f, 66f, 300f, 26f),
                SaveService.Data.TryGetRecord(level.Id, out var progress) && progress.completed
                    ? $"LOCAL RECORD  {progress.bestMoves:000}/{progress.bestPushes:000}"
                    : "NO LOCAL RECORD",
                smallStyle);

            var controls = "WASD / ARROWS  MOVE     X  SPLIT     C  RECOMBINE     Z  UNDO     R  RESTART";
            DrawPanel(new Rect(22f, Screen.height - 58f, Mathf.Min(Screen.width - 44f, 760f), 36f),
                new Color(0.055f, 0.075f, 0.105f, 0.9f));
            GUI.Label(new Rect(38f, Screen.height - 53f, 730f, 26f), controls, smallStyle);

            if (session.State.IsSolved())
            {
                DrawCompletion();
            }
        }

        private void DrawCompletion()
        {
            var width = Mathf.Min(560f, Screen.width - 40f);
            var height = 180f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            DrawPanel(rect, new Color(0.05f, 0.07f, 0.1f, 0.98f));
            GUI.Label(new Rect(rect.x, rect.y + 30f, rect.width, 46f), "PUZZLE COMPLETE", completeStyle);
            GUI.Label(
                new Rect(rect.x, rect.y + 86f, rect.width, 28f),
                "The mass configuration is stable.",
                centreStyle);
            GUI.Label(
                new Rect(rect.x, rect.y + 124f, rect.width, 24f),
                levelIndex + 1 < levelCount ? "Press N for the next chamber." : "All chambers complete. Press N to loop.",
                smallStyle);
        }

        private void EnsureStyles()
        {
            if (stylesReady)
            {
                return;
            }

            var font = Font.CreateDynamicFontFromOSFont(
                new[] { "Bahnschrift", "Arial", "Segoe UI" },
                20);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.93f, 1f) }
            };
            smallStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 13,
                normal = { textColor = new Color(0.62f, 0.7f, 0.8f) }
            };
            centreStyle = new GUIStyle(smallStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            };
            smallStyle.alignment = TextAnchor.MiddleLeft;
            completeStyle = new GUIStyle(titleStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28
            };
            stylesReady = true;
        }

        private static void DrawPanel(Rect rect, Color colour)
        {
            var previous = GUI.color;
            GUI.color = colour;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
