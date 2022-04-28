using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUnweighted : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //// No of vertices
        //int v = 8;

        //// Adjacency list for storing
        //// which vertices are connected
        //List<List<int>> adj =
        //          new List<List<int>>(v);

        //for (int i = 0; i < v; i++)
        //{
        //    adj.Add(new List<int>());
        //}

        //// Creating graph given in the above diagram. add_edge
        //// function takes adjacency list, source and destination vertex
        //// as argument and forms an edge between them.
        //AddEdge(adj, 0, 1);
        //AddEdge(adj, 1, 2);
        //AddEdge(adj, 2, 3);
        //AddEdge(adj, 3, 4);
        //AddEdge(adj, 4, 5);
        //AddEdge(adj, 5, 6);
        //AddEdge(adj, 6, 7);
        //AddEdge(adj, 1, 6);
        //AddEdge(adj, 2, 5);

        Graph graph_7f = new Graph(8);
        graph_7f.AddEdge(0, 1);
        graph_7f.AddEdge(1, 2);
        graph_7f.AddEdge(2, 3);
        graph_7f.AddEdge(3, 4);
        graph_7f.AddEdge(4, 5);
        graph_7f.AddEdge(5, 6);
        graph_7f.AddEdge(6, 7);
        graph_7f.AddEdge(1, 6);
        graph_7f.AddEdge(2, 5);

        Debug.Log(graph_7f.PrintPath(0, 7));
        Debug.Log(graph_7f.PrintPath(3, 5));
        Debug.Log(graph_7f.PrintPath(5, 3));
        Debug.Log(graph_7f.PrintPath(2, 6));
        Debug.Log(graph_7f.PrintPath(6, 2));
    }

    static List<int> GetShortestDistance(List<List<int>> adj, int s, int dest, int v)
    {
        List<int> path = new List<int>();

        // predecessor[i] array stores predecessor of i and distance
        // array stores distance of i from s
        int[] pred = new int[v];
        int[] dist = new int[v];


        if (BFS(adj, s, dest,
                v, pred, dist) == false)
        {
            Debug.Log("Given source and destination" +
                              "are not connected");
            return path;
        }

        int crawl = dest;
        path.Add(crawl);

        while (pred[crawl] != -1)
        {
            path.Add(pred[crawl]);
            crawl = pred[crawl];
        }
        path.Reverse();
        return path;
    }

    public static List<int> GetShortestDistance_1D(List<List<int>> adj, int s, int dest, int v)
    {
        List<int> path = new List<int>();

        if (dest > s)
        {
            return GetShortestDistance(adj, s, dest, v);
        }

        path = GetShortestDistance(adj, dest, s, v);
        if (path.Count > 0)
            path.Reverse();
        return path;
    }

    static void printShortestDistance(List<List<int>> adj, int s, int dest, int v)
    {
        // predecessor[i] array stores predecessor of i and distance
        // array stores distance of i from s
        int[] pred = new int[v];
        int[] dist = new int[v];


        if (BFS(adj, s, dest,
                v, pred, dist) == false)
        {
            Debug.Log("Given source and destination" +
                              "are not connected");
            return;
        }

        // List to store path
        List<int> path = new List<int>();
        int crawl = dest;
        path.Add(crawl);

        while (pred[crawl] != -1)
        {
            path.Add(pred[crawl]);
            crawl = pred[crawl];
        }

        // Print distance
        Debug.Log("Shortest path length is: " + dist[dest]);
        // Print path
        Debug.Log("Path is ::");

        for (int i = path.Count - 1;
                 i >= 0; i--)
        {
            Debug.Log(path[i] + " ");
        }
    }

    // a modified version of BFS that stores predecessor of each vertex
    // in array pred and its distance from source in array dist
    private static bool BFS(List<List<int>> adj, int src, int dest,
                            int v, int[] pred, int[] dist)
    {
        // a queue to maintain queue of vertices whose adjacency list
        // is to be scanned as per normal BFS algorithm using List of int type
        List<int> queue = new List<int>();

        // bool array visited[] which stores the information whether
        // ith vertex is reached at least once in the Breadth first search
        bool[] visited = new bool[v];

        // initially all vertices are unvisited so v[i] for all i
        // is false and as no path is yet constructed dist[i] for
        // all i set to infinity
        for (int i = 0; i < v; i++)
        {
            visited[i] = false;
            dist[i] = int.MaxValue;
            pred[i] = -1;
        }

        // now source is first to be visited and distance from
        // source to itself should be 0
        visited[src] = true;
        dist[src] = 0;
        queue.Add(src);

        // bfs Algorithm
        while (queue.Count != 0)
        {
            int u = queue[0];
            queue.RemoveAt(0);

            for (int i = 0;
                     i < adj[u].Count; i++)
            {
                if (visited[adj[u][i]] == false)
                {
                    visited[adj[u][i]] = true;
                    dist[adj[u][i]] = dist[u] + 1;
                    pred[adj[u][i]] = u;
                    queue.Add(adj[u][i]);

                    // stopping condition (when we find our destination)
                    if (adj[u][i] == dest)
                        return true;
                }
            }
        }
        return false;
    }


    // This code is contributed by Rajput-Ji
    // https://www.geeksforgeeks.org/shortest-path-unweighted-graph/
}


public class Graph
{
    List<List<int>> adj;
    int node_count;

    public Graph(int v)
    {
        adj = new List<List<int>>(v);
        for (int i = 0; i < v; i++)
        {
            adj.Add(new List<int>());
        }
        node_count = v;
    }

    public void AddEdge(int i, int j)
    {
        adj[i].Add(j);
        adj[j].Add(i);
    }

    public List<int> GetPath(int s, int dest)
    {
        return PathUnweighted.GetShortestDistance_1D(adj, s, dest, node_count);
    }

    public string PrintPath(int s, int dest)
    {
        string msg = string.Format("Path found from {0} to {1}: ", s, dest);
        List<int> path = PathUnweighted.GetShortestDistance_1D(adj, s, dest, node_count);
        if (path.Count == 0)
            return "Given source and destination are not connected";
        for (int i = 0; i < path.Count; i++)
        {
            msg += path[i].ToString();
            msg += ", ";
        }
        return msg;
    }
}