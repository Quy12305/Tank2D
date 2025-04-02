// using Google.OrTools.Graph;
// using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;
//
// public class DynamicFlowManager : MonoBehaviour
// {
//     [SerializeField] private MazeGenerator mazeGenerator;
//
//     private MinCostFlow flow;
//     private Dictionary<Vector2Int, int> nodeMap = new Dictionary<Vector2Int, int>();
//     private List<int> arcIndices = new List<int>(); // Danh sách lưu chỉ số các cung
//     private Vector2Int lastPlayerGridPos;
//     private int width;
//
//     private void Start()
//     {
//         StartCoroutine(WaitForMapInitialization());
//     }
//
//     private IEnumerator WaitForMapInitialization()
//     {
//         while (mazeGenerator == null || mazeGenerator.GetMap() == null)
//             yield return null;
//
//         InitializeFlow(mazeGenerator.GetMap());
//     }
//
//     private void InitializeFlow(int[,] map)
//     {
//         flow = new MinCostFlow();
//         width = mazeGenerator.width;
//         arcIndices.Clear(); // Xóa danh sách cũ khi khởi tạo lại
//         BuildGraph(map);
//     }
//
//     private void BuildGraph(int[,] map)
//     {
//         nodeMap.Clear();
//         for (int x = 0; x < mazeGenerator.width; x++)
//         {
//             for (int y = 0; y < mazeGenerator.height; y++)
//             {
//                 if (map[x, y] == 0)
//                 {
//                     int nodeId = x + y * width;
//                     nodeMap[new Vector2Int(x, y)] = nodeId;
//                     AddEdges(nodeId, x, y, map);
//                 }
//             }
//         }
//     }
//
//     private void AddEdges(int fromNode, int x, int y, int[,] map)
//     {
//         AddEdgeIfValid(fromNode, x + 1, y, map);
//         AddEdgeIfValid(fromNode, x - 1, y, map);
//         AddEdgeIfValid(fromNode, x, y + 1, map);
//         AddEdgeIfValid(fromNode, x, y - 1, map);
//     }
//
//     private void AddEdgeIfValid(int fromNode, int x, int y, int[,] map)
//     {
//         if (x >= 0 && x < width && y >= 0 && y < mazeGenerator.height && map[x, y] == 0)
//         {
//             int toNode = x + y * width;
//             // Lưu chỉ số của cung khi thêm vào
//             int arc1 = flow.AddArcWithCapacityAndUnitCost(fromNode, toNode, 2, 1);
//             int arc2 = flow.AddArcWithCapacityAndUnitCost(toNode, fromNode, 2, 1);
//             arcIndices.Add(arc1);
//             arcIndices.Add(arc2);
//         }
//     }
//
//     public void UpdatePathsIfNeeded(List<Vector2Int> enemyGridPositions, Vector2Int playerGridPos)
//     {
//         if (playerGridPos != lastPlayerGridPos)
//         {
//             lastPlayerGridPos = playerGridPos;
//             foreach (var node in nodeMap.Values) flow.SetNodeSupply(node, 0);
//
//             flow.SetNodeSupply(nodeMap[lastPlayerGridPos], -enemyGridPositions.Count);
//             foreach (var pos in enemyGridPositions)
//                 flow.SetNodeSupply(nodeMap[pos], 1);
//
//             if (flow.Solve() == MinCostFlow.Status.OPTIMAL)
//                 EventManager.OnPathsUpdated?.Invoke();
//         }
//     }
//
//     public List<Vector3> GetBotPath(Vector2Int botGridPos)
//     {
//         if (!nodeMap.ContainsKey(botGridPos)) return new List<Vector3>();
//         return ExtractPath(nodeMap[botGridPos]);
//     }
//
//     private List<Vector3> ExtractPath(int startNode)
//     {
//         List<Vector3> path = new List<Vector3>();
//         int currentNode = startNode;
//
//         while (true)
//         {
//             bool found = false;
//             // Lặp qua danh sách các chỉ số cung đã lưu
//             foreach (var arc in arcIndices)
//             {
//                 if (flow.Tail(arc) == currentNode && flow.Flow(arc) > 0)
//                 {
//                     currentNode = flow.Head(arc);
//                     path.Add(mazeGenerator.GridToWorldPosition(currentNode % width, currentNode / width));
//                     found = true;
//                     break;
//                 }
//             }
//             if (!found) break;
//         }
//
//         return path;
//     }
//
//     public Vector2Int WorldToGridPosition(Vector3 worldPos) =>
//         mazeGenerator.WorldToGridPosition(worldPos);
// }