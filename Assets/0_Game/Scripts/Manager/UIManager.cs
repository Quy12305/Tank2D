using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject       mainmenuUI;
    [SerializeField] private GameObject       winUI;
    [SerializeField] private GameObject       loseUI;
    [SerializeField] private GameObject       settingsUI;
    [SerializeField] private GameObject       gameplayUI;
    [SerializeField] private GameObject       modeUI;
    [SerializeField] private TMP_Text         textBotInMap;
    [SerializeField] private GameObject       ButtonSettingWhilePlay;
    [SerializeField] private GameObject       ButtonSettingWhileMenu;
    [SerializeField] private List<GameObject> ButtonInMainMenu;

    private void Start()
    {
        this.OpenMainMenuUI();
    }

    public void OpenMainMenuUI()
    {
        this.CloseAllUI();
        this.mainmenuUI.SetActive(true);

        foreach (GameObject button in this.ButtonInMainMenu)
        {
            ScaleButton(button.GetComponent<RectTransform>());
        }

        SoundManager.Instance.OnChangeToMenu();
    }

    public void OpenGamePlayUI()
    {
        this.CloseAllUI();
        this.gameplayUI.SetActive(true);
        Time.timeScale = 1f;
        SoundManager.Instance.OnInGame();
    }

    public void OpenFinishUI()
    {
        this.CloseAllUI();
        this.winUI.SetActive(true);
        SoundManager.Instance.OnWin();
    }

    public void OpenLoseUI()
    {
        this.CloseAllUI();
        this.loseUI.SetActive(true);
        SoundManager.Instance.OnLose();
    }

    public void OpenSettingUI()
    {
        SoundManager.Instance.OnClickButton();
        PauseGame();
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

        this.ScaleButton(this.settingsUI.GetComponent<RectTransform>());
    }

    public void OpenModeUI()
    {
        SoundManager.Instance.OnClickButton();
        this.CloseAllUI();
        this.modeUI.SetActive(true);

        this.ScaleButton(this.modeUI.GetComponent<RectTransform>());
    }

    public void PlayButton()
    {
        SoundManager.Instance.OnClickButton();
        this.OpenGamePlayUI();
        LevelManager.Instance.OnStart();
    }

    public void ReplayButton()
    {
        SoundManager.Instance.OnClickButton();
        this.OpenGamePlayUI();
        LevelManager.Instance.OnStart();
    }

    public void NextButton()
    {
        SoundManager.Instance.OnClickButton();
        this.OpenGamePlayUI();
        LevelManager.Instance.NextLevel();
        LevelManager.Instance.OnStart();
    }

    public void HomeButton()
    {
        SoundManager.Instance.OnClickButton();
        GameManager.Instance.ChangeState(GameState.MainMenu);
        this.OpenMainMenuUI();
    }

    public void PauseGame()
    {
        SoundManager.Instance.OnClickButton();
        Time.timeScale = 0f;
    }

    public void Continue(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        this.Exit(UI);
    }

    public void EasyModeButton(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.SetLevelData(Mode.Easy);
        LevelManager.Instance.CurrentMode  = Mode.Easy;
        GameDataLevel.Instance.currentMode = Mode.Easy;
        ExitUIforMode(UI);
    }

    public void NomarlModeButton(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.SetLevelData(Mode.Nomarl);
        LevelManager.Instance.CurrentMode  = Mode.Nomarl;
        GameDataLevel.Instance.currentMode = Mode.Nomarl;
        ExitUIforMode(UI);
    }

    public void HardModeButton(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.SetLevelData(Mode.Hard);
        LevelManager.Instance.CurrentMode  = Mode.Hard;
        GameDataLevel.Instance.currentMode = Mode.Hard;
        ExitUIforMode(UI);
    }

    public void ExitUIforMode(GameObject UI)
    {
        SaveLoadManager.Instance.SaveGame();
        this.Exit(UI);
        this.OpenMainMenuUI();
    }

    public void Exit(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        UI.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ScaleButton(RectTransform uiButton)
    {
        uiButton.localScale = Vector3.zero;
        uiButton.DOScale(Vector3.one, 1.5f).SetEase(Ease.InOutBack);
    }

    public void UpdateTextBotInMap()
    {
        Debug.Log("UpdateTextBotInMap");
        this.textBotInMap.text = LevelManager.Instance.CurrentLevel.BotInMap().ToString();
    }

    public void CloseAllUI()
    {
        this.mainmenuUI.SetActive(false);
        this.winUI.SetActive(false);
        this.loseUI.SetActive(false);
        this.settingsUI.SetActive(false);
        this.gameplayUI.SetActive(false);
        this.modeUI.SetActive(false);
    }
}