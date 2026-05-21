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
            if (data.bottles == null)
            {
                Debug.LogError("[LevelValidator] bottles array is null.");
                return false;
            }
            if (data.bottles.Length == 0)
            {
                Debug.LogError("[LevelValidator] bottles array is empty.");
                return false;
            }
            if (data.levelNumber <= 0)
            {
                Debug.LogError("[LevelValidator] Invalid level number.");
                return false;
            }
            if (data.bottleCapacity <= 0)
            {
                Debug.LogError("[LevelValidator] bottleCapacity must be > 0.");
                return false;
            }

            int totalCount = 0;
            for (int i = 0; i < data.bottles.Length; i++)
            {
                var bottle = data.bottles[i];
                // Every BottleLayoutData entry can be null only if treated as empty.
                if (bottle == null)
                {
                    totalCount++;
                    continue;
                }

                // colors can be null only if treated as empty.
                if (bottle.colors == null)
                {
                    totalCount++;
                    continue;
                }

                if (bottle.colors.Length > data.bottleCapacity)
                {
                    Debug.LogError($"[LevelValidator] Level {data.levelNumber}: Bottle {i} exceeds capacity ({bottle.colors.Length} > {data.bottleCapacity}).");
                    return false;
                }

                totalCount++;
            }

            // filledBottleCount + emptyBottleCount should match data.bottles.Length.
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
                    string enemyLabel = !string.IsNullOrEmpty(data.enemyName) ? data.enemyName : data.enemyId;
                    Debug.Log($"[LevelLoader] Successfully loaded Level {data.levelNumber}");
                    Debug.Log($"[LevelLoader] Enemy: {enemyLabel}, Bottles: {data.bottles.Length}");
                    
                    for (int i = 0; i < data.bottles.Length; i++)
                    {
                        string colorsStr = "EMPTY";
                        if (data.bottles[i] != null && data.bottles[i].colors != null && data.bottles[i].colors.Length > 0)
                        {
                            colorsStr = string.Join(", ", data.bottles[i].colors);
                        }
                        Debug.Log($"[LevelLoader] Bottle {i} (Bottom-to-Top): [{colorsStr}]");
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
