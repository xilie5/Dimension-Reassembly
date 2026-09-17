using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CompoundBox.Tests
{
    public sealed class ArchitectureTests
    {
        [Test]
        public void CommandHistory_RecordsCommandsAndUsesDualStacks()
        {
            var session = new GridSession(LevelParser.Parse(BuiltInLevels.All[0]));

            Assert.That(session.Move(GridDirection.Right).Success, Is.True);
            Assert.That(session.UndoDepth, Is.EqualTo(1));
            Assert.That(session.UndoHistory[0], Is.EqualTo("Move R"));

            Assert.That(session.Undo().Success, Is.True);
            Assert.That(session.UndoDepth, Is.Zero);
            Assert.That(session.RedoDepth, Is.EqualTo(1));

            Assert.That(session.Redo().Success, Is.True);
            Assert.That(session.UndoDepth, Is.EqualTo(1));
            Assert.That(session.RedoDepth, Is.Zero);
        }

        [Test]
        public void SaveData_RoundTripsThroughJson()
        {
            var data = new SaveData
            {
                highestUnlockedLevel = 4,
                audioMuted = true
            };
            var record = data.GetRecord("reassembly");
            record.completed = true;
            record.bestMoves = 22;
            record.bestPushes = 7;

            var json = SaveService.Serialize(data);
            var restored = SaveService.Deserialize(json);

            Assert.That(restored.highestUnlockedLevel, Is.EqualTo(4));
            Assert.That(restored.audioMuted, Is.True);
            Assert.That(restored.TryGetRecord("reassembly", out var restoredRecord), Is.True);
            Assert.That(restoredRecord.bestMoves, Is.EqualTo(22));
            Assert.That(restoredRecord.completed, Is.True);
        }

        [Test]
        public void StateMachine_TransitionsAndReportsPreviousState()
        {
            var machine = new StateMachine<PlayerActionState>(PlayerActionState.Idle);
            var previous = PlayerActionState.Idle;
            var next = PlayerActionState.Idle;
            machine.StateChanged += (from, to) =>
            {
                previous = from;
                next = to;
            };

            Assert.That(machine.Change(PlayerActionState.Moving), Is.True);
            Assert.That(previous, Is.EqualTo(PlayerActionState.Idle));
            Assert.That(next, Is.EqualTo(PlayerActionState.Moving));
            Assert.That(machine.Change(PlayerActionState.Moving), Is.False);
        }

        [Test]
        public void LevelCatalogAsset_ConvertsConfiguredDefinitions()
        {
            var level = ScriptableObject.CreateInstance<LevelDefinitionAsset>();
            level.Configure(
                "catalog-test",
                "Catalog Test",
                "Fixture",
                "####\n#@1#\n#x.#\n####",
                "R");
            var catalog = ScriptableObject.CreateInstance<LevelCatalogAsset>();
            catalog.SetLevels(new System.Collections.Generic.List<LevelDefinitionAsset> { level });

            var definitions = catalog.ToDefinitions();

            Assert.That(definitions.Count, Is.EqualTo(1));
            Assert.That(definitions[0].Id, Is.EqualTo("catalog-test"));
            Assert.That(definitions[0].Rows.Length, Is.EqualTo(4));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(level);
        }

        [Test]
        public void ObjectPool_ReusesReleasedComponent()
        {
            var pool = new ObjectPool<Transform>(
                () => new GameObject("Pooled Test").transform,
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false));
            pool.Prewarm(2);

            Assert.That(pool.AvailableCount, Is.EqualTo(2));
            var first = pool.Get();
            Assert.That(first.gameObject.activeSelf, Is.True);
            Assert.That(pool.AvailableCount, Is.EqualTo(1));

            pool.Release(first);
            Assert.That(pool.AvailableCount, Is.EqualTo(2));
            Assert.That(pool.Get(), Is.SameAs(first));

            Object.DestroyImmediate(first.gameObject);
        }

        [Test]
        public void AdvancedLayout_BuildsOneMultiMaterialEntity()
        {
            var definition = new LevelDefinition(
                "multi-material",
                "Multi Material",
                "Fixture",
                new[]
                {
                    "#######",
                    "#@11..#",
                    "#..g..#",
                    "#######"
                },
                new[]
                {
                    "0000000",
                    "0012000",
                    "0001000",
                    "0000000"
                },
                string.Empty);

            var state = LevelParser.Parse(definition);
            var entity = state.Entities.Single(item => item.Kind == EntityKind.Matter);

            Assert.That(entity.CellStates.Count, Is.EqualTo(2));
            Assert.That(entity.GetMatterAt(new Vector2Int(2, 2)), Is.EqualTo(MatterType.Cyan));
            Assert.That(entity.GetMatterAt(new Vector2Int(3, 2)), Is.EqualTo(MatterType.Amber));
            Assert.That(entity.HasConnection(new Vector2Int(2, 2), new Vector2Int(3, 2)), Is.True);
        }

        [Test]
        public void Movement_PreservesPerCellMaterialAndConnections()
        {
            var definition = new LevelDefinition(
                "multi-material-move",
                "Multi Material Move",
                "Fixture",
                new[]
                {
                    "#######",
                    "#@11..#",
                    "#.....#",
                    "#######"
                },
                new[]
                {
                    "0000000",
                    "0012000",
                    "0000000",
                    "0000000"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));
            var before = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter).Clone();

            Assert.That(session.Move(GridDirection.Right).Success, Is.True);
            var after = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);

            Assert.That(after.GetMatterAt(new Vector2Int(3, 2)), Is.EqualTo(MatterType.Cyan));
            Assert.That(after.GetMatterAt(new Vector2Int(4, 2)), Is.EqualTo(MatterType.Amber));
            Assert.That(after.HasConnection(new Vector2Int(3, 2), new Vector2Int(4, 2)), Is.True);

            Assert.That(session.Undo().Success, Is.True);
            var restored = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(restored.GetMatterAt(new Vector2Int(2, 2)), Is.EqualTo(before.GetMatterAt(new Vector2Int(2, 2))));
            Assert.That(restored.GetMatterAt(new Vector2Int(3, 2)), Is.EqualTo(before.GetMatterAt(new Vector2Int(3, 2))));
            Assert.That(restored.HasConnection(new Vector2Int(2, 2), new Vector2Int(3, 2)), Is.True);
        }

        [Test]
        public void GenericGoal_ReadsMaterialFromMaterialLayer()
        {
            var definition = new LevelDefinition(
                "generic-goal",
                "Generic Goal",
                "Fixture",
                new[]
                {
                    "#######",
                    "#@11..#",
                    "#..gG.#",
                    "#######"
                },
                new[]
                {
                    "0000000",
                    "0012000",
                    "00012 0",
                    "0000000"
                },
                string.Empty);

            var state = LevelParser.Parse(definition);

            Assert.That(state.Goals.Count, Is.EqualTo(2));
            Assert.That(state.Goals[0].Matter, Is.EqualTo(MatterType.Cyan));
            Assert.That(state.Goals[0].RequiresCompound, Is.False);
            Assert.That(state.Goals[1].Matter, Is.EqualTo(MatterType.Amber));
            Assert.That(state.Goals[1].RequiresCompound, Is.True);
        }

        [Test]
        public void MovePreview_ShowsPlayerAndPushedEntityTargets()
        {
            var state = LevelParser.Parse(BuiltInLevels.All[0]);

            var preview = GridBoardSimulation.PreviewMove(state, GridDirection.Right);

            Assert.That(preview.IsValid, Is.True, preview.FailureReason);
            Assert.That(preview.Entities.Count, Is.EqualTo(2));
            var playerPreview = preview.Entities.Single(item => item.Kind == EntityKind.Player);
            Assert.That(playerPreview.TargetCells[0].Position, Is.EqualTo(new Vector2Int(3, 3)));
        }

        [Test]
        public void MovePreview_UsesPortalTransitWithoutMutatingState()
        {
            var level = BuiltInLevels.All.Single(item => item.Id == "compound-gate");
            var state = LevelParser.Parse(level);
            var originalCount = state.ActionCount;

            var preview = GridBoardSimulation.PreviewMove(state, GridDirection.Right);

            Assert.That(preview.IsValid, Is.True, preview.FailureReason);
            Assert.That(state.ActionCount, Is.EqualTo(originalCount));
            Assert.That(state.Entities.Single(entity => entity.Kind == EntityKind.Matter).Anchor, Is.EqualTo(new Vector2Int(3, 3)));
        }

        [Test]
        public void LevelAudit_ReportsAllTwelveLevelsAsSolvable()
        {
            Assert.That(BuiltInLevels.All.Count, Is.EqualTo(12));

            for (var index = 0; index < BuiltInLevels.All.Count; index++)
            {
                var audit = LevelAudit.Analyze(BuiltInLevels.All[index], index);

                Assert.That(audit.Errors, Is.Empty, BuiltInLevels.All[index].Id);
                Assert.That(audit.IsSolvable, Is.True, BuiltInLevels.All[index].Id);
                Assert.That(audit.SolutionActionCount, Is.GreaterThan(0));
                Assert.That(string.IsNullOrWhiteSpace(audit.PrimaryMechanic), Is.False);
            }
        }
    }
}
