using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TankSpawner : Singleton<TankSpawner>
{
    [Header("References")]
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private GameObject sentryTankPrefab;
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private HealthBar healthBarPrefab;

    [Header("Spawn Settings")]
    [Range(1, 20)] public int numberOfEnemies = 1;
    [SerializeField] public float minDistanceBetweenTanks = 2f;
    public int smartBotCount = 1;
    public int dumbBotCount = 0;
    public int sentryBotCount = 1;
    public int maxActiveMobileBots = 3;
    public int respawnThreshold = 1;
    [SerializeField] private float minDistanceFromPlayerForMobileSpawn = 7.5f;
    [SerializeField] private float minDistanceFromPlayerForSentry = 5f;
    [SerializeField] private float minDistanceBetweenSentries = 4f;
    [SerializeField] private int sentryInnerBorderPadding = 2;
    [SerializeField] private int minimumSpawnRegionSize = 14;
    [SerializeField] private int minimumSpawnOpenScore = 12;

    private readonly Queue<BotBehaviorType> pendingMobileBots = new Queue<BotBehaviorType>();
    private readonly List<BotTank> activeMobileBots = new List<BotTank>();
    private readonly List<BotTank> activeSentryBots = new List<BotTank>();
    public static event System.Action<Transform> OnPlayerSpawned;
    public static event System.Action<int> OnEnemyRosterInitialized;
    public static event System.Action<BotTank> OnBotDestroyed;

    private void Start()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("Maze Generator is missing!");
            return;
        }

        mazeGenerator.OnMapGenerationCompleted += HandleMapGenerated;
    }

    private void HandleMapGenerated()
    {
        activeMobileBots.Clear();
        activeSentryBots.Clear();
        pendingMobileBots.Clear();

        SpawnPlayerAndEnemies();
    }

    private void SpawnPlayerAndEnemies()
    {
        List<Vector2Int> emptyCells = mazeGenerator.GetEmptyCells();
        if (emptyCells.Count == 0)
        {
            Debug.LogWarning("No empty cells available for spawning.");
            return;
        }

        Vector2Int playerCell = GetValidatedSpawnCell(emptyCells);
        Vector3 playerPosition = mazeGenerator.GridToWorldPosition(playerCell.x, playerCell.y);
        GameObject player = SpawnTank(playerTankPrefab, playerPosition, BotBehaviorType.Smart);
        OnPlayerSpawned?.Invoke(player.transform);

        BuildMobileQueue();
        SpawnSentryBots();
        SpawnNextMobileWave();

        FindObjectOfType<DynamicFlowManager>()?.UpdateTarget(player.transform.position);
        UIManager.Instance.UpdateTextBotInMap();
        OnEnemyRosterInitialized?.Invoke(GetRemainingEnemyCount());
    }

    private void BuildMobileQueue()
    {
        int smartCount = Mathf.Max(0, smartBotCount);
        int dumbCount = Mathf.Max(0, dumbBotCount);

        if (smartCount == 0 && dumbCount == 0)
        {
            smartCount = Mathf.Max(1, numberOfEnemies);
        }

        for (int i = 0; i < smartCount; i++)
        {
            pendingMobileBots.Enqueue(BotBehaviorType.Smart);
        }

        for (int i = 0; i < dumbCount; i++)
        {
            pendingMobileBots.Enqueue(BotBehaviorType.Dumb);
        }
    }

    private void SpawnSentryBots()
    {
        List<Vector2Int> wallCells = mazeGenerator.GetCellsByType(CellType.SolidWall);
        Transform player = GameObject.FindWithTag("Player")?.transform;

        wallCells.RemoveAll(cell =>
            cell.x <= sentryInnerBorderPadding ||
            cell.y <= sentryInnerBorderPadding ||
            cell.x >= mazeGenerator.height - 1 - sentryInnerBorderPadding ||
            cell.y >= mazeGenerator.width - 1 - sentryInnerBorderPadding);

        wallCells.RemoveAll(cell => !IsValidSentryCell(cell, player));

        int sentriesToSpawn = Mathf.Min(sentryBotCount, wallCells.Count);

        for (int i = 0; i < sentriesToSpawn; i++)
        {
            int randomIndex = Random.Range(0, wallCells.Count);
            Vector2Int cell = wallCells[randomIndex];
            wallCells.RemoveAt(randomIndex);

            Vector3 position = mazeGenerator.GridToWorldPosition(cell.x, cell.y);
            if (!IsPositionValid(position) || !IsFarEnoughFromOtherSentries(position))
            {
                continue;
            }

            BotTank sentry = SpawnBot(cell, BotBehaviorType.Sentry);
            if (sentry != null)
            {
                activeSentryBots.Add(sentry);
            }
        }
    }

    private bool IsValidSentryCell(Vector2Int cell, Transform player)
    {
        Vector3 worldPosition = mazeGenerator.GridToWorldPosition(cell.x, cell.y);

        if (player != null && Vector3.Distance(worldPosition, player.position) < minDistanceFromPlayerForSentry)
        {
            return false;
        }

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        int openDirections = 0;
        foreach (Vector2Int direction in directions)
        {
            Vector2Int stepOne = cell + direction;
            Vector2Int stepTwo = cell + direction * 2;

            if (mazeGenerator.GetCellType(stepOne.x, stepOne.y) == CellType.Empty)
            {
                openDirections++;
                continue;
            }

            if (mazeGenerator.GetCellType(stepTwo.x, stepTwo.y) == CellType.Empty)
            {
                openDirections++;
            }
        }

        return openDirections >= 1;
    }

    private bool IsFarEnoughFromOtherSentries(Vector3 position)
    {
        activeSentryBots.RemoveAll(bot => bot == null);

        foreach (BotTank sentry in activeSentryBots)
        {
            if (Vector3.Distance(sentry.transform.position, position) < minDistanceBetweenSentries)
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnNextMobileWave()
    {
        int aliveMobile = GetAliveMobileBotCount();
        if (aliveMobile > respawnThreshold)
        {
            return;
        }

        int availableSlots = Mathf.Max(0, maxActiveMobileBots - aliveMobile);
        if (availableSlots == 0)
        {
            return;
        }

        int countToSpawn = availableSlots;
        for (int i = 0; i < countToSpawn; i++)
        {
            if (pendingMobileBots.Count == 0)
            {
                break;
            }

            BotBehaviorType botType = pendingMobileBots.Dequeue();
            Vector2Int spawnCell = GetRandomEmptySpawnCell();
            BotTank bot = spawnCell.x < 0 ? null : SpawnBot(spawnCell, botType);
            if (bot != null)
            {
                activeMobileBots.Add(bot);
            }
        }

        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player != null)
        {
            FindObjectOfType<DynamicFlowManager>()?.UpdateTarget(player.position);
        }

        DOVirtual.DelayedCall(0.1f, () => UIManager.Instance.UpdateTextBotInMap());
    }

    private BotTank SpawnBot(Vector2Int spawnCell, BotBehaviorType botType)
    {
        if (spawnCell.x < 0 || spawnCell.y < 0)
        {
            return null;
        }

        if (botType != BotBehaviorType.Sentry && mazeGenerator.GetCellType(spawnCell.x, spawnCell.y) != CellType.Empty)
        {
            return null;
        }

        Vector3 position = mazeGenerator.GridToWorldPosition(spawnCell.x, spawnCell.y);
        GameObject prefab = botType == BotBehaviorType.Sentry && sentryTankPrefab != null ? sentryTankPrefab : enemyTankPrefab;
        GameObject tank = SpawnTank(prefab, position, botType);
        return tank != null ? tank.GetComponent<BotTank>() : null;
    }

    private GameObject SpawnTank(GameObject prefab, Vector3 position, BotBehaviorType botType)
    {
        GameObject tank = Instantiate(prefab, position, Quaternion.identity, transform);

        TankBase tankBase = tank.GetComponent<TankBase>();
        BotTank bot = tank.GetComponent<BotTank>();
        if (bot != null)
        {
            bot.Configure(botType);
        }

        HealthBar healthBar = Instantiate(healthBarPrefab, position, Quaternion.identity, transform);
        healthBar.OnInit(tankBase.maxHealth, tank.transform);
        tankBase.healthBar = healthBar;

        return tank;
    }

    private Vector2Int GetRandomEmptySpawnCell()
    {
        List<Vector2Int> emptyCells = mazeGenerator.GetEmptyCells();
        emptyCells.Sort((a, b) => EvaluateSpawnCellScore(b).CompareTo(EvaluateSpawnCellScore(a)));
        Transform player = GameObject.FindWithTag("Player")?.transform;

        while (emptyCells.Count > 0)
        {
            int candidatePoolSize = Mathf.Min(8, emptyCells.Count);
            int randomIndex = Random.Range(0, candidatePoolSize);
            Vector2Int cell = emptyCells[randomIndex];
            emptyCells.RemoveAt(randomIndex);

            Vector3 position = mazeGenerator.GridToWorldPosition(cell.x, cell.y);
            if (mazeGenerator.GetCellType(cell.x, cell.y) == CellType.Empty &&
                IsPositionValid(position) &&
                IsPreferredSpawnCell(cell) &&
                IsFarEnoughFromPlayer(position, player, minDistanceFromPlayerForMobileSpawn))
            {
                return cell;
            }
        }

        return new Vector2Int(-1, -1);
    }

    private Vector2Int GetValidatedSpawnCell(List<Vector2Int> cells)
    {
        cells.Sort((a, b) => EvaluateSpawnCellScore(b).CompareTo(EvaluateSpawnCellScore(a)));

        while (cells.Count > 0)
        {
            int candidatePoolSize = Mathf.Min(10, cells.Count);
            int randomIndex = Random.Range(0, candidatePoolSize);
            Vector2Int cell = cells[randomIndex];
            cells.RemoveAt(randomIndex);

            Vector3 spawnPosition = mazeGenerator.GridToWorldPosition(cell.x, cell.y);
            if (mazeGenerator.GetCellType(cell.x, cell.y) == CellType.Empty &&
                IsPositionValid(spawnPosition) &&
                IsPreferredSpawnCell(cell))
            {
                return cell;
            }
        }

        List<Vector2Int> fallbackCells = mazeGenerator.GetEmptyCells();
        return fallbackCells.Count > 0 ? fallbackCells[0] : new Vector2Int(1, 1);
    }

    private bool IsPositionValid(Vector3 newPosition)
    {
        foreach (TankBase tank in FindObjectsOfType<TankBase>())
        {
            if (Vector3.Distance(tank.transform.position, newPosition) < minDistanceBetweenTanks)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsFarEnoughFromPlayer(Vector3 position, Transform player, float minimumDistance)
    {
        if (player == null)
        {
            return true;
        }

        return Vector3.Distance(position, player.position) >= minimumDistance;
    }

    private bool IsPreferredSpawnCell(Vector2Int cell)
    {
        int regionSize = mazeGenerator.GetEmptyRegionSize(cell);
        int openScore = mazeGenerator.GetLocalOpenCellScore(cell);
        return regionSize >= minimumSpawnRegionSize && openScore >= minimumSpawnOpenScore;
    }

    private int EvaluateSpawnCellScore(Vector2Int cell)
    {
        int regionSize = mazeGenerator.GetEmptyRegionSize(cell);
        int openScore = mazeGenerator.GetLocalOpenCellScore(cell);
        return regionSize * 10 + openScore;
    }

    private int GetAliveMobileBotCount()
    {
        activeMobileBots.RemoveAll(bot => bot == null);
        return activeMobileBots.Count;
    }

    public void NotifyBotDestroyed(BotTank bot)
    {
        if (bot == null)
        {
            return;
        }

        activeMobileBots.Remove(bot);
        activeSentryBots.Remove(bot);
        OnBotDestroyed?.Invoke(bot);
        SpawnNextMobileWave();
        UIManager.Instance.UpdateTextBotInMap();
    }

    public int GetRemainingEnemyCount()
    {
        activeMobileBots.RemoveAll(bot => bot == null);
        activeSentryBots.RemoveAll(bot => bot == null);
        return pendingMobileBots.Count + activeMobileBots.Count + activeSentryBots.Count;
    }

    private void OnDestroy()
    {
        if (mazeGenerator != null)
        {
            mazeGenerator.OnMapGenerationCompleted -= HandleMapGenerated;
        }
    }
}
