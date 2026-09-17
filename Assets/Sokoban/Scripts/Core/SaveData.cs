using System;
using System.Collections.Generic;

namespace CompoundBox
{
    [Serializable]
    public sealed class LevelProgressRecord
    {
        public string levelId;
        public bool completed;
        public int bestMoves;
        public int bestPushes;
    }

    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public int highestUnlockedLevel = 1;
        public List<LevelProgressRecord> levels = new List<LevelProgressRecord>();
        public bool audioMuted;

        public LevelProgressRecord GetRecord(string levelId)
        {
            for (var i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelId == levelId)
                {
                    return levels[i];
                }
            }

            var record = new LevelProgressRecord { levelId = levelId };
            levels.Add(record);
            return record;
        }

        public bool TryGetRecord(string levelId, out LevelProgressRecord record)
        {
            for (var i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelId == levelId)
                {
                    record = levels[i];
                    return true;
                }
            }

            record = null;
            return false;
        }

        public void ApplyCompletion(
            string levelId,
            int levelIndex,
            int moveCount,
            int pushCount,
            int levelCount)
        {
            var record = GetRecord(levelId);
            record.completed = true;
            record.bestMoves = record.bestMoves <= 0 ? moveCount : System.Math.Min(record.bestMoves, moveCount);
            record.bestPushes = record.bestPushes <= 0 ? pushCount : System.Math.Min(record.bestPushes, pushCount);
            highestUnlockedLevel = System.Math.Max(highestUnlockedLevel, levelIndex + 2);
            highestUnlockedLevel = System.Math.Max(1, System.Math.Min(highestUnlockedLevel, levelCount));
        }
    }
}
