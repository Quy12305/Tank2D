using System.Collections.Generic;
using UnityEngine;

public class GameDataLevel : Singleton<GameDataLevel>
{
    public Mode currentMode = Mode.Easy;

    public Dictionary<Mode, int> LevelIndexByMode = new Dictionary<Mode, int>
    {
        { Mode.Easy, 0 },
        { Mode.Nomarl, 0 },
        { Mode.Hard, 0 }
    };

    [System.Serializable]
    public class ModeLevelIndex
    {
        public Mode mode;
        public int  levelIndex;
    }

    [System.Serializable]
    public class LevelData
    {
        public Mode             currentMode;
        public ModeLevelIndex[] levelIndexes;
    }

    public LevelData GetData()
    {
        var levelIndexes = new ModeLevelIndex[LevelIndexByMode.Count];
        int i            = 0;
        foreach (var pair in LevelIndexByMode)
        {
            levelIndexes[i++] = new ModeLevelIndex
            {
                mode       = pair.Key,
                levelIndex = pair.Value
            };
        }

        return new LevelData
        {
            currentMode  = this.currentMode,
            levelIndexes = levelIndexes
        };
    }

    public void LoadFromData(LevelData data)
    {
        this.currentMode = data.currentMode;
        LevelIndexByMode.Clear();
        foreach (var item in data.levelIndexes)
        {
            LevelIndexByMode[item.mode] = item.levelIndex;
        }
    }
}