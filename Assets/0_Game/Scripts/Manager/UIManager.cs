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
        CloseAllUI();
        mainmenuUI.SetActive(true);
    }

    public void OpenGamePlayUI()
    {
        CloseAllUI();
        gameplayUI.SetActive(true);
    }

    public void OpenFinishUI()
    {
        CloseAllUI();
        mainmenuUI.SetActive(true);
    }

    public void OpenLoseUI()
    {
        CloseAllUI();
        loseUI.SetActive(true);
    }

    public void OpenSettingUI()
    {
        CloseAllUI();
        settingsUI.SetActive(true);
    }

    public void PlayButton()
    {
        mainmenuUI.SetActive(false);
        LevelManager.Instance.OnStart();
    }

    public void ReplayButton()
    {
        finishUI.SetActive(false);
        LevelManager.Instance.OnStart();
    }

    public void NextLevelButton()
    {
        finishUI.SetActive(false);
        LevelManager.Instance.NextLevel();
        LevelManager.Instance.OnStart();
    }

    public void SettingButton()
    {
        settingsUI.SetActive(true);
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