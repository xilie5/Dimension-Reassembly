using System.Collections.Generic;

namespace CompoundBox
{
    public static class BuiltInLevels
    {
        private static readonly List<LevelDefinition> Levels = new List<LevelDefinition>
        {
            new LevelDefinition(
                "first-push",
                "First Push",
                "Learn the grid, then deliver the cyan mass.",
                new[]
                {
                    "#########",
                    "#.......#",
                    "#.@1.x..#",
                    "#..E....#",
                    "#.......#",
                    "#########"
                },
                "RRLD"),

            new LevelDefinition(
                "split-decision",
                "Split Decision",
                "A compound can be separated, but every shard wants a destination.",
                new[]
                {
                    "#########",
                    "#..x...x#",
                    "#.......#",
                    "#.@11...#",
                    "#.......#",
                    "#.......#",
                    "#########"
                },
                "SDRRUULURRRLLLDLDDRUU"),

            new LevelDefinition(
                "anchor-transit",
                "Anchor Transit",
                "A portal preserves the body's anchor instead of its leading edge.",
                new[]
                {
                    "###########",
                    "#.@1.a#..x#",
                    "#.....#...#",
                    "#.....#A..#",
                    "#.....#...#",
                    "###########"
                },
                "RRRRDRUU"),

            new LevelDefinition(
                "compound-gate",
                "Compound Gate",
                "Two cells, one gate, and a socket that accepts the whole shape.",
                new[]
                {
                    "###########",
                    "#.@11.a#..#",
                    "#.....##..#",
                    "#......Axx#",
                    "###########"
                },
                "RRRR"),

            new LevelDefinition(
                "reassembly",
                "Reassembly",
                "Bring matching matter together before the compound socket can accept it.",
                new[]
                {
                    "###########",
                    "#....XX...#",
                    "#.........#",
                    "#.@1...1..#",
                    "#.........#",
                    "#.........#",
                    "###########"
                },
                "RURRRRRDLLCULLLDRDRUU"),

            new LevelDefinition(
                "phase-loom",
                "Phase Loom",
                "Transit a compound, split it under pressure, and feed both outputs.",
                new[]
                {
                    "############",
                    "#.@11.a#x.x#",
                    "#.....#....#",
                    "#.......A..#",
                    "#..........#",
                    "############"
                },
                "RRRRSDRRUUDLDLUU")
        };

        public static IReadOnlyList<LevelDefinition> All => Levels;
    }
}
