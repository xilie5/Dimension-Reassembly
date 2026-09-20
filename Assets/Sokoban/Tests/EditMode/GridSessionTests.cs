using System.Collections.Generic;
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
            Assert.That(matter.Anchor, Is.EqualTo(exit + Vector2Int.right));
            Assert.That(matter.Occupies(exit), Is.False);
            Assert.That(session.State.IsSolved(), Is.True);
        }

        [Test]
        public void PortablePortal_ReceivesTeleportedMatterBeyondExitNode()
        {
            var level = BuiltInLevels.All.Single(item => item.Id == "portable-gate");
            var session = new GridSession(LevelParser.Parse(level));

            var result = session.ReplaySolution(level);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(session.State.IsSolved(), Is.True);
            Assert.That(session.State.ValidateInvariants(), Is.Empty);

            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            var portalNode = session.State.Entities.Single(entity => entity.Kind == EntityKind.PortalNode);

            Assert.That(matter.Occupies(portalNode.Anchor), Is.False);
            Assert.That(portalNode.Cells.All(cell => !matter.Occupies(cell)), Is.True);
        }

        [Test]
        public void Portal_IsBidirectional()
        {
            var definition = new LevelDefinition(
                "bidirectional-portal",
                "Bidirectional Portal",
                "Test fixture",
                new[]
                {
                    "############",
                    "#..a.......#",
                    "#..A...1@..#",
                    "############"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);

            ActionResolution result = null;
            for (var i = 0; i < 4; i++)
            {
                result = session.Move(GridDirection.Left);
                Assert.That(result.Success, Is.True, result.Message);
            }

            Assert.That(result.UsedPortal, Is.True);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(2, 2)));
            Assert.That(matter.Occupies(new Vector2Int(3, 1)), Is.False);
        }

        [Test]
        public void Portal_TeleportsUpBeyondDestination()
        {
            var definition = new LevelDefinition(
                "vertical-up-portal",
                "Vertical Up Portal",
                "Test fixture",
                new[]
                {
                    "#######",
                    "#.....#",
                    "#..A..#",
                    "#.....#",
                    "#.....#",
                    "#..a..#",
                    "#..1..#",
                    "#..@..#",
                    "#######"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Up);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(3, 7)));
            Assert.That(matter.Occupies(new Vector2Int(3, 6)), Is.False);
        }

        [Test]
        public void Portal_TeleportsDownBeyondDestination()
        {
            var definition = new LevelDefinition(
                "vertical-down-portal",
                "Vertical Down Portal",
                "Test fixture",
                new[]
                {
                    "#######",
                    "#..@..#",
                    "#..1..#",
                    "#..A..#",
                    "#.....#",
                    "#.....#",
                    "#..a..#",
                    "#.....#",
                    "#######"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Down);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(3, 1)));
            Assert.That(matter.Occupies(new Vector2Int(3, 2)), Is.False);
        }

        [Test]
        public void Portal_TeleportsUpWhenDestinationIsBehindEntry()
        {
            var definition = new LevelDefinition(
                "vertical-up-behind",
                "Vertical Up Behind",
                "Test fixture",
                new[]
                {
                    "#######",
                    "#.....#",
                    "#..a..#",
                    "#..1..#",
                    "#..@..#",
                    "#.....#",
                    "#..A..#",
                    "#.....#",
                    "#######"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Up);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(3, 3)));
            Assert.That(matter.Occupies(new Vector2Int(3, 2)), Is.False);
        }

        [Test]
        public void Portal_TeleportsDownWhenDestinationIsBehindEntry()
        {
            var definition = new LevelDefinition(
                "vertical-down-behind",
                "Vertical Down Behind",
                "Test fixture",
                new[]
                {
                    "#######",
                    "#.....#",
                    "#..A..#",
                    "#.....#",
                    "#..@..#",
                    "#..1..#",
                    "#..a..#",
                    "#.....#",
                    "#######"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Down);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(3, 5)));
            Assert.That(matter.Occupies(new Vector2Int(3, 6)), Is.False);
        }

        [Test]
        public void IrregularCompound_TeleportsUpWithoutCoveringExit()
        {
            var definition = new LevelDefinition(
                "vertical-up-compound",
                "Vertical Up Compound",
                "Test fixture",
                new[]
                {
                    "############",
                    "#..........#",
                    "#..........#",
                    "#......A...#",
                    "#..........#",
                    "#..........#",
                    "#..a1......#",
                    "#..11......#",
                    "#..@.......#",
                    "############"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Up);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            var expected = new HashSet<Vector2Int>
            {
                new Vector2Int(7, 7),
                new Vector2Int(8, 7),
                new Vector2Int(8, 8)
            };
            Assert.That(matter.Cells, Is.EquivalentTo(expected));
            Assert.That(matter.Occupies(new Vector2Int(7, 6)), Is.False);
        }

        [Test]
        public void IrregularCompound_TeleportsDownWithoutCoveringExit()
        {
            var definition = new LevelDefinition(
                "vertical-down-compound",
                "Vertical Down Compound",
                "Test fixture",
                new[]
                {
                    "############",
                    "#..@.......#",
                    "#..11......#",
                    "#..a1......#",
                    "#..........#",
                    "#..........#",
                    "#......A...#",
                    "#..........#",
                    "#..........#",
                    "############"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Down);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            var expected = new HashSet<Vector2Int>
            {
                new Vector2Int(7, 2),
                new Vector2Int(8, 2),
                new Vector2Int(8, 1)
            };
            Assert.That(matter.Cells, Is.EquivalentTo(expected));
            Assert.That(matter.Occupies(new Vector2Int(7, 3)), Is.False);
        }

        [Test]
        public void PortablePortal_TeleportsUpBeyondMovableExit()
        {
            var definition = new LevelDefinition(
                "vertical-up-movable-exit",
                "Vertical Up Movable Exit",
                "Test fixture",
                new[]
                {
                    "#########",
                    "#.......#",
                    "#...P...#",
                    "#.......#",
                    "#.......#",
                    "#..a....#",
                    "#..1....#",
                    "#..@....#",
                    "#########"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Up);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            var portalNode = session.State.Entities.Single(entity => entity.Kind == EntityKind.PortalNode);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(4, 7)));
            Assert.That(matter.Occupies(portalNode.Anchor), Is.False);
        }

        [Test]
        public void PortablePortal_TeleportsDownBeyondMovableExit()
        {
            var definition = new LevelDefinition(
                "vertical-down-movable-exit",
                "Vertical Down Movable Exit",
                "Test fixture",
                new[]
                {
                    "#########",
                    "#..@....#",
                    "#..1....#",
                    "#..P....#",
                    "#.......#",
                    "#.......#",
                    "#..a....#",
                    "#.......#",
                    "#########"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Down);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            var portalNode = session.State.Entities.Single(entity => entity.Kind == EntityKind.PortalNode);
            Assert.That(matter.Anchor, Is.EqualTo(new Vector2Int(3, 1)));
            Assert.That(matter.Occupies(portalNode.Anchor), Is.False);
        }

        [Test]
        public void IrregularCompound_ClearsPortalAndPreservesShape()
        {
            var definition = new LevelDefinition(
                "irregular-portal",
                "Irregular Portal",
                "Test fixture",
                new[]
                {
                    "############",
                    "#@11a.....#",
                    "#..1......#",
                    "#....A....#",
                    "#..........#",
                    "############"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));

            var result = session.Move(GridDirection.Right);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.UsedPortal, Is.True);
            var matter = session.State.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            var expected = new HashSet<Vector2Int>
            {
                new Vector2Int(6, 2),
                new Vector2Int(7, 2),
                new Vector2Int(7, 1)
            };
            Assert.That(matter.Cells, Is.EquivalentTo(expected));
            Assert.That(matter.Occupies(new Vector2Int(5, 2)), Is.False);
        }

        [Test]
        public void PortablePortal_CannotBePushedOntoAnotherPortal()
        {
            var definition = new LevelDefinition(
                "portal-overlap",
                "Portal Overlap",
                "Test fixture",
                new[]
                {
                    "#########",
                    "#@P.a.A.#",
                    "#.p.....#",
                    "#########"
                },
                string.Empty);
            var session = new GridSession(LevelParser.Parse(definition));
            var portalNode = session.State.Entities.Single(
                entity => entity.Kind == EntityKind.PortalNode &&
                          entity.Anchor == new Vector2Int(2, 2));

            Assert.That(session.Move(GridDirection.Right).Success, Is.True);
            Assert.That(portalNode.Anchor, Is.EqualTo(new Vector2Int(3, 2)));

            var blocked = session.Move(GridDirection.Right);

            Assert.That(blocked.Success, Is.False);
            Assert.That(blocked.Message, Does.Contain("portal"));
            Assert.That(portalNode.Anchor, Is.EqualTo(new Vector2Int(3, 2)));
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
