using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public enum EntityKind
    {
        Player,
        Matter
    }

    public sealed class GridEntity
    {
        public GridEntity(int id, EntityKind kind, MatterType matter, IEnumerable<Vector2Int> cells)
        {
            Id = id;
            Kind = kind;
            Matter = matter;
            Cells = new List<Vector2Int>(cells);
            SortCells();
        }

        public int Id { get; private set; }
        public EntityKind Kind { get; private set; }
        public MatterType Matter { get; }
        public List<Vector2Int> Cells { get; private set; }

        public Vector2Int Anchor
        {
            get
            {
                var anchor = Cells[0];
                for (var i = 1; i < Cells.Count; i++)
                {
                    var cell = Cells[i];
                    if (cell.y < anchor.y || (cell.y == anchor.y && cell.x < anchor.x))
                    {
                        anchor = cell;
                    }
                }

                return anchor;
            }
        }

        public bool Occupies(Vector2Int cell)
        {
            for (var i = 0; i < Cells.Count; i++)
            {
                if (Cells[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        public GridEntity Clone()
        {
            return new GridEntity(Id, Kind, Matter, Cells);
        }

        public void SetCells(IReadOnlyList<Vector2Int> cells)
        {
            Cells.Clear();
            for (var i = 0; i < cells.Count; i++)
            {
                Cells.Add(cells[i]);
            }

            SortCells();
        }

        private void SortCells()
        {
            Cells.Sort((left, right) =>
            {
                var yComparison = left.y.CompareTo(right.y);
                return yComparison != 0 ? yComparison : left.x.CompareTo(right.x);
            });
        }
    }
}
