using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public  List<LevelData> levels = new List<LevelData>();
    private Level           currentLevel;

    public Level CurrentLevel => currentLevel;

    private int levelIndex = 0;

    public void LoadLevel()
    {
        if (currentLevel != null)
        {
            currentLevel.DeleteAllData();
        }

        MazeGenerator.Instance.CreateMap();
    }

    public void OnStart()
    {
        GameManager.Instance.ChangeState(GameState.GamePlay);
        SetDataToGenMapAndBot();
        LoadLevel();
    }

    public void OnFinish()
    {
        UIManager.Instance.OpenFinishUI();
        GameManager.Instance.ChangeState(GameState.Win);
    }

    public void OnLose()
    {
        UIManager.Instance.OpenLoseUI();
        GameManager.Instance.ChangeState(GameState.Lose);
    }

    public void NextLevel()
    {
        if (this.levelIndex < levels.Count)
        {
            this.levelIndex++;
        }

        else
        {
            this.levelIndex = 0;
        }
    }

    public void SetLevelData(List<LevelData> levelData)
    {
        levels.Clear();
        levels.AddRange(levelData);
    }

    public void SetDataToGenMapAndBot()
    {
        MazeGenerator.Instance.height                = levels[this.levelIndex].height;
        MazeGenerator.Instance.width                = levels[this.levelIndex].width;
        MazeGenerator.Instance.wallDensity           = levels[this.levelIndex].wallDensity;
        MazeGenerator.Instance.minWallLength         = levels[this.levelIndex].minWallLength;
        MazeGenerator.Instance.maxWallLength         = levels[this.levelIndex].maxWallLength;
        MazeGenerator.Instance.maxWallThickness      = levels[this.levelIndex].maxWallThickness;
        TankSpawner.Instance.numberOfEnemies         = levels[this.levelIndex].botCount;
        TankSpawner.Instance.minDistanceBetweenTanks = levels[this.levelIndex].distanceBetweenBots;
    }
}