using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CompoundBox.Tests
{
    public sealed class GridSessionTests
    {
        [Test]
        public void KnownSolutions_CompleteEveryBuiltInLevel()
        {
            foreach (var level in BuiltInLevels.All)
            {
                var session = new GridSession(LevelParser.Parse(level));
                var result = session.ReplaySolution(level);

                Assert.That(result.Success, Is.True, $"{level.Id}: {result.Message}");
                Assert.That(session.State.IsSolved(), Is.True, $"{level.Id} did not reach a solved state.");
            }
        }

        [Test]
        public void BlockedMove_DoesNotAdvanceCounters()
        {
            var definition = new LevelDefinition(
                "blocked",
                "Blocked",
                "Test fixture",
                new[]
                {
                    "#####",
                    "#@#.#",
                    "#####"
                },
                string.Empty);
            var state = LevelParser.Parse(definition);
            var session = new GridSession(state);

            var result = session.Move(GridDirection.Left);

            Assert.That(result.Success, Is.False);
            Assert.That(session.State.MoveCount, Is.Zero);
            Assert.That(session.State.ActionCount, Is.Zero);
        }

        [Test]
        public void SplitThenRecombine_RestoresACompoundEntity()
        {
            var definition = new LevelDefinition(
                "split-recombine",
                "Split Recombine",
                "Test fixture",
                new[]
                {
                    "######",
                    "#@11.#",
                    "#....#",
                    "######"
                },
                "SC");

            var session = new GridSession(LevelParser.Parse(definition));
            var split = session.SplitFacingEntity();

            Assert.That(split.Success, Is.True, split.Message);
            Assert.That(session.State.Entities.Count(entity => entity.Kind == EntityKind.Matter), Is.EqualTo(2));
            Assert.That(session.State.Entities.Where(entity => entity.Kind == EntityKind.Matter).All(entity => entity.Cells.Count == 1), Is.True);

            var recombine = session.RecombineAdjacentMatter();

            Assert.That(recombine.Success, Is.True, recombine.Message);
            var merged = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(merged.Cells.Count, Is.EqualTo(2));
        }

        [Test]
        public void Portal_TeleportsWholeCompoundToExit()
        {
            var level = BuiltInLevels.All.Single(item => item.Id == "compound-gate");
            var state = LevelParser.Parse(level);
            var exit = state.Portals[new Vector2Int(6, 3)].Exit;
            var session = new GridSession(state);

            var first = session.Move(GridDirection.Right);
            var second = session.Move(GridDirection.Right);

            Assert.That(first.Success, Is.True, first.Message);
            Assert.That(second.Success, Is.True, second.Message);
            Assert.That(second.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(matter.Anchor, Is.EqualTo(exit));

            Assert.That(session.Move(GridDirection.Right).Success, Is.True);
            Assert.That(session.Move(GridDirection.Right).Success, Is.True);
            Assert.That(session.State.IsSolved(), Is.True);
        }

        [Test]
        public void Undo_And_Redo_RestoreExactBoardState()
        {
            var session = new GridSession(LevelParser.Parse(BuiltInLevels.All[0]));
            var initialPlayer = session.State.Player.Anchor;

            Assert.That(session.Move(GridDirection.Right).Success, Is.True);
            Assert.That(session.Move(GridDirection.Up).Success, Is.True);
            var afterMoves = session.State.Player.Anchor;

            Assert.That(session.Undo().Success, Is.True);
            Assert.That(session.State.Player.Anchor, Is.Not.EqualTo(afterMoves));
            Assert.That(session.Redo().Success, Is.True);
            Assert.That(session.State.Player.Anchor, Is.EqualTo(afterMoves));
            Assert.That(session.State.Player.Anchor, Is.Not.EqualTo(initialPlayer));
        }

        [Test]
        public void EveryBuiltInLevel_PassesValidation()
        {
            foreach (var level in BuiltInLevels.All)
            {
                var report = LevelValidation.Validate(level);
                Assert.That(report.IsValid, Is.True, $"{level.Id}: {string.Join(" | ", report.Errors)}");
            }
        }
    }
}
