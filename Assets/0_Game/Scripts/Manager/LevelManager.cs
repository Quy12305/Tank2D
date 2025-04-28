using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public List<Level> levels = new List<Level>();
    Level currentLevel;

    public Level CurrentLevel => currentLevel;

    int level = 1;

    public void LoadLevel()
    {
        LoadLevel(level);
        OnInit();
    }
    public void LoadLevel(int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
        }
        currentLevel = Instantiate(levels[index - 1]);
    }

    public void OnInit()
    {

    }

    public void OnStart()
    {
        GameManager.Instance.ChangeState(GameState.GamePlay);
        LoadLevel();
    }

    public void OnFinish()
    {
        UIManager.Instance.OpenFinishUI();
        GameManager.Instance.ChangeState(GameState.Finish);
    }

    public void NextLevel()
    {
        if (level < 2)
        {
            level++;
        }

        else
        {
            level = 1;
        }
    }
}