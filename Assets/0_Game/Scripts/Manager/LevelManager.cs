using System.Collections.Generic;
using UnityEngine;

public enum Mode
{
    TankWarfare,
    GemQuest
}

public class LevelManager : Singleton<LevelManager>
{
    public List<LevelData> levels = new List<LevelData>();

    [SerializeField] private Level currentLevel;
    [SerializeField] private Mode currentMode;
    public Level CurrentLevel => currentLevel;
    public Mode CurrentMode { get => currentMode; set => currentMode = value; }

    private int   levelIndex = 0;

    [SerializeField] private List<LevelData> listLevelDataTankWarfareMode = new List<LevelData>();
    [SerializeField] private List<LevelData> listLevelDataGemQuestMode = new List<LevelData>();

    private Dictionary<Mode, List<LevelData>> modeLevelData;

    private void Awake()
    {
        SaveLoadManager.Instance.LoadGame();
        currentMode = GameDataLevel.Instance.currentMode;

        modeLevelData = new Dictionary<Mode, List<LevelData>>
        {
            { Mode.TankWarfare, this.listLevelDataTankWarfareMode },
            { Mode.GemQuest, this.listLevelDataGemQuestMode }
        };

        SetLevelData(currentMode);
        levelIndex = GameDataLevel.Instance.LevelIndexByMode[currentMode];
    }

    public void LoadLevel()
    {
        if (currentLevel != null)
        {
            currentLevel.DeleteAllData();
        }

        UIManager.Instance.SetLevelIndex(levelIndex);
        DynamicFlowManager.Instance.Reset();
        MazeGenerator.Instance.CreateMap();
    }

    public void OnStart()
    {
        SetDataToGenMapAndBot();
        LoadLevel();
        GameManager.Instance.ChangeState(GameState.GamePlay);
    }

    public void OnFinish()
    {
        if (!GameManager.Instance.IsState(GameState.Win)) return;
        UIManager.Instance.OpenFinishUI();
        GameManager.Instance.ChangeState(GameState.Win);
        Coin.Instance.SpawnWinCoins((this.levelIndex + 1) * 25);
    }

    public void OnLose()
    {
        if (!GameManager.Instance.IsState(GameState.Lose)) return;
        UIManager.Instance.OpenLoseUI();
        GameManager.Instance.ChangeState(GameState.Lose);
    }

    public void NextLevel()
    {
        if (levelIndex < levels.Count - 1)
        {
            levelIndex++;
            GameDataLevel.Instance.LevelIndexByMode[currentMode]++;
        }
        else
        {
            levelIndex = 0;
            GameDataLevel.Instance.LevelIndexByMode[currentMode] = 0;
        }
        SaveLoadManager.Instance.SaveGame();
    }

    public void SetLevelData(Mode mode)
    {
        if (modeLevelData.TryGetValue(mode, out var levelList))
        {
            levels.Clear();
            levels.AddRange(levelList);
            levelIndex = GameDataLevel.Instance.LevelIndexByMode[mode];
        }
    }

    public void SetDataToGenMapAndBot()
    {
        var levelData = levels[levelIndex];
        MazeGenerator.Instance.height                = levelData.height;
        MazeGenerator.Instance.width                 = levelData.width;
        MazeGenerator.Instance.wallDensity           = Mathf.Max(8, levelData.wallDensity - levelData.baseWallDensityReduction);
        MazeGenerator.Instance.minWallLength         = levelData.minWallLength;
        MazeGenerator.Instance.maxWallLength         = levelData.maxWallLength;
        MazeGenerator.Instance.maxWallThickness      = levelData.maxWallThickness;
        MazeGenerator.Instance.breakableWallDensity  = levelData.breakableWallDensity;
        TankSpawner.Instance.numberOfEnemies         = levelData.botCount;
        TankSpawner.Instance.minDistanceBetweenTanks = levelData.distanceBetweenBots;
        TankSpawner.Instance.smartBotCount           = levelData.smartBotCount > 0 ? levelData.smartBotCount : levelData.botCount;
        TankSpawner.Instance.dumbBotCount            = levelData.dumbBotCount;
        TankSpawner.Instance.sentryBotCount          = levelData.sentryBotCount;
        TankSpawner.Instance.maxActiveMobileBots     = Mathf.Max(1, levelData.maxActiveMobileBots);
        TankSpawner.Instance.respawnThreshold        = Mathf.Max(0, levelData.respawnThreshold);
        TankSpawner.Instance.spawnBatchSize          = Mathf.Max(1, levelData.spawnBatchSize);
        TankSpawner.Instance.spawnInterval           = Mathf.Max(0.25f, levelData.spawnInterval);
        this.currentLevel.gemToWin                  = levelData.gem;
    }
}
