using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CompoundBox.Tests
{
    public sealed class TransformMechanicsTests
    {
        [Test]
        public void PrecisionCut_SeparatesOneConnectionIntoTwoComponents()
        {
            var state = CreateOpenState();
            var player = new GridEntity(1, EntityKind.Player, MatterType.None, new[] { new Vector2Int(2, 3) });
            var matter = new GridEntity(
                2,
                EntityKind.Matter,
                MatterType.Cyan,
                new[] { new Vector2Int(3, 3), new Vector2Int(4, 3), new Vector2Int(5, 3) });
            state.Entities.Add(player);
            state.Entities.Add(matter);
            state.PlayerId = player.Id;
            state.NextEntityId = 3;
            state.Facing = GridDirection.Right;

            var result = GridBoardSimulation.PrecisionCutFacingEntity(state);

            Assert.That(result.Success, Is.True, result.Message);
            var components = state.Entities.Where(entity => entity.Kind == EntityKind.Matter).ToList();
            Assert.That(components.Count, Is.EqualTo(2));
            Assert.That(components.Select(entity => entity.Cells.Count).OrderBy(value => value), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Rotate_TransformsCellsAndConnectionsWithoutChangingMatter()
        {
            var state = CreateOpenState();
            var player = new GridEntity(1, EntityKind.Player, MatterType.None, new[] { new Vector2Int(2, 3) });
            var matter = new GridEntity(
                2,
                EntityKind.Matter,
                MatterType.Cyan,
                new[] { new Vector2Int(3, 3), new Vector2Int(3, 4), new Vector2Int(4, 3) });
            state.Entities.Add(player);
            state.Entities.Add(matter);
            state.PlayerId = player.Id;
            state.NextEntityId = 3;
            state.Facing = GridDirection.Right;

            var result = GridBoardSimulation.RotateFacingEntity(state, true);

            Assert.That(result.Success, Is.True, result.Message);
            var rotated = state.Entities.Single(entity => entity.Kind == EntityKind.Matter);
            Assert.That(rotated.Occupies(new Vector2Int(3, 3)), Is.True);
            Assert.That(rotated.Occupies(new Vector2Int(4, 3)), Is.True);
            Assert.That(rotated.Occupies(new Vector2Int(3, 2)), Is.True);
            Assert.That(rotated.CellStates.All(cell => cell.Matter == MatterType.Cyan), Is.True);
            Assert.That(rotated.Connections.Count, Is.EqualTo(2));
        }

        [Test]
        public void Rotate_FailsWhenTargetSpaceIsOccupied()
        {
            var state = CreateOpenState();
            var player = new GridEntity(1, EntityKind.Player, MatterType.None, new[] { new Vector2Int(2, 3) });
            var matter = new GridEntity(
                2,
                EntityKind.Matter,
                MatterType.Cyan,
                new[] { new Vector2Int(3, 3), new Vector2Int(3, 4), new Vector2Int(4, 3) });
            var blocker = new GridEntity(3, EntityKind.Matter, MatterType.Amber, new[] { new Vector2Int(3, 2) });
            state.Entities.Add(player);
            state.Entities.Add(matter);
            state.Entities.Add(blocker);
            state.PlayerId = player.Id;
            state.NextEntityId = 4;
            state.Facing = GridDirection.Right;

            var result = GridBoardSimulation.RotateFacingEntity(state, true);

            Assert.That(result.Success, Is.False);
            Assert.That(state.GetEntity(2).Occupies(new Vector2Int(3, 4)), Is.True);
        }

        private static GridBoardState CreateOpenState()
        {
            var state = new GridBoardState(8, 8);
            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    state.SetTile(new Vector2Int(x, y), TileKind.Floor);
                }
            }

            return state;
        }
    }
}
