using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DynamicFlowManager : Singleton<DynamicFlowManager>
{
    [SerializeField] private MazeGenerator               mazeGenerator;
    private                  MinCostFlowSolver           flow;
    private                  Dictionary<Vector2Int, int> nodeMap = new();
    private                  int                         width, height;
    private                  int[,]                      map;

    private Vector2Int lastPlayerGridPos;
    private Transform  playerTransform;
    private float      pathUpdateTimer    = 0f;
    private float      pathUpdateInterval = 2f;

    private Dictionary<Vector2Int, List<Vector3>> botPaths = new();

    public static event System.Action OnPathsUpdated;

    private void Start()
    {
        StartCoroutine(WaitForMapInitialization());
        TankSpawner.OnPlayerSpawned += OnPlayerSpawned;
    }

    private void OnDestroy() { TankSpawner.OnPlayerSpawned -= OnPlayerSpawned; }

    private void OnPlayerSpawned(Transform player)
    {
        playerTransform   = player;
        lastPlayerGridPos = WorldToGridPosition(player.position);
    }

    private IEnumerator WaitForMapInitialization()
    {
        while (mazeGenerator == null || mazeGenerator.GetMap() == null) yield return null;

        InitializeFlow(mazeGenerator.GetMap());
    }

    public void SetMap(int[,] map) { this.map = map; }

    public void UpdateTarget(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPos);
        lastPlayerGridPos = gridPos;

        // Gọi thủ công nếu bạn muốn ép cập nhật
        List<Vector2Int> enemyGridPositions = new();
        foreach (BotTank bot in FindObjectsOfType<BotTank>())
        {
            enemyGridPositions.Add(WorldToGridPosition(bot.transform.position));
        }

        UpdatePathsIfNeeded(enemyGridPositions, gridPos);
    }

    private void InitializeFlow(int[,] map)
    {
        this.map = map;
        width    = mazeGenerator.width;
        height   = mazeGenerator.height;
        int totalNodes = width * height;
        flow = new MinCostFlowSolver(totalNodes);
        nodeMap.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                {
                    int id = x + y * width;
                    nodeMap[new Vector2Int(x, y)] = id;
                    AddEdges(id, x, y, map);
                }
            }
        }
    }

    private void AddEdges(int fromNode, int x, int y, int[,] map)
    {
        AddEdgeIfValid(fromNode, x + 1, y, map);
        AddEdgeIfValid(fromNode, x - 1, y, map);
        AddEdgeIfValid(fromNode, x, y + 1, map);
        AddEdgeIfValid(fromNode, x, y - 1, map);
    }

    private void AddEdgeIfValid(int fromNode, int x, int y, int[,] map)
    {
        if (x >= 0 && x < width && y >= 0 && y < height && map[x, y] == 0)
        {
            int toNode = x + y * width;
            flow.AddEdge(fromNode, toNode, 2, 1);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        Vector2Int currentPlayerGridPos = WorldToGridPosition(playerTransform.position);

        pathUpdateTimer += Time.deltaTime;
        if (currentPlayerGridPos != lastPlayerGridPos || pathUpdateTimer >= pathUpdateInterval)
        {
            lastPlayerGridPos = currentPlayerGridPos;
            pathUpdateTimer   = 0f;

            List<Vector2Int> enemyGridPositions = new();
            foreach (BotTank bot in FindObjectsOfType<BotTank>())
            {
                enemyGridPositions.Add(WorldToGridPosition(bot.transform.position));
            }

            UpdatePathsIfNeeded(enemyGridPositions, currentPlayerGridPos);
        }
    }

    public void UpdatePathsIfNeeded(List<Vector2Int> enemyGridPositions, Vector2Int playerGridPos)
    {
        botPaths.Clear();

        // Kiểm tra điều kiện hợp lệ
        if (!nodeMap.ContainsKey(playerGridPos))
        {
            Debug.LogWarning($"Player position {playerGridPos} is invalid!");
            return;
        }

        int       sink    = nodeMap[playerGridPos];
        List<int> sources = new List<int>();

        // Chỉ thêm mỗi bot 1 lần vào sources
        foreach (var pos in enemyGridPositions)
        {
            if (nodeMap.TryGetValue(pos, out int nodeId))
            {
                sources.Add(nodeId);
            }
            else
            {
                Debug.LogWarning($"[Flow] Bot at {pos} is in invalid position!");
            }
        }

        // Tạo đồ thị mở rộng
        int totalNodes      = width * height;
        var extendedFlow    = new MinCostFlowSolver(totalNodes + 2);
        int sourceSuperNode = totalNodes;
        int sinkSuperNode   = totalNodes + 1;

        // Thêm các edge với capacity
        foreach (var kvp in nodeMap)
        {
            foreach (var neighbor in GetNeighbors(kvp.Key, map))
            {
                if (nodeMap.TryGetValue(neighbor, out int neighborId))
                {
                    // Capacity = 1 (trên mỗi cạnh ở 1 chiều chir cho 1 bot qua)
                    extendedFlow.AddUndirectedEdge(kvp.Value, neighborId, 1, 1);
                }
            }
        }

        // Kết nối super node
        foreach (int source in sources)
        {
            extendedFlow.AddUndirectedEdge(sourceSuperNode, source, 1, 0);
        }
        extendedFlow.AddUndirectedEdge(sink, sinkSuperNode, sources.Count, 0);

        // Tính toán flow
        var result = extendedFlow.MinCostMaxFlow(sourceSuperNode, sinkSuperNode, sources.Count);

        // Lấy paths và gán vào botPaths
        var paths = extendedFlow.GetAllPathsFromSources(sources);

        for (int i = 0; i < enemyGridPositions.Count; i++)
        {
            if (i < paths.Count && paths[i].Count > 0)
            {
                List<Vector3> worldPath = new List<Vector3>();
                foreach (int node in paths[i])
                {
                    int x = node % width;
                    int y = node / width;
                    worldPath.Add(mazeGenerator.GridToWorldPosition(x, y));
                }
                botPaths[enemyGridPositions[i]] = worldPath;
            }
            else
            {
                Debug.LogWarning($"No path found for bot at {enemyGridPositions[i]}");
                botPaths[enemyGridPositions[i]] = new List<Vector3>();
            }
        }

        OnPathsUpdated?.Invoke();
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos, int[,] map)
    {
        List<Vector2Int> neighbors = new();
        Vector2Int[]     dirs      = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int newPos = pos + dir;
            if (nodeMap.ContainsKey(newPos)) neighbors.Add(newPos);
        }
        return neighbors;
    }

    public List<Vector3> GetBotPath(Vector2Int botGridPos) { return botPaths.TryGetValue(botGridPos, out var path) ? path : new List<Vector3>(); }

    public Vector2Int WorldToGridPosition(Vector2 worldPos) =>
        mazeGenerator.WorldToGridPosition(worldPos);
}