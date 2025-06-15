//using System.Text.Json;

static async Task Runner()
{
    var file = await File.ReadAllTextAsync("deps.json");
    //var file = await File.ReadAllTextAsync("reverse-deps.json");
    //var dependencyGraph = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(file)!;
    //var reverseDependencyGraph = CreateReverseDependencyGraph(dependencyGraph);

    //var v = TopologicalSort(reverseDependencyGraph);
}

static Dictionary<string, List<string>> CreateReverseDependencyGraph(
    Dictionary<string, List<string>> dependencyGraph
)
{
    return dependencyGraph;
}

static List<string> TopologicalSort(Dictionary<string, List<string>> graph)
{
    // Initialize in-degree for each node
    var inDegree = graph.ToDictionary(node => node.Key, node => 0);

    // Calculate in-degrees
    foreach (var node in graph)
    {
        foreach (var neighbor in node.Value)
        {
            inDegree[neighbor]++;
        }
    }

    // Find all nodes with in-degree 0
    var queue = new Queue<string>(inDegree.Where(pair => pair.Value == 0).Select(pair => pair.Key));

    var topoOrder = new List<string>();

    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        topoOrder.Add(current);

        foreach (var neighbor in graph[current])
        {
            inDegree[neighbor]--;
            if (inDegree[neighbor] == 0)
            {
                queue.Enqueue(neighbor);
            }
        }
    }

    // Check for cycles
    if (topoOrder.Count != graph.Count)
    {
        throw new InvalidOperationException(
            "Graph contains a cycle, topological sort not possible."
        );
    }

    return topoOrder;
}

await Runner();
