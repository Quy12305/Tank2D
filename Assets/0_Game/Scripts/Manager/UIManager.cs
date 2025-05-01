using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject mainmenuUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject modeUI;
    [SerializeField] private TMP_Text   textBotInMap;
    [SerializeField] private GameObject ButtonSettingWhilePlay;
    [SerializeField] private GameObject ButtonSettingWhileMenu;

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
    }

    public void PlayButton()
    {
        this.OpenGamePlayUI();
        LevelManager.Instance.OnStart();
    }

    public void ReplayButton()
    {
        this.winUI.SetActive(false);
        LevelManager.Instance.OnStart();
    }

    public void NextButton()
    {
        this.winUI.SetActive(false);
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

    }

    public void Continue(GameObject UI)
    {
        this.Exit(UI);
    }

    public void EasyModeButton(GameObject UI)
    {
        this.Exit(UI);
    }

    public void NomarlModeButton(GameObject UI)
    {
        this.Exit(UI);
    }

    public void HardModeButton(GameObject UI)
    {
        this.Exit(UI);
    }

    public void Exit(GameObject UI)
    {
        UI.SetActive(false);
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