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
    }
}
