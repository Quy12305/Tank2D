using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject mainmenuUI;
    public GameObject finishUI;

    public void OpenMainMenuUI()
    {
        mainmenuUI.SetActive(true);
        finishUI.SetActive(false);
    }

    public void OpenFinishUI()
    {
        mainmenuUI.SetActive(false);
        finishUI.SetActive(true);
    }

    public void PlayButton()
    {
        mainmenuUI.SetActive(false);
        LevelManager.Instance.OnStart();
    }

    public void ReplayButton()
    {
        finishUI.SetActive(false);
        //LevelManager.Instance.LoadLevel();
        LevelManager.Instance.OnStart();
    }

    public void NextLevelButton()
    {
        LevelManager.Instance.NextLevel();
        finishUI.SetActive(false);
        LevelManager.Instance.OnStart();
    }

}
