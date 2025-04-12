using System;
using System.Collections.Generic;

public class MinCostFlowSolver
{
    private class Edge
    {
        public int from, to, capacity, cost, flow;
        public Edge residual;

        public Edge(int from, int to, int capacity, int cost)
        {
            this.from = from;
            this.to = to;
            this.capacity = capacity;
            this.cost = cost;
            this.flow = 0;
        }
    }

    private List<Edge>[] graph;
    private int nodeCount;
    private const int INF = int.MaxValue;

    public MinCostFlowSolver(int nodes)
    {
        nodeCount = nodes;
        graph = new List<Edge>[nodes];
        for (int i = 0; i < nodes; i++)
            graph[i] = new List<Edge>();
    }

    public void AddEdge(int from, int to, int capacity, int cost)
    {
        Edge forward = new Edge(from, to, capacity, cost);
        Edge backward = new Edge(to, from, 0, -cost);
        forward.residual = backward;
        backward.residual = forward;
        graph[from].Add(forward);
        graph[to].Add(backward);
    }

    public int[] MinCostMaxFlow(int source, int sink, int maxFlow)
    {
        int totalFlow = 0, totalCost = 0;
        int[] dist = new int[nodeCount];
        int[] parent = new int[nodeCount];
        Edge[] parentEdge = new Edge[nodeCount];

        while (totalFlow < maxFlow)
        {
            Array.Fill(dist, INF);
            dist[source] = 0;
            bool[] inQueue = new bool[nodeCount];
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(source);
            inQueue[source] = true;

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                inQueue[u] = false;
                foreach (Edge edge in graph[u])
                {
                    if (edge.flow < edge.capacity && dist[edge.to] > dist[u] + edge.cost)
                    {
                        dist[edge.to] = dist[u] + edge.cost;
                        parent[edge.to] = u;
                        parentEdge[edge.to] = edge;
                        if (!inQueue[edge.to])
                        {
                            queue.Enqueue(edge.to);
                            inQueue[edge.to] = true;
                        }
                    }
                }
            }

            if (dist[sink] == INF) break;

            int pushFlow = maxFlow - totalFlow;
            for (int v = sink; v != source; v = parent[v])
                pushFlow = Math.Min(pushFlow, parentEdge[v].capacity - parentEdge[v].flow);

            for (int v = sink; v != source; v = parent[v])
            {
                parentEdge[v].flow += pushFlow;
                parentEdge[v].residual.flow -= pushFlow;
                totalCost += pushFlow * parentEdge[v].cost;
            }

            totalFlow += pushFlow;
        }

        return new int[] { totalFlow, totalCost };
    }

    public List<List<int>> GetAllPathsFromSources(List<int> sources)
    {
        List<List<int>> paths = new List<List<int>>();

        foreach (int source in sources)
        {
            List<int>    path    = new List<int>();
            int          current = source;
            HashSet<int> visited = new HashSet<int>();

            while (true)
            {
                path.Add(current);
                visited.Add(current);

                Edge nextEdge = graph[current].Find(e =>
                                                        e.flow > 0 && !visited.Contains(e.to)
                );

                if (nextEdge == null) break;
                current = nextEdge.to;
            }

            paths.Add(path);
        }

        return paths;
    }
}