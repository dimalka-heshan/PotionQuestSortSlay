using System.Collections.Generic;
using UnityEngine;

namespace BottleSystem
{
    [System.Serializable]
    public class BottleLayoutData
    {
        public string[] colors;
    }

    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int worldId;
        public string themeId;
        public string difficultyTier;
        public int bottleCapacity;
        public int colorCount;
        public int filledBottleCount;
        public int emptyBottleCount;
        public int moveLimit;
        public string enemyId;
        public string enemyName;
        public int enemyHP;
        public string enemyWeaknessColor;
        public bool isBossLevel;
        public int rewardCoins;
        public int rewardGems;
        public BottleLayoutData[] bottles;
    }
}
