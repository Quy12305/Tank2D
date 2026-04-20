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
    public int spawnBatchSize = 2;
    public float spawnInterval = 2f;
    [SerializeField] private float minDistanceFromPlayerForSentry = 5f;
    [SerializeField] private float minDistanceBetweenSentries = 4f;
    [SerializeField] private int sentryInnerBorderPadding = 2;

    private readonly Queue<BotBehaviorType> pendingMobileBots = new Queue<BotBehaviorType>();
    private readonly List<BotTank> activeMobileBots = new List<BotTank>();
    private readonly List<BotTank> activeSentryBots = new List<BotTank>();
    private float spawnTimer;
    public static event System.Action<Transform> OnPlayerSpawned;

    private void Start()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("Maze Generator is missing!");
            return;
        }

        mazeGenerator.OnMapGenerationCompleted += HandleMapGenerated;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsState(GameState.GamePlay))
        {
            return;
        }

        if (pendingMobileBots.Count == 0)
        {
            return;
        }

        if (GetAliveMobileBotCount() > respawnThreshold)
        {
            return;
        }

        if (GetAliveMobileBotCount() >= maxActiveMobileBots)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnNextMobileWave();
            spawnTimer = 0f;
        }
    }

    private void HandleMapGenerated()
    {
        activeMobileBots.Clear();
        activeSentryBots.Clear();
        pendingMobileBots.Clear();
        spawnTimer = 0f;

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

        Vector3 playerPosition = GetValidatedSpawnPosition(emptyCells);
        GameObject player = SpawnTank(playerTankPrefab, playerPosition, BotBehaviorType.Smart);
        OnPlayerSpawned?.Invoke(player.transform);

        BuildMobileQueue();
        SpawnSentryBots();
        SpawnNextMobileWave(true);

        FindObjectOfType<DynamicFlowManager>()?.UpdateTarget(player.transform.position);
        UIManager.Instance.UpdateTextBotInMap();
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

            BotTank sentry = SpawnBot(position, BotBehaviorType.Sentry);
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

    private void SpawnNextMobileWave(bool immediate = false)
    {
        int aliveMobile = GetAliveMobileBotCount();
        int availableSlots = Mathf.Max(0, maxActiveMobileBots - aliveMobile);
        if (availableSlots == 0)
        {
            return;
        }

        int countToSpawn = immediate ? availableSlots : Mathf.Min(spawnBatchSize, availableSlots);
        for (int i = 0; i < countToSpawn; i++)
        {
            if (pendingMobileBots.Count == 0)
            {
                break;
            }

            BotBehaviorType botType = pendingMobileBots.Dequeue();
            BotTank bot = SpawnBot(GetRandomEmptySpawnPosition(), botType);
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

    private BotTank SpawnBot(Vector3 position, BotBehaviorType botType)
    {
        if (position == Vector3.positiveInfinity)
        {
            return null;
        }

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

    private Vector3 GetRandomEmptySpawnPosition()
    {
        List<Vector2Int> emptyCells = mazeGenerator.GetEmptyCells();
        while (emptyCells.Count > 0)
        {
            int randomIndex = Random.Range(0, emptyCells.Count);
            Vector2Int cell = emptyCells[randomIndex];
            emptyCells.RemoveAt(randomIndex);

            Vector3 position = mazeGenerator.GridToWorldPosition(cell.x, cell.y);
            if (IsPositionValid(position))
            {
                return position;
            }
        }

        return Vector3.positiveInfinity;
    }

    private Vector3 GetValidatedSpawnPosition(List<Vector2Int> cells)
    {
        while (cells.Count > 0)
        {
            int randomIndex = Random.Range(0, cells.Count);
            Vector2Int cell = cells[randomIndex];
            cells.RemoveAt(randomIndex);

            Vector3 spawnPosition = mazeGenerator.GridToWorldPosition(cell.x, cell.y);
            if (IsPositionValid(spawnPosition))
            {
                return spawnPosition;
            }
        }

        return Vector3.zero;
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
