using UnityEngine;
using System.Collections.Generic;

public class TankSpawner : Singleton<TankSpawner>
{
    [Header("References")]
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private HealthBar healthBarPrefab;

    [Header("Spawn Settings")]
    [Range(1, 10)] public int numberOfEnemies = 1;
    [SerializeField] private float minDistanceBetweenTanks = 2f;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    public static event System.Action<Transform> OnPlayerSpawned;

    private void Start()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("Maze Generator reference is missing!");
            return;
        }

        mazeGenerator.OnMapGenerationCompleted += HandleMapGenerated;
    }

    private void HandleMapGenerated()
    {
        SpawnAllTanks();
    }

    private void SpawnAllTanks()
    {
        List<Vector2Int> emptyCells = mazeGenerator.GetEmptyCells();
        if (emptyCells.Count < numberOfEnemies + 1) // +1 for player
        {
            Debug.LogWarning("Not enough empty cells for all tanks!");
            return;
        }

        // Tổng số vị trí cần sinh (enemy + player)
        int totalPositions = numberOfEnemies + 1;
        List<Vector3> allSpawnPositions = new List<Vector3>();

        // Tìm tất cả vị trí hợp lệ trước
        while (allSpawnPositions.Count < totalPositions && emptyCells.Count > 0)
        {
            Vector2Int cell = GetRandomEmptyCell(emptyCells);
            Vector3 spawnPosition = mazeGenerator.GridToWorldPosition(cell.x, cell.y);

            if (IsPositionValid(spawnPosition, allSpawnPositions))
            {
                allSpawnPositions.Add(spawnPosition);
            }
            emptyCells.Remove(cell);
        }

        // Sinh enemy tanks trước
        for (int i = 0; i < numberOfEnemies; i++)
        {
            SpawnTank(enemyTankPrefab, allSpawnPositions[i]);
        }

        // Sinh player tank ở vị trí cuối cùng
        GameObject player = SpawnTank(playerTankPrefab, allSpawnPositions[allSpawnPositions.Count - 1]);
        OnPlayerSpawned?.Invoke(player.transform);

        FindObjectOfType<DynamicFlowManager>()?.UpdateTarget(player.transform.position);
    }

    private GameObject SpawnTank(GameObject prefab, Vector3 position)
    {
        // Tạo Tank
        GameObject tank = Instantiate(prefab, position, Quaternion.identity, transform);

        // Tạo HealthBar và liên kết với Tank
        HealthBar healthBar = Instantiate(healthBarPrefab, position, Quaternion.identity, transform);
        healthBar.OnInit(100f, tank.transform);
        tank.GetComponent<TankBase>().healthBar = healthBar;

        spawnedPositions.Add(position);
        return tank;
    }

    private Vector2Int GetRandomEmptyCell(List<Vector2Int> cells)
    {
        return cells[Random.Range(0, cells.Count)];
    }

    private bool IsPositionValid(Vector3 newPosition, List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            if (Vector3.Distance(pos, newPosition) < minDistanceBetweenTanks)
            {
                return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        if (mazeGenerator != null)
        {
            mazeGenerator.OnMapGenerationCompleted -= HandleMapGenerated;
        }
    }
}