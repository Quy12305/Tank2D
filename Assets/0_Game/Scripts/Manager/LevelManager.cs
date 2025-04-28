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
        GameManager.Instance.ChangeState(GameState.Finish);
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

    public void SetDataToGenMapAndBot()
    {
        MazeGenerator.Instance.width            = levels[this.levelIndex - 1].width;
        MazeGenerator.Instance.height           = levels[this.levelIndex - 1].height;
        MazeGenerator.Instance.wallDensity      = levels[this.levelIndex - 1].wallDensity;
        MazeGenerator.Instance.minWallLength    = levels[this.levelIndex - 1].minWallLength;
        MazeGenerator.Instance.maxWallLength    = levels[this.levelIndex - 1].maxWallLength;
        MazeGenerator.Instance.maxWallThickness = levels[this.levelIndex - 1].maxWallThickness;
        TankSpawner.Instance.numberOfEnemies    = levels[this.levelIndex - 1].botCount;
    }
}