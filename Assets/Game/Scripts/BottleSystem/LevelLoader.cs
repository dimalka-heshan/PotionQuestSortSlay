using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BottleSystem
{
    public static class LevelValidator
    {
        public static bool Validate(LevelData data)
        {
            if (data == null)
            {
                Debug.LogError("[LevelValidator] LevelData is null.");
                return false;
            }
            if (data.levelNumber <= 0)
            {
                Debug.LogError("[LevelValidator] Invalid level number.");
                return false;
            }
            if (string.IsNullOrEmpty(data.themeId))
            {
                Debug.LogError("[LevelValidator] themeId is missing.");
                return false;
            }
            if (data.bottleCapacity <= 0)
            {
                Debug.LogError("[LevelValidator] bottleCapacity must be > 0.");
                return false;
            }
            if (data.bottles == null || data.bottles.Length == 0)
            {
                Debug.LogError("[LevelValidator] No bottles defined in level data.");
                return false;
            }

            int totalCount = 0;
            for (int i = 0; i < data.bottles.Length; i++)
            {
                var bottle = data.bottles[i];
                if (bottle == null)
                {
                    Debug.LogError($"[LevelValidator] Bottle at index {i} is null.");
                    return false;
                }

                if (bottle.colors == null)
                {
                    Debug.LogWarning($"[LevelValidator] Bottle at index {i} colors array is null. Treating as empty.");
                    bottle.colors = new string[0];
                }

                if (bottle.colors.Length > data.bottleCapacity)
                {
                    Debug.LogError($"[LevelValidator] Level {data.levelNumber}: Bottle {i} exceeds capacity ({bottle.colors.Length} > {data.bottleCapacity}).");
                    return false;
                }

                if (bottle.colors.Length == 0)
                {
                    // Ensure empty bottles are truly empty (already handled by Length == 0)
                }

                totalCount++;
            }

            if (totalCount != (data.filledBottleCount + data.emptyBottleCount))
            {
                Debug.LogWarning($"[LevelValidator] Level {data.levelNumber}: Total bottle count mismatch ({totalCount} vs {data.filledBottleCount + data.emptyBottleCount}).");
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
                    Debug.Log($"[LevelLoader] Level: {data.levelNumber}, Enemy: {(!string.IsNullOrEmpty(data.enemyName) ? data.enemyName : data.enemyId)}, Moves: {data.moveLimit}, Bottles: {data.bottles.Length}");
                    
                    for (int i = 0; i < data.bottles.Length; i++)
                    {
                        string colorsStr = data.bottles[i].colors != null ? string.Join(", ", data.bottles[i].colors) : "EMPTY";
                        Debug.Log($"[LevelLoader] Bottle {i}: [{colorsStr}]");
                    }

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
                Debug.LogError($"[LevelLoader] Error parsing JSON for Level {levelNumber}: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }
    }
}
