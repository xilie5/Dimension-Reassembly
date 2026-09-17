using System.Collections.Generic;

namespace CompoundBox
{
    public sealed class LevelAuditReport
    {
        public string LevelId;
        public string ChapterName;
        public int Width;
        public int Height;
        public int MatterEntityCount;
        public int PortalNodeCount;
        public int GoalCount;
        public int SolutionActionCount;
        public int MoveCount;
        public int PushCount;
        public bool UsesPortal;
        public bool UsesSplit;
        public bool UsesRecombine;
        public bool UsesPrecisionCut;
        public bool UsesRotation;
        public bool HasMixedMatter;
        public bool IsSolvable;
        public string PrimaryMechanic;
        public readonly List<string> Errors = new List<string>();
    }

    public static class LevelAudit
    {
        public static LevelAuditReport Analyze(LevelDefinition definition, int levelIndex = -1)
        {
            var report = new LevelAuditReport
            {
                LevelId = definition.Id,
                ChapterName = levelIndex >= 0 ? ChapterUtility.GetChapterName(levelIndex) : "Unassigned"
            };

            try
            {
                var state = LevelParser.Parse(definition);
                report.Width = state.Width;
                report.Height = state.Height;
                report.GoalCount = state.Goals.Count;

                for (var i = 0; i < state.Entities.Count; i++)
                {
                    var entity = state.Entities[i];
                    if (entity.Kind == EntityKind.Matter)
                    {
                        report.MatterEntityCount++;
                        if (!entity.IsUniformMatter(out _))
                        {
                            report.HasMixedMatter = true;
                        }
                    }
                    else if (entity.Kind == EntityKind.PortalNode)
                    {
                        report.PortalNodeCount++;
                    }
                }

                var session = new GridSession(state);
                var solution = definition.KnownSolution?.Trim().ToUpperInvariant();
                if (!string.IsNullOrWhiteSpace(solution))
                {
                    for (var i = 0; i < solution.Length; i++)
                    {
                        var code = solution[i];
                        if (char.IsWhiteSpace(code) || code == '-')
                        {
                            continue;
                        }

                        report.SolutionActionCount++;
                        var action = ReplayCode(session, code);
                        if (!action.Success)
                        {
                            report.Errors.Add(
                                $"Solution failed at '{code}' ({i + 1}/{solution.Length}): {action.Message}");
                            break;
                        }

                        report.UsesPortal |= action.UsedPortal;
                        report.UsesSplit |= action.ActionType == GridActionType.Split;
                        report.UsesPrecisionCut |= action.ActionType == GridActionType.PrecisionCut;
                        report.UsesRecombine |= action.ActionType == GridActionType.Recombine;
                        report.UsesRotation |= action.ActionType == GridActionType.Rotate;
                    }

                    report.IsSolvable = report.Errors.Count == 0 && session.State.IsSolved();
                    report.MoveCount = session.State.MoveCount;
                    report.PushCount = session.State.PushCount;
                }

                report.PrimaryMechanic = DeterminePrimaryMechanic(report);
            }
            catch (System.Exception exception)
            {
                report.Errors.Add(exception.Message);
            }

            return report;
        }

        private static ActionResolution ReplayCode(GridSession session, char code)
        {
            if (GridDirectionUtility.TryParseSolutionCode(code, out var direction))
            {
                return session.Move(direction);
            }

            switch (code)
            {
                case 'S':
                    return session.SplitFacingEntity();
                case 'V':
                    return session.PrecisionCutFacingEntity();
                case 'C':
                    return session.RecombineAdjacentMatter();
                case 'Q':
                    return session.RotateFacingEntity(false);
                case 'E':
                    return session.RotateFacingEntity(true);
                default:
                    return new ActionResolution(false, GridActionType.Move, $"Unsupported code '{code}'.");
            }
        }

        private static string DeterminePrimaryMechanic(LevelAuditReport report)
        {
            if (report.UsesPrecisionCut)
            {
                return "Precision Cut";
            }

            if (report.UsesRotation)
            {
                return "Rotation";
            }

            if (report.PortalNodeCount > 0)
            {
                return "Portable Portal";
            }

            if (report.HasMixedMatter)
            {
                return "Multi-material";
            }

            if (report.UsesPortal)
            {
                return "Portal";
            }

            if (report.UsesSplit)
            {
                return "Split";
            }

            if (report.UsesRecombine)
            {
                return "Recombine";
            }

            return "Sokoban";
        }
    }

    public static class ChapterUtility
    {
        public static string GetChapterName(int levelIndex)
        {
            if (levelIndex < 3)
            {
                return "Assembly";
            }

            if (levelIndex < 6)
            {
                return "Transit";
            }

            if (levelIndex < 9)
            {
                return "Alloy";
            }

            return "Structure";
        }
    }
}
