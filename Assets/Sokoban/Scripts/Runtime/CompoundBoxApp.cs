using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    [DisallowMultipleComponent]
    public sealed class CompoundBoxApp : MonoBehaviour
    {
        public static int RequestedStartLevel { get; set; }

        [SerializeField] private LevelCatalogAsset levelCatalog;
        [SerializeField] private ArtThemeAsset artTheme;

        private readonly List<LevelDefinition> levels = new List<LevelDefinition>();
        private readonly LevelFlowStateMachine levelFlow = new LevelFlowStateMachine();
        private readonly PlayerActionStateMachine playerState = new PlayerActionStateMachine();
        private BoardView boardView;
        private GameHud hud;
        private ProceduralAudio audioService;
        private CompoundBoxInput input;
        private GridSession session;
        private int levelIndex;
        private float completeAt = -1f;
        private float playerStateUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (FindObjectOfType<CompoundBoxApp>() != null)
            {
                return;
            }

            var root = new GameObject("Compound Box Application");
            root.AddComponent<CompoundBoxApp>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 120;
            artTheme = artTheme != null
                ? artTheme
                : Resources.Load<ArtThemeAsset>("Art/FoundryTheme");
            GridPalette.Theme = artTheme;
            LoadLevelContent();
            SaveService.EnsureLoaded();

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = GridPalette.Background;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var boardObject = new GameObject("Board");
            boardObject.transform.SetParent(transform, false);
            boardView = boardObject.AddComponent<BoardView>();
            boardView.Initialize(camera, artTheme);

            var hudObject = new GameObject("HUD");
            hudObject.transform.SetParent(transform, false);
            hud = hudObject.AddComponent<GameHud>();
            hud.SetTheme(artTheme);

            audioService = new ProceduralAudio(gameObject);
            audioService.ConfigureTheme(artTheme);
            audioService.Muted = SaveService.Data.audioMuted;
            input = new CompoundBoxInput();
            var startLevel = RequestedStartLevel >= 0
                ? RequestedStartLevel
                : SaveService.Data.highestUnlockedLevel - 1;
            RequestedStartLevel = -1;
            LoadLevel(Mathf.Clamp(startLevel, 0, levels.Count - 1));
            hud.SetMainMenuCallbacks(ContinueFromMenu, StartNewGame);
            hud.SetMainMenuVisible(true);
            levelFlow.Change(LevelFlowState.MainMenu);
        }

        private void OnDestroy()
        {
            input?.Dispose();
        }

        private void Update()
        {
            if (playerState.CurrentState != PlayerActionState.Idle && Time.unscaledTime >= playerStateUntil)
            {
                playerState.Change(PlayerActionState.Idle);
            }

            if (input.MutePressed)
            {
                audioService.Muted = !audioService.Muted;
                SaveService.SetAudioMuted(audioService.Muted);
                audioService.Play(AudioCue.Ui);
            }

            if (hud.IsMainMenuOpen)
            {
                if (input.LevelSelectPressed)
                {
                    hud.ToggleLevelSelect();
                }

                if (input.ConfirmPressed)
                {
                    ContinueFromMenu();
                }

                return;
            }

            if (input.AdvancePressed)
            {
                TryAdvanceLevel();
                return;
            }

            if (input.LevelSelectPressed)
            {
                hud.ToggleLevelSelect();
                audioService.Play(AudioCue.Ui);
                return;
            }

            if (input.PreviousLevelPressed)
            {
                LoadLevel((levelIndex - 1 + levels.Count) % levels.Count);
                audioService.Play(AudioCue.Ui);
                return;
            }

            if (input.NextLevelPressed)
            {
                TryAdvanceLevel();
                return;
            }

            if (input.RestartPressed)
            {
                ApplyResolution(session.Restart(), true);
                return;
            }

            var controlHeld = input.ControlHeld;
            var shiftHeld = input.ShiftHeld;

            if (input.UndoPressed && !controlHeld && !shiftHeld)
            {
                ApplyResolution(session.Undo(), true);
                return;
            }

            if (controlHeld && !shiftHeld && input.UndoPressed)
            {
                ApplyResolution(session.Undo(), true);
                return;
            }

            if (controlHeld &&
                (input.RedoPressed ||
                 (shiftHeld && input.UndoPressed)))
            {
                ApplyResolution(session.Redo(), true);
                return;
            }

            if (input.SplitPressed)
            {
                if (!CanAcceptGameplayInput())
                {
                    return;
                }

                ApplyResolution(session.SplitFacingEntity());
                return;
            }

            if (input.PrecisionCutPressed)
            {
                if (!CanAcceptGameplayInput())
                {
                    return;
                }

                ApplyResolution(session.PrecisionCutFacingEntity());
                return;
            }

            if (input.RecombinePressed)
            {
                if (!CanAcceptGameplayInput())
                {
                    return;
                }

                ApplyResolution(session.RecombineAdjacentMatter());
                return;
            }

            if (input.RotateLeftPressed)
            {
                if (!CanAcceptGameplayInput())
                {
                    return;
                }

                ApplyResolution(session.RotateFacingEntity(false));
                return;
            }

            if (input.RotateRightPressed)
            {
                if (!CanAcceptGameplayInput())
                {
                    return;
                }

                ApplyResolution(session.RotateFacingEntity(true));
                return;
            }

            if (input.TryReadDirection(out var direction))
            {
                if (!CanAcceptGameplayInput())
                {
                    return;
                }

                ApplyResolution(session.Move(direction));
            }

            if (CanAcceptGameplayInput() && input.TryReadHeldDirection(out var heldDirection))
            {
                boardView.ShowMovePreview(session.State, heldDirection);
            }
            else
            {
                boardView.ClearMovePreview();
            }
        }

        private void LoadLevelContent()
        {
            levels.Clear();
            if (levelCatalog != null)
            {
                levels.AddRange(levelCatalog.ToDefinitions());
            }

            if (levels.Count == 0)
            {
                levels.AddRange(BuiltInLevels.All);
            }
        }

        private void LoadLevel(int index)
        {
            levelIndex = Mathf.Clamp(index, 0, levels.Count - 1);
            var state = LevelParser.Parse(levels[levelIndex]);
            session = new GridSession(state);
            completeAt = -1f;
            boardView.Rebuild(session.State);
            hud.SetState(levels[levelIndex], levelIndex, levels.Count, session, SelectLevel);
            levelFlow.Change(LevelFlowState.Playing);
        }

        private void SelectLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
            {
                return;
            }

            if (index >= SaveService.Data.highestUnlockedLevel)
            {
                audioService.Play(AudioCue.Blocked);
                return;
            }

            LoadLevel(index);
            levelFlow.Change(LevelFlowState.Playing);
            hud.SetMainMenuVisible(false);
            audioService.Play(AudioCue.Ui);
        }

        private void ContinueFromMenu()
        {
            levelFlow.Change(LevelFlowState.Playing);
            hud.SetMainMenuVisible(false);
            audioService.Play(AudioCue.Ui);
        }

        private void StartNewGame()
        {
            SaveService.Reset();
            LoadLevel(0);
            levelFlow.Change(LevelFlowState.Playing);
            hud.SetMainMenuVisible(false);
            audioService.Play(AudioCue.Ui);
        }

        private void ApplyResolution(ActionResolution resolution, bool forceVisualRefresh = false)
        {
            if (!resolution.Success)
            {
                if (resolution.ActionType == GridActionType.Move ||
                    resolution.ActionType == GridActionType.Split ||
                    resolution.ActionType == GridActionType.PrecisionCut ||
                    resolution.ActionType == GridActionType.Recombine ||
                    resolution.ActionType == GridActionType.Rotate)
                {
                    audioService.Play(AudioCue.Blocked);
                }

                return;
            }

            if (forceVisualRefresh)
            {
                boardView.Rebuild(session.State);
            }
            else
            {
                boardView.SetState(session.State, resolution);
            }

            if (resolution.ActionType == GridActionType.Move)
            {
                playerState.Change(resolution.PushedEntities > 0
                    ? PlayerActionState.Interacting
                    : PlayerActionState.Moving);
            }
            else if (resolution.ActionType == GridActionType.Split ||
                     resolution.ActionType == GridActionType.PrecisionCut ||
                     resolution.ActionType == GridActionType.Recombine ||
                     resolution.ActionType == GridActionType.Rotate)
            {
                playerState.Change(PlayerActionState.Interacting);
            }

            playerStateUntil = Time.unscaledTime + (playerState.CurrentState == PlayerActionState.Interacting ? 0.18f : 0.09f);

            switch (resolution.ActionType)
            {
                case GridActionType.Split:
                    audioService.Play(AudioCue.Split);
                    break;
                case GridActionType.PrecisionCut:
                    audioService.Play(AudioCue.Split);
                    break;
                case GridActionType.Recombine:
                    audioService.Play(AudioCue.Recombine);
                    break;
                case GridActionType.Rotate:
                    audioService.Play(AudioCue.Ui);
                    break;
                case GridActionType.Undo:
                case GridActionType.Redo:
                case GridActionType.Restart:
                    audioService.Play(AudioCue.Ui);
                    break;
                default:
                    if (resolution.UsedPortal)
                    {
                        audioService.Play(AudioCue.Portal);
                    }
                    else if (resolution.PushedEntities > 0)
                    {
                        audioService.Play(AudioCue.Push);
                    }
                    else
                    {
                        audioService.Play(AudioCue.Move);
                    }

                    break;
            }

            if (session.State.IsSolved() && completeAt < 0f)
            {
                completeAt = Time.unscaledTime;
                levelFlow.Change(LevelFlowState.Completed);
                SaveService.RecordCompletion(
                    levels[levelIndex].Id,
                    levelIndex,
                    session.State.MoveCount,
                    session.State.PushCount,
                    levels.Count);
                audioService.Play(AudioCue.Complete);
            }
            else if (!session.State.IsSolved())
            {
                completeAt = -1f;
            }
        }

        private void TryAdvanceLevel()
        {
            var nextIndex = levelIndex + 1;
            var unlocked = SaveService.Data.highestUnlockedLevel;
            if (nextIndex >= levels.Count)
            {
                LoadLevel(0);
                audioService.Play(AudioCue.Ui);
                return;
            }

            if (nextIndex >= unlocked)
            {
                audioService.Play(AudioCue.Blocked);
                return;
            }

            LoadLevel(nextIndex);
            audioService.Play(AudioCue.Ui);
        }

        private bool CanAcceptGameplayInput()
        {
            return levelFlow.CurrentState == LevelFlowState.Playing &&
                   !hud.IsLevelSelectOpen &&
                   !hud.IsMainMenuOpen;
        }

    }
}
