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
                "RRRDRUU"),

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
                "RR"),

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
                "RSRRDDRRDRRUURULDDDRUU"),

            new LevelDefinition(
                "dual-alloy",
                "Dual Alloy",
                "A mixed compound moves as one body, but every cell carries its own material.",
                new[]
                {
                    "#########",
                    "#..gg...#",
                    "#.......#",
                    "#.@11...#",
                    "#.......#",
                    "#########"
                },
                new[]
                {
                    "000000000",
                    "000120000",
                    "000000000",
                    "000120000",
                    "000000000",
                    "000000000"
                },
                "DRUU"),

            new LevelDefinition(
                "alloy-split",
                "Alloy Split",
                "The splitter preserves each cell's material and turns one body into separate shipments.",
                new[]
                {
                    "###########",
                    "#..ggg....#",
                    "#.........#",
                    "#.@111....#",
                    "#.........#",
                    "#.........#",
                    "###########"
                },
                new[]
                {
                    "00000000000",
                    "00012300000",
                    "00000000000",
                    "00012300000",
                    "00000000000",
                    "00000000000",
                    "00000000000"
                },
                "SDRUUDDRUUDDRUU"),

            new LevelDefinition(
                "molecule-socket",
                "Molecule Socket",
                "The socket checks the whole molecule, not three independent cells.",
                new[]
                {
                    "#########",
                    "#.......#",
                    "#..GG...#",
                    "#..G....#",
                    "#.......#",
                    "#.......#",
                    "#.@11...#",
                    "#..1....#",
                    "#.......#",
                    "#########"
                },
                new[]
                {
                    "000000000",
                    "000000000",
                    "000120000",
                    "000200000",
                    "000000000",
                    "000000000",
                    "000120000",
                    "000200000",
                    "000000000",
                    "000000000"
                },
                "DDRUUUU"),

            new LevelDefinition(
                "precision-cut",
                "Precision Cut",
                "Cut one connection, keep the remaining bond, and deliver two components.",
                new[]
                {
                    "##########",
                    "#..gGG...#",
                    "#........#",
                    "#.@111...#",
                    "#........#",
                    "#........#",
                    "#........#",
                    "##########"
                },
                new[]
                {
                    "0000000000",
                    "0001110000",
                    "0000000000",
                    "0001110000",
                    "0000000000",
                    "0000000000",
                    "0000000000",
                    "0000000000"
                },
                "VDRUUDDRUU"),

            new LevelDefinition(
                "rotation-vault",
                "Rotation Vault",
                "Orient the compound before moving it into the socket.",
                new[]
                {
                    "##########",
                    "#........#",
                    "#........#",
                    "#..1.....#",
                    "#.@11GG..#",
                    "#....G...#",
                    "#........#",
                    "##########"
                },
                new[]
                {
                    "0000000000",
                    "0000000000",
                    "0000000000",
                    "0001000000",
                    "0001111000",
                    "0000010000",
                    "0000000000",
                    "0000000000"
                },
                "ERR"),

            new LevelDefinition(
                "portable-gate",
                "Portable Gate",
                "Move the exit node before routing the payload through the fixed entry.",
                new[]
                {
                    "###########",
                    "#.........#",
                    "#.........#",
                    "#.......g.#",
                    "#.@1.a..P.#",
                    "#.........#",
                    "#.........#",
                    "###########"
                },
                new[]
                {
                    "00000000000",
                    "00000000000",
                    "00000000000",
                    "00000000100",
                    "00010000000",
                    "00000000000",
                    "00000000000",
                    "00000000000"
                },
                "URDLDRRDRU")
        };

        public static IReadOnlyList<LevelDefinition> All => Levels;
    }
}
