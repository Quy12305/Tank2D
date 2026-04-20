using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

    private Dictionary<int, List<Vector3>> botPaths = new();

    public static event System.Action OnPathsUpdated;

    private void Start()
    {
        StartCoroutine(WaitForMapInitialization());
        TankSpawner.OnPlayerSpawned += OnPlayerSpawned;
        if (mazeGenerator != null)
        {
            mazeGenerator.OnMapChanged += HandleMapChanged;
        }
    }

    public void Reset()
    {
        nodeMap.Clear();
        botPaths.Clear();
        flow              = null;
        lastPlayerGridPos = Vector2Int.zero;
        playerTransform   = null;
        pathUpdateTimer   = 0f;

        StartCoroutine(WaitForMapInitialization());
    }

    private void OnDestroy()
    {
        TankSpawner.OnPlayerSpawned -= OnPlayerSpawned;
        if (mazeGenerator != null)
        {
            mazeGenerator.OnMapChanged -= HandleMapChanged;
        }
    }

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

    private void HandleMapChanged()
    {
        if (mazeGenerator == null || mazeGenerator.GetMap() == null)
        {
            return;
        }

        InitializeFlow(mazeGenerator.GetMap());

        if (playerTransform == null)
        {
            return;
        }

        List<BotTank> enemyBots = FindObjectsOfType<BotTank>().Where(bot => bot != null && bot.IsMobile).ToList();
        UpdatePathsIfNeeded(enemyBots, WorldToGridPosition(playerTransform.position));
    }

    public void UpdateTarget(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPos);
        lastPlayerGridPos = gridPos;

        List<BotTank> enemyBots = FindObjectsOfType<BotTank>().Where(bot => bot != null && bot.IsMobile).ToList();
        UpdatePathsIfNeeded(enemyBots, gridPos);
    }

    private void InitializeFlow(int[,] map)
    {
        this.map = map;

        // Đang ngược giữa chiều dài và rộng ( sửa sau và ảnh hưởng đến nhiều chỗ )
        width    = mazeGenerator.height;
        height   = mazeGenerator.width;
        int totalNodes = width * height;
        flow = new MinCostFlowSolver(totalNodes);
        nodeMap.Clear();

        // Tạo nodeMap
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if ((CellType)map[x, y] == CellType.Empty)
                {
                    // Đánh số id không trùng lặp
                    int id = x + y * width;
                    nodeMap[new Vector2Int(x, y)] = id;
                }
            }
        }

        // Thêm edges sau khi đã có đầy đủ nodes
        foreach (var kvp in nodeMap)
        {
            Vector2Int pos = kvp.Key;
            int        id  = kvp.Value;
            AddEdges(id, pos.x, pos.y, map);
        }
    }

    private void AddEdges(int fromNode, int x, int y, int[,] map)
    {
        // Kiểm tra 4 hướng lân cận
        AddEdgeIfValid(fromNode, x + 1, y, map);
        AddEdgeIfValid(fromNode, x - 1, y, map);
        AddEdgeIfValid(fromNode, x, y + 1, map);
        AddEdgeIfValid(fromNode, x, y - 1, map);
    }

    private void AddEdgeIfValid(int fromNode, int x, int y, int[,] map)
    {
        if (x >= 0 && x < width && y >= 0 && y < height && (CellType)map[x, y] == CellType.Empty)
        {
            int toNode = x + y * width;
            flow.AddEdge(fromNode, toNode, 1, 1);
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

            List<BotTank> enemyBots = FindObjectsOfType<BotTank>().Where(bot => bot != null && bot.IsMobile).ToList();
            UpdatePathsIfNeeded(enemyBots, currentPlayerGridPos);
        }
    }

    public void UpdatePathsIfNeeded(List<BotTank> enemyBots, Vector2Int playerGridPos)
    {
        botPaths.Clear();

        // Kiểm tra điều kiện hợp lệ
        if (!nodeMap.ContainsKey(playerGridPos))
        {
            return;
        }

        int sinkBase = nodeMap[playerGridPos];
        List<int> sources = new List<int>();
        List<int> sourceBotIds = new List<int>();
        List<Vector2Int> sourceGridPositions = new List<Vector2Int>();

        // Thêm bot vào sources
        foreach (var bot in enemyBots)
        {
            Vector2Int pos = WorldToGridPosition(bot.transform.position);
            if (nodeMap.TryGetValue(pos, out int nodeId))
            {
                sources.Add(nodeId);
                sourceBotIds.Add(bot.GetInstanceID());
                sourceGridPositions.Add(pos);
            }
        }

        if (sources.Count == 0)
        {
            return;
        }

        // Tạo đồ thị mở rộng
        int totalNodes = width * height;
        int splitNodes = totalNodes * 2;
        var extendedFlow = new MinCostFlowSolver(splitNodes + 2);
        int sourceSuperNode = splitNodes; // Nút ảo, là nguồn đại diện cho tất cả bot đi đến
        int sinkSuperNode = splitNodes + 1; // Nút ảo nối đến player, dùng để lúc có nhiều player dễ mở rộng

        // Thêm các edge với capacity
        foreach (var kvp in nodeMap)
        {
            int baseId = kvp.Value;
            int inNode = baseId * 2;
            int outNode = baseId * 2 + 1;
            int nodeCapacity = (baseId == sinkBase) ? sources.Count : 1;

            extendedFlow.AddEdge(inNode, outNode, nodeCapacity, 0);

            foreach (var neighbor in GetNeighbors(kvp.Key, map))
            {
                if (nodeMap.TryGetValue(neighbor, out int neighborId))
                {
                    int neighborIn = neighborId * 2;
                    // Capacity = 1 (trên mỗi cạnh ở 1 chiều chỉ cho 1 bot qua)
                    int moveCost = GetMoveCost(kvp.Key, neighbor);
                    extendedFlow.AddEdge(outNode, neighborIn, 1, moveCost);
                }
            }
        }

        // Kết nối super node
        foreach (int source in sources)
        {
            int sourceIn = source * 2;
            extendedFlow.AddEdge(sourceSuperNode, sourceIn, 1, 0);
        }
        int sinkOut = sinkBase * 2 + 1;
        extendedFlow.AddEdge(sinkOut, sinkSuperNode, sources.Count, 0);

        // Tính toán flow
        var result = extendedFlow.MinCostMaxFlow(sourceSuperNode, sinkSuperNode, sources.Count);

        // Lấy paths và gán vào botPaths
        var paths = extendedFlow.GetAllPathsFromSources(sources.Select(s => s * 2).ToList());

        for (int i = 0; i < sourceBotIds.Count; i++)
        {
            if (i < paths.Count && paths[i].Count > 0)
            {
                // Chuyển đổi các node trong path sang tọa độ
                List<Vector3> worldPath = new List<Vector3>();
                Vector2Int? lastCell = null;
                foreach (int node in paths[i])
                {
                    if (node >= splitNodes) continue;
                    int baseId = node / 2;
                    int x = baseId % width;
                    int y = baseId / width;
                    var cell = new Vector2Int(x, y);
                    if (lastCell.HasValue && lastCell.Value == cell) continue;
                    lastCell = cell;
                    worldPath.Add(mazeGenerator.GridToWorldPosition(x, y));
                }
                botPaths[sourceBotIds[i]] = worldPath;
            }
            else
            {
                botPaths[sourceBotIds[i]] = new List<Vector3>();
            }
        }

        OnPathsUpdated?.Invoke();
    }

    // Lấy các ô lân cận đi được của một ô
    private List<Vector2Int> GetNeighbors(Vector2Int pos, int[,] map)
    {
        List<Vector2Int> neighbors = new();
        Vector2Int[]     dirs      =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };
        foreach (var dir in dirs)
        {
            Vector2Int newPos = pos + dir;
            if (nodeMap.ContainsKey(newPos)) neighbors.Add(newPos);
        }
        return neighbors;
    }

    private int GetMoveCost(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        bool isDiagonal = dx == 1 && dy == 1;
        return isDiagonal ? 14 : 10;
    }

    // Lấy đường đi cho bot
    public List<Vector3> GetBotPath(BotTank bot)
    {
        return botPaths.TryGetValue(bot.GetInstanceID(), out var path) ? path : new List<Vector3>();
    }

    public Vector2Int WorldToGridPosition(Vector2 worldPos) =>
        mazeGenerator.WorldToGridPosition(worldPos);
}
