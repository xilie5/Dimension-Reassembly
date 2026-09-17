using System.IO;
using UnityEngine;

namespace CompoundBox
{
    public static class SaveService
    {
        private static SaveData data;

        public static SaveData Data
        {
            get
            {
                EnsureLoaded();
                return data;
            }
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, "compound-box-save.json");

        public static void EnsureLoaded()
        {
            if (data != null)
            {
                return;
            }

            if (!File.Exists(SavePath))
            {
                data = new SaveData();
                return;
            }

            try
            {
                data = Deserialize(File.ReadAllText(SavePath));
            }
            catch
            {
                data = new SaveData();
            }
        }

        public static void RecordCompletion(
            string levelId,
            int levelIndex,
            int moveCount,
            int pushCount,
            int levelCount)
        {
            EnsureLoaded();
            data.ApplyCompletion(levelId, levelIndex, moveCount, pushCount, levelCount);
            Save();
        }

        public static void SetAudioMuted(bool muted)
        {
            EnsureLoaded();
            data.audioMuted = muted;
            Save();
        }

        public static void Save()
        {
            EnsureLoaded();
            var directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(SavePath, Serialize(data));
        }

        public static void Reset()
        {
            data = new SaveData();
            Save();
        }

        public static string Serialize(SaveData saveData)
        {
            return JsonUtility.ToJson(saveData, true);
        }

        public static SaveData Deserialize(string json)
        {
            var saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null)
            {
                return new SaveData();
            }

            if (saveData.levels == null)
            {
                saveData.levels = new System.Collections.Generic.List<LevelProgressRecord>();
            }

            saveData.version = SaveData.CurrentVersion;
            return saveData;
        }
    }
}
