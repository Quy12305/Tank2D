using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject       mainmenuUI;
    [SerializeField] private GameObject       winUI;
    [SerializeField] private GameObject       loseUI;
    [SerializeField] private GameObject       settingsUI;
    [SerializeField] private GameObject       gameplayUI;
    [SerializeField] private GameObject       modeUI;
    [SerializeField] private GameObject       shopUI;
    [SerializeField] private TMP_Text         textBotInMap;
    [SerializeField] private TMP_Text         textGem;
    [SerializeField] private TMP_Text         textCoin;
    [SerializeField] private TMP_Text         textCoinGamePlay;
    [SerializeField] private TMP_Text         textLevelIndex;
    [SerializeField] private GameObject       imgBotInMap;
    [SerializeField] private GameObject       imgGem;
    [SerializeField] private GameObject       ButtonSettingWhilePlay;
    [SerializeField] private GameObject       ButtonSettingWhileMenu;
    [SerializeField] private Image            ImageSetting;
    [SerializeField] private GameObject       NotificationUI;
    [SerializeField] private List<GameObject> ButtonInMainMenu;
    public                   Button           shootButton;
    [SerializeField] private Button           switchAmmoButton;
    [SerializeField] private TMP_Text         switchAmmoText;
    [SerializeField] private MiniMapController miniMapController;
    private                  PlayerTank       boundPlayer;

    private void Start() { this.OpenMainMenuUI(); }

    public void OpenMainMenuUI()
    {
        Time.timeScale = 1f;
        LevelManager.Instance.CurrentLevel.DeleteAllData();
        this.CloseAllUI();
        this.mainmenuUI.SetActive(true);
        SoundManager.Instance.OnStopMove();

        foreach (GameObject button in this.ButtonInMainMenu)
        {
            ScaleButton(button.GetComponent<RectTransform>());
        }

        SoundManager.Instance.OnChangeToMenu();
    }

    public void OpenGamePlayUI()
    {
        Time.timeScale = 1f;
        this.CloseAllUI();
        this.gameplayUI.SetActive(true);
        NotificationUI.SetActive(true);
        SoundManager.Instance.OnInGame();
        RefreshGameplayHud();
        ScaleNotification();
        StartCoroutine(RefreshGameplayButtonsNextFrame());
    }

    public void OpenFinishUI()
    {
        this.winUI.SetActive(true);
        SoundManager.Instance.OnWin();
    }

    public void OpenLoseUI()
    {
        this.loseUI.SetActive(true);
        SoundManager.Instance.OnLose();
    }

    public void OpenSettingUI()
    {
        SoundManager.Instance.OnClickButton();
        PauseGame();
        this.settingsUI.SetActive(true);

        RectTransform rectTransform   = ImageSetting.rectTransform;
        Vector3       currentPosition = ImageSetting.rectTransform.localPosition;
        Vector2       currentSize     = ImageSetting.rectTransform.sizeDelta;

        if (GameManager.Instance.IsState(GameState.MainMenu))
        {
            rectTransform.localPosition = new Vector3(currentPosition.x, 116f, currentPosition.z);
            rectTransform.sizeDelta     = new Vector2(currentSize.x, 250f);

            this.ButtonSettingWhileMenu.SetActive(true);
            this.ButtonSettingWhilePlay.SetActive(false);
        }
        if (GameManager.Instance.IsState(GameState.GamePlay))
        {
            rectTransform.localPosition = new Vector3(currentPosition.x, 25f, currentPosition.z);
            rectTransform.sizeDelta     = new Vector2(currentSize.x, 450f);
            this.ButtonSettingWhilePlay.SetActive(true);
            this.ButtonSettingWhileMenu.SetActive(false);
        }
    }

    public void OpenModeUI()
    {
        SoundManager.Instance.OnClickButton();
        this.modeUI.SetActive(true);
        this.ScaleButton(this.modeUI.GetComponent<RectTransform>());
    }

    public void OpenShopUI()
    {
        SoundManager.Instance.OnClickButton();
        this.shopUI.SetActive(true);
        this.ScaleButton(this.shopUI.GetComponent<RectTransform>());
    }

    public void PlayButton()
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.OnStart();
        this.OpenGamePlayUI();
        RefreshGameplayHud();
    }

    public void ReplayButton()
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.OnStart();
        this.OpenGamePlayUI();
        RefreshGameplayHud();
        this.textGem.text = "0";
    }

    public void NextButton()
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.NextLevel();
        LevelManager.Instance.OnStart();
        this.OpenGamePlayUI();
        RefreshGameplayHud();
        this.textGem.text = "0";
    }

    public void HomeButton()
    {
        SoundManager.Instance.OnClickButton();
        GameManager.Instance.ChangeState(GameState.MainMenu);
        this.OpenMainMenuUI();
        this.textGem.text = "0";
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

    public void TankWarfareModeButton(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.SetLevelData(Mode.TankWarfare);
        LevelManager.Instance.CurrentMode  = Mode.TankWarfare;
        GameDataLevel.Instance.currentMode = Mode.TankWarfare;
        ExitUIforMode(UI);
    }

    public void GemQuestModeButton(GameObject UI)
    {
        SoundManager.Instance.OnClickButton();
        LevelManager.Instance.SetLevelData(Mode.GemQuest);
        LevelManager.Instance.CurrentMode  = Mode.GemQuest;
        GameDataLevel.Instance.currentMode = Mode.GemQuest;
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
        Time.timeScale = 1f;
        SoundManager.Instance.OnClickButton();
        UI.SetActive(false);
        if (GameManager.Instance.IsState(GameState.MainMenu))
        {
            foreach (GameObject button in this.ButtonInMainMenu)
            {
                ScaleButton(button.GetComponent<RectTransform>());
            }
        }
    }

    private void ScaleButton(RectTransform uiButton)
    {
        uiButton.localScale = Vector3.zero;
        uiButton.DOScale(Vector3.one, 1.5f).SetEase(Ease.InOutBack);
    }

    private void ScaleNotification()
    {
        NotificationUI.GetComponent<RectTransform>().localScale = Vector3.zero;
        NotificationUI.GetComponent<RectTransform>().DOScale(Vector3.one, 2.5f).SetEase(Ease.OutBounce);
    }

    public void ExitNotification()
    {
        NotificationUI.GetComponent<RectTransform>()
                      .DOScale(Vector3.zero, 0.5f)
                      .SetEase(Ease.InBack)
                      .OnComplete(() => NotificationUI.SetActive(false));
    }

    public bool IsNotificationOpen()
    {
        return NotificationUI != null && NotificationUI.activeSelf;
    }

    public void UpdateTextBotInMap()
    {
        Debug.Log("UpdateTextBotInMap");
        if (TankSpawner.Instance != null)
        {
            this.textBotInMap.text = TankSpawner.Instance.GetRemainingEnemyCount().ToString();
        }
        else
        {
            this.textBotInMap.text = LevelManager.Instance.CurrentLevel.BotInMap().ToString();
        }

        if (GameManager.Instance.IsState(GameState.GamePlay) && LevelManager.Instance.CurrentLevel.CheckWinModeBot())
        {
            GameManager.Instance.ChangeState(GameState.Win);
            DOVirtual.DelayedCall(2f, () =>
            {
                LevelManager.Instance.OnFinish();
            });
        }
    }

    public void UpdateGem(PlayerTank player) { this.textGem.text = player.Gem.ToString(); }

    public void UpdateCoin()
    {
        this.textCoin.text         = Coin.Instance.coinCount.ToString();
        this.textCoinGamePlay.text = Coin.Instance.coinCount.ToString();
    }

    public void SetLevelIndex(int levelIndex)
    {
        this.textLevelIndex.text = (levelIndex + 1).ToString();
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

    private IEnumerator RefreshGameplayButtonsNextFrame()
    {
        yield return null;
        RefreshGameplayHud();
    }

    private void SetGameplayButtonsActive(bool isActive)
    {
        if (shootButton != null)
        {
            shootButton.gameObject.SetActive(isActive);
        }

        if (switchAmmoButton != null)
        {
            switchAmmoButton.gameObject.SetActive(isActive);
        }
    }

    public void BindPlayer(PlayerTank player)
    {
        boundPlayer = player;

        if (switchAmmoButton != null)
        {
            switchAmmoButton.onClick.RemoveAllListeners();
            switchAmmoButton.onClick.AddListener(() =>
            {
                if (boundPlayer != null)
                {
                    boundPlayer.ToggleBulletType();
                }
            });
        }

        RefreshGameplayHud();
    }

    public void UpdateAmmoMode(BulletType bulletType)
    {
        if (switchAmmoText == null)
        {
            return;
        }

        switchAmmoText.text = bulletType == BulletType.Normal ? "Bullet" : "Lazer";
    }

    public void RefreshGameplayHud()
    {
        if (gameplayUI == null)
        {
            return;
        }

        bool isTankMode = LevelManager.Instance.CurrentMode == Mode.TankWarfare;

        if (imgBotInMap != null)
        {
            imgBotInMap.SetActive(isTankMode);
        }

        if (textBotInMap != null)
        {
            textBotInMap.gameObject.SetActive(isTankMode);
        }

        if (imgGem != null)
        {
            imgGem.SetActive(!isTankMode);
        }

        if (textGem != null)
        {
            textGem.gameObject.SetActive(!isTankMode);
        }

        SetGameplayButtonsActive(isTankMode);

        if (miniMapController != null)
        {
            miniMapController.gameObject.SetActive(isTankMode);
        }

        TMP_Text notificationText = NotificationUI != null ? NotificationUI.GetComponentInChildren<TMP_Text>() : null;
        if (notificationText != null)
        {
            notificationText.text = isTankMode
                ? "\"Survive and destroy all enemy tanks to win. If you're destroyed, it's game over\""
                : "\"Survive and collect all magical Gems to win. If you're destroyed, it's game over\"";
        }
    }
}
