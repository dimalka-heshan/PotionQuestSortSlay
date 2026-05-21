using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BottleSystem
{
    public static class LevelValidator
    {
        public static bool Validate(LevelData data)
        {
            if (data == null) return false;
            if (data.levelNumber <= 0) return false;
            if (string.IsNullOrEmpty(data.themeId)) return false;
            if (data.bottleCapacity <= 0) return false;
            if (data.bottleLayouts == null || data.bottleLayouts.Count == 0) return false;

            int totalCount = 0;
            foreach (var layout in data.bottleLayouts)
            {
                if (layout.Count > data.bottleCapacity)
                {
                    Debug.LogWarning($"Level {data.levelNumber}: Bottle exceeds capacity.");
                    return false;
                }
                totalCount++;
            }

            if (totalCount != (data.filledBottleCount + data.emptyBottleCount))
            {
                Debug.LogWarning($"Level {data.levelNumber}: Total bottle count mismatch.");
            }

            return true;
        }
    }

    public class LevelLoader : MonoBehaviour
    {
        public LevelData LoadLevel(int levelNumber)
        {
            string fileName = $"level_{levelNumber:D3}";
            string path = Path.Combine(Application.dataPath, "Game/Levels/Json", fileName + ".json");

            if (!File.Exists(path))
            {
                Debug.LogError($"Level file not found: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                LevelData data = JsonUtility.FromJson<LevelData>(json);
                
                if (LevelValidator.Validate(data))
                {
                    Debug.Log($"[LevelLoader] Successfully loaded Level {levelNumber}");
                    return data;
                }
                else
                {
                    Debug.LogError($"[LevelLoader] Level {levelNumber} validation failed.");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LevelLoader] Error parsing JSON for Level {levelNumber}: {e.Message}");
                return null;
            }
        }
    }
}
