using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Serialize] private GameObject mainmenuUI;
    [Serialize] private GameObject finishUI;
    [Serialize] private GameObject loseUI;
    [Serialize] private GameObject settingsUI;
    [Serialize] private GameObject gameplayUI;

    private void Start()
    {
        OpenMainMenuUI();
    }

    public void OpenMainMenuUI()
    {
        mainmenuUI.SetActive(true);
        CloseAllUI();
    }

    public void OpenGamePlayUI()
    {
        gameplayUI.SetActive(false);
        CloseAllUI();
    }

    public void OpenFinishUI()
    {
        mainmenuUI.SetActive(false);
        CloseAllUI();
    }

    public void OpenLoseUI()
    {
        loseUI.SetActive(false);
        CloseAllUI();
    }

    public void OpenSettingUI()
    {
        settingsUI.SetActive(false);
        CloseAllUI();
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

    public void CloseAllUI()
    {
        mainmenuUI.SetActive(false);
        finishUI.SetActive(false);
        loseUI.SetActive(false);
        settingsUI.SetActive(false);
        gameplayUI.SetActive(false);
    }

}