using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : Singleton<MazeGenerator>
{
    [SerializeField]         private GameObject MapContainer;
    [Header("Map Settings")] public  int        height;
    public                           int        width;
    public                           float      tileSize;
    [SerializeField] private         GameObject wallPrefab;
    [SerializeField] private         GameObject breakableWallPrefab;
    [SerializeField] private         GameObject pathPrefab;
    public event Action                         OnMapGenerationCompleted;
    public event Action                         OnMapChanged;

    [Header("Generation Settings")] [Range(0, 100)] public int wallDensity;
    [Range(2, 8)]                                   public int minWallLength;
    [Range(3, 12)]                                  public int maxWallLength;
    [Range(1, 3)]                                   public int maxWallThickness;
    [Header("Breakable Walls")] [Range(0, 100)] public int breakableWallDensity = 12;

    private int[,]        map;
    private int[,]        emptyRegionIds;
    private Dictionary<int, int> emptyRegionSizes = new Dictionary<int, int>();
    private System.Random rand;

    private void Start()
    {
        OnMapGenerationCompleted += () =>
        {
            DynamicFlowManager.Instance.SetMap(GetMap());
            DynamicFlowManager.Instance.Reset();
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Generating map...");
            this.CreateMap();
        }
    }

    public void ClearMap()
    {
        for (int i = 0; i < this.MapContainer.transform.childCount; i++)
        {
            Destroy(this.MapContainer.transform.GetChild(i).gameObject);
        }
    }

    public void CreateMap()
    {
        this.ClearMap();

        rand = new System.Random(System.Environment.TickCount);

        if (!wallPrefab)
        {
            Debug.LogError("Wall Prefab is not assigned!");
            return;
        }

        map = GenerateMap();
        PlaceBreakableWalls();
        RebuildEmptyRegions();
        GenerateMapInUnity();
    }

    private int[,] GenerateMap()
    {
        map = new int[this.height, this.width];

        // Tạo tường bao quanh map
        for (int i = 0; i < this.height; i++)
        {
            map[i, 0]              = 1;
            map[i, this.width - 1] = 1;
        }
        for (int j = 0; j < this.width; j++)
        {
            map[0, j]               = 1;
            map[this.height - 1, j] = 1;
        }

        // Tạo tường ngẫu nhiên
        for (int i = 1; i < this.height - 1; i += 3)
        {
            for (int j = 1; j < this.width - 1; j += 3)
            {
                if (rand.Next(100) < wallDensity)
                {
                    CreateWallPattern(i, j);
                }
            }
        }

        EnsureConnectivity(); //Đảm bảo liên thông

        return map;
    }

    private void CreateWallPattern(int x, int y)
    {
        int length    = rand.Next(minWallLength, maxWallLength + 1);
        int thickness = rand.Next(1, maxWallThickness + 1);

        //Kiểm tra xem ô đó có phải nằm ở góc/ gần góc không
        bool isTouchingLeftBorder   = x <= 2;
        bool isTouchingRightBorder  = x >= this.height - 3;
        bool isTouchingTopBorder    = y >= this.width - 3;
        bool isTouchingBottomBorder = y <= 2;
        bool isNearCorner           = (isTouchingLeftBorder || isTouchingRightBorder) && (isTouchingTopBorder || isTouchingBottomBorder);

        if (isNearCorner) return;

        // Xử lý gần biên trên/dưới
        if (isTouchingTopBorder || isTouchingBottomBorder)
        {
            if (!TryPlaceWall(x, y, length, thickness, true)) return;
            return;
        }

        // Xử lý gần biên trái/phải
        if (isTouchingLeftBorder || isTouchingRightBorder)
        {
            if (!TryPlaceWall(x, y, length, thickness, false)) return;
            return;
        }

        // Tạo tường khi không phải xoay
        int rotation = rand.Next(2);
        if (rotation == 0) // Horizontal
        {
            if (!TryPlaceWall(x, y, length, thickness, false)) return;
        }
        else // Vertical
        {
            if (!TryPlaceWall(x, y, length, thickness, true)) return;
        }
    }

    private bool TryPlaceWall(int x, int y, int length, int thickness, bool vertical)
    {
        // Kiểm tra trước khi đặt tường
        bool canPlace = true;
        for (int t = 0; t < thickness; t++)
        {
            for (int i = 0; i < length; i++)
            {
                int checkX = vertical ? x + t : x + i;
                int checkY = vertical ? y + i : y + t;
                if (checkX >= this.height - 1 || checkY >= this.width - 1 || map[checkX, checkY] == 1)
                {
                    canPlace = false;
                    break;
                }
            }
            if (!canPlace) break;
        }
        if (!canPlace) return false;

        // Tạo tường
        for (int t = 0; t < thickness; t++)
        {
            if (vertical)
            {
                if (x + t < this.height - 1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        map[x + t, y + i] = 1;
                    }
                }
            }
            else
            {
                if (y + t < this.width - 1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        map[x + i, y + t] = 1;
                    }
                }
            }
        }

        return true;
    }

    private void EnsureConnectivity()
    {
        List<List<Vector2Int>> regions = FindAllRegions();
        if (regions.Count <= 1) return;

        // Chọn vùng lớn nhất
        List<Vector2Int> mainRegion = regions.OrderByDescending(r => r.Count).First();

        // Nối tất cả vùng còn lại vào vùng chính
        foreach (var region in regions.Where(r => r != mainRegion))
        {
            ConnectWithCorridor(region, mainRegion);
        }

        // Đệ quy kiểm tra lại
        regions = FindAllRegions();
        if (regions.Count > 1) EnsureConnectivity();
    }

    private List<List<Vector2Int>> FindAllRegions()
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        bool[][]               visited = new bool[this.height][];
        for (int index = 0; index < this.height; index++)
        {
            visited[index] = new bool[this.width];
        }

        for (int x = 1; x < this.height - 1; x++)
        {
            for (int y = 1; y < this.width - 1; y++)
            {
                if (map[x, y] == 0 && !visited[x][y])
                {
                    List<Vector2Int>  region = new List<Vector2Int>();
                    Queue<Vector2Int> queue  = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x][y] = true;

                    while (queue.Count > 0)
                    {
                        Vector2Int cell = queue.Dequeue();
                        region.Add(cell);

                        foreach (var dir in new Vector2Int[]
                            {
                                Vector2Int.up,
                                Vector2Int.down,
                                Vector2Int.left,
                                Vector2Int.right
                            })
                        {
                            Vector2Int next = cell + dir;
                            if (next.x >= 1 && next.x < this.height - 1 && next.y >= 1 && next.y < this.width - 1 && !visited[next.x][next.y] && map[next.x, next.y] == 0)
                            {
                                visited[next.x][next.y] = true;
                                queue.Enqueue(next);
                            }
                        }
                    }
                    regions.Add(region);
                }
            }
        }
        return regions;
    }

    private void ConnectWithCorridor(List<Vector2Int> region, List<Vector2Int> mainRegion)
    {
        Vector2Int start = region[rand.Next(region.Count)];
        Vector2Int end   = mainRegion[rand.Next(mainRegion.Count)];

        // Tạo hành hình chữ L để liên kết
        CreateHorizontalCorridor(start.x, end.x, start.y);
        CreateVerticalCorridor(start.y, end.y, end.x);
    }

    private void CreateHorizontalCorridor(int xStart, int xEnd, int y)
    {
        int dir = xStart < xEnd ? 1 : -1;
        for (int x = xStart; x != xEnd + dir; x += dir)
        {
            // Phá tường theo chiều dọc, phá 3 ô để đảm bảo đủ rộng
            for (int dy = -1; dy <= 1; dy++)
            {
                if (y + dy > 0 && y + dy < this.width - 1)
                {
                    map[x, y + dy] = 0;
                }
            }
        }
    }

    private void CreateVerticalCorridor(int yStart, int yEnd, int x)
    {
        int dir = yStart < yEnd ? 1 : -1;
        for (int y = yStart; y != yEnd + dir; y += dir)
        {
            // Phá tường theo chiều ngang, phá 3 ô để đảm bảo đủ rộng
            for (int dx = -1; dx <= 1; dx++)
            {
                if (x + dx > 0 && x + dx < this.height - 1)
                {
                    map[x + dx, y] = 0;
                }
            }
        }
    }

    private void GenerateMapInUnity()
    {
        float mapWidth  = this.height * tileSize;
        float mapHeight = this.width * tileSize;
        float offsetX   = -mapWidth / 2f;
        float offsetY   = -mapHeight / 2f;

        Transform mapParent = MapContainer.transform;

        for (int i = 0; i < this.height; i++)
        {
            for (int j = 0; j < this.width; j++)
            {
                //Tính vị trí để tạo tường/đường từ prefabs
                Vector3 position = new Vector3(
                    i * tileSize + offsetX + tileSize / 2,
                    j * tileSize + offsetY + tileSize / 2,
                    0f
                );

                if ((CellType)map[i, j] != CellType.Empty)
                {
                    bool isBreakableWall = (CellType)map[i, j] == CellType.BreakableWall;
                    if (isBreakableWall)
                    {
                        GameObject pathUnderBreakable = Instantiate(pathPrefab, position, Quaternion.identity);
                        pathUnderBreakable.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                        pathUnderBreakable.transform.parent = mapParent;
                    }

                    GameObject prefabToSpawn = isBreakableWall && breakableWallPrefab != null ? breakableWallPrefab : wallPrefab;
                    GameObject wall = Instantiate(prefabToSpawn, position, Quaternion.identity);
                    wall.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                    wall.transform.parent     = mapParent;

                    if (isBreakableWall)
                    {
                        if (wall.GetComponent<BreakableWall>() == null)
                        {
                            wall.AddComponent<BreakableWall>();
                        }

                        if (prefabToSpawn == wallPrefab)
                        {
                            SpriteRenderer renderer = wall.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                            {
                                renderer.color = new Color(0.82f, 0.66f, 0.52f, 1f);
                            }
                        }
                    }
                }
                else
                {
                    GameObject path = Instantiate(pathPrefab, position, Quaternion.identity);
                    path.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                    path.transform.parent     = mapParent;
                }
            }
        }

        mapParent.position = Vector3.zero;

        OnMapGenerationCompleted?.Invoke();
    }

    public List<Vector2Int> GetEmptyCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 1; x < this.height - 1; x++)
        {
            for (int y = 1; y < this.width - 1; y++)
            {
                if (map[x, y] == 0)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }
        return cells;
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        float mapWidth  = this.height * tileSize;
        float mapHeight = this.width * tileSize;
        float offsetX   = -mapWidth / 2f;
        float offsetY   = -mapHeight / 2f;

        return new Vector3(
            x * tileSize + offsetX + tileSize / 2,
            y * tileSize + offsetY + tileSize / 2,
            0f
        );
    }

    public bool IsCellEmpty(int x, int y)
    {
        if (x < 0 || x >= this.height || y < 0 || y >= this.width) return false;
        return map[x, y] == (int)CellType.Empty;
    }

    public int[,] GetMap() => map;

    public CellType GetCellType(int x, int y)
    {
        if (x < 0 || x >= this.height || y < 0 || y >= this.width)
        {
            return CellType.SolidWall;
        }

        return (CellType)map[x, y];
    }

    public List<Vector2Int> GetCellsByType(CellType type)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int x = 0; x < this.height; x++)
        {
            for (int y = 0; y < this.width; y++)
            {
                if ((CellType)map[x, y] == type)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }

        return cells;
    }

    public int GetEmptyRegionSize(Vector2Int cell)
    {
        if (emptyRegionIds == null || !IsWithinBounds(cell.x, cell.y) || GetCellType(cell.x, cell.y) != CellType.Empty)
        {
            return 0;
        }

        int regionId = emptyRegionIds[cell.x, cell.y];
        return emptyRegionSizes.TryGetValue(regionId, out int size) ? size : 0;
    }

    public bool IsBreakableAdjacentToSameRegion(Vector2Int breakableCell, Vector2Int emptyOriginCell)
    {
        if (GetCellType(breakableCell.x, breakableCell.y) != CellType.BreakableWall ||
            GetCellType(emptyOriginCell.x, emptyOriginCell.y) != CellType.Empty ||
            emptyRegionIds == null)
        {
            return false;
        }

        int originRegionId = emptyRegionIds[emptyOriginCell.x, emptyOriginCell.y];
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int adjacent = breakableCell + directions[i];
            if (!IsWithinBounds(adjacent.x, adjacent.y) || GetCellType(adjacent.x, adjacent.y) != CellType.Empty)
            {
                continue;
            }

            if (emptyRegionIds[adjacent.x, adjacent.y] == originRegionId)
            {
                return true;
            }
        }

        return false;
    }

    public int GetLocalOpenCellScore(Vector2Int cell, int radius = 2)
    {
        if (GetCellType(cell.x, cell.y) != CellType.Empty)
        {
            return 0;
        }

        int score = 0;
        for (int x = cell.x - radius; x <= cell.x + radius; x++)
        {
            for (int y = cell.y - radius; y <= cell.y + radius; y++)
            {
                if (!IsWithinBounds(x, y))
                {
                    continue;
                }

                if (GetCellType(x, y) == CellType.Empty)
                {
                    score++;
                }
            }
        }

        return score;
    }

    public void RemoveBreakableWallAtWorld(Vector3 worldPosition)
    {
        Vector2Int gridPosition = WorldToGridPosition(worldPosition);
        RemoveBreakableWall(gridPosition);
    }

    public void RemoveBreakableWall(Vector2Int gridPosition)
    {
        if (GetCellType(gridPosition.x, gridPosition.y) != CellType.BreakableWall)
        {
            return;
        }

        map[gridPosition.x, gridPosition.y] = (int)CellType.Empty;
        RebuildEmptyRegions();
        OnMapChanged?.Invoke();
    }

    public Vector3 GetMapCenter()
    {
        return Vector3.zero;
    }

    public float GetMapWorldWidth() { return this.height * tileSize; }

    public float GetMapWorldHeight() { return this.width * tileSize; }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        float mapWidth  = this.height * tileSize;
        float mapHeight = this.width * tileSize;
        float offsetX   = -mapWidth / 2f;
        float offsetY   = -mapHeight / 2f;

        int x = Mathf.FloorToInt((worldPos.x - offsetX) / tileSize);
        int y = Mathf.FloorToInt((worldPos.y - offsetY) / tileSize);
        return new Vector2Int(
            Mathf.Clamp(x, 0, this.height - 1),
            Mathf.Clamp(y, 0, this.width - 1)
        );
    }

    private void PlaceBreakableWalls()
    {
        if (breakableWallDensity <= 0)
        {
            return;
        }

        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 1; x < this.height - 1; x++)
        {
            for (int y = 1; y < this.width - 1; y++)
            {
                if ((CellType)map[x, y] != CellType.Empty)
                {
                    continue;
                }
                
                candidates.Add(new Vector2Int(x, y));
            }
        }

        int wallsToPlace = Mathf.RoundToInt(candidates.Count * (breakableWallDensity / 100f));
        wallsToPlace = Mathf.Clamp(wallsToPlace, 0, candidates.Count);
        for (int i = 0; i < wallsToPlace; i++)
        {
            int randomIndex = rand.Next(candidates.Count);
            Vector2Int cell = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);
            map[cell.x, cell.y] = (int)CellType.BreakableWall;
        }
    }

    private void RebuildEmptyRegions()
    {
        if (map == null)
        {
            emptyRegionIds = null;
            emptyRegionSizes.Clear();
            return;
        }

        emptyRegionIds = new int[height, width];
        emptyRegionSizes.Clear();

        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                emptyRegionIds[x, y] = -1;
            }
        }

        int regionId = 0;
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (GetCellType(x, y) != CellType.Empty || emptyRegionIds[x, y] != -1)
                {
                    continue;
                }

                int regionSize = 0;
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(new Vector2Int(x, y));
                emptyRegionIds[x, y] = regionId;

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    regionSize++;

                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int next = current + directions[i];
                        if (!IsWithinBounds(next.x, next.y) ||
                            GetCellType(next.x, next.y) != CellType.Empty ||
                            emptyRegionIds[next.x, next.y] != -1)
                        {
                            continue;
                        }

                        emptyRegionIds[next.x, next.y] = regionId;
                        queue.Enqueue(next);
                    }
                }

                emptyRegionSizes[regionId] = regionSize;
                regionId++;
            }
        }
    }

    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < height && y >= 0 && y < width;
    }
}
