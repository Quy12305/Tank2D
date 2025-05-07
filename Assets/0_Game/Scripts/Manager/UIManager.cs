using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject      mainmenuUI;
    [SerializeField] private GameObject      winUI;
    [SerializeField] private GameObject      loseUI;
    [SerializeField] private GameObject      settingsUI;
    [SerializeField] private GameObject      gameplayUI;
    [SerializeField] private GameObject      modeUI;
    [SerializeField] private TMP_Text        textBotInMap;
    [SerializeField] private GameObject      ButtonSettingWhilePlay;
    [SerializeField] private GameObject      ButtonSettingWhileMenu;

    private void Start()
    {
        //this.OpenMainMenuUI();
    }

    public void OpenMainMenuUI()
    {
        this.CloseAllUI();
        this.mainmenuUI.SetActive(true);
    }

    public void OpenGamePlayUI()
    {
        this.CloseAllUI();
        this.gameplayUI.SetActive(true);
        Time.timeScale = 1f;
    }

    public void OpenFinishUI()
    {
        this.CloseAllUI();
        this.mainmenuUI.SetActive(true);
    }

    public void OpenLoseUI()
    {
        this.CloseAllUI();
        this.loseUI.SetActive(true);
    }

    public void OpenSettingUI()
    {
        Time.timeScale = 0f;
        this.settingsUI.SetActive(true);
        if (GameManager.Instance.IsState(GameState.MainMenu))
        {
            this.ButtonSettingWhileMenu.SetActive(true);
            this.ButtonSettingWhilePlay.SetActive(false);
        }
        if (GameManager.Instance.IsState(GameState.GamePlay))
        {
            this.ButtonSettingWhilePlay.SetActive(true);
            this.ButtonSettingWhileMenu.SetActive(false);
        }
    }

    public void OpenModeUI()
    {
        this.CloseAllUI();
        this.modeUI.SetActive(true);
        Time.timeScale = 1f;
    }

    public void PlayButton()
    {
        this.OpenGamePlayUI();
        LevelManager.Instance.OnStart();
    }

    public void ReplayButton()
    {
        this.winUI.SetActive(false);
        Time.timeScale = 1f;
        LevelManager.Instance.OnStart();
    }

    public void NextButton()
    {
        this.winUI.SetActive(false);
        Time.timeScale = 1f;
        LevelManager.Instance.NextLevel();
        LevelManager.Instance.OnStart();
    }

    public void HomeButton()
    {
        this.PauseGame();
        GameManager.Instance.ChangeState(GameState.MainMenu);
        this.OpenMainMenuUI();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void Continue(GameObject UI)
    {
        this.Exit(UI);
        Time.timeScale = 1f;
    }

    public void EasyModeButton(GameObject UI)
    {
        LevelManager.Instance.SetLevelData(Mode.Easy);
        LevelManager.Instance.CurrentMode  = Mode.Easy;
        GameDataLevel.Instance.currentMode = Mode.Easy;
        ExitUIforMode(UI);
    }

    public void NomarlModeButton(GameObject UI)
    {
        LevelManager.Instance.SetLevelData(Mode.Nomarl);
        LevelManager.Instance.CurrentMode = Mode.Nomarl;
        GameDataLevel.Instance.currentMode = Mode.Nomarl;
        ExitUIforMode(UI);
    }

    public void HardModeButton(GameObject UI)
    {
        LevelManager.Instance.SetLevelData(Mode.Hard);
        LevelManager.Instance.CurrentMode = Mode.Hard;
        GameDataLevel.Instance.currentMode = Mode.Hard;
        ExitUIforMode(UI);
    }

    public void ExitUIforMode(GameObject UI)
    {
        SaveSystem.SaveGame();
        this.Exit(UI);
        this.OpenMainMenuUI();
    }

    public void Exit(GameObject UI)
    {
        UI.SetActive(false);
        Time.timeScale = 1f;

    }

    public void UpdateTextBotInMap()
    {
        this.textBotInMap.text = LevelManager.Instance.CurrentLevel.BotInMap().ToString();
    }

    public void CloseAllUI()
    {
        this.PauseGame();
        this.mainmenuUI.SetActive(false);
        this.winUI.SetActive(false);
        this.loseUI.SetActive(false);
        this.settingsUI.SetActive(false);
        this.gameplayUI.SetActive(false);
        this.modeUI.SetActive(false);
    }
}