using LogisticsAI.Api.Models;

namespace LogisticsAI.Api.Services;

public class RouteService
{
    // Graph: city -> list of (neighbor, costPerTon, timeHours)
    private static readonly Dictionary<string, List<(string To, double Cost, double Hours)>> Graph = new()
    {
        ["Beograd"] =
        [
            ("Novi Sad", 80, 1.5),
            ("Niš", 150, 3.0),
            ("Subotica", 160, 2.5),
            ("Šabac", 60, 1.0),
            ("Pančevo", 30, 0.5),
        ],
        ["Novi Sad"] =
        [
            ("Beograd", 80, 1.5),
            ("Subotica", 90, 1.5),
            ("Zrenjanin", 70, 1.0),
            ("Sombor", 100, 2.0),
        ],
        ["Niš"] =
        [
            ("Beograd", 150, 3.0),
            ("Leskovac", 50, 1.0),
            ("Vranje", 90, 1.5),
            ("Pirot", 60, 1.0),
        ],
        ["Subotica"] =
        [
            ("Novi Sad", 90, 1.5),
            ("Beograd", 160, 2.5),
            ("Sombor", 60, 1.0),
        ],
        ["Šabac"] =
        [
            ("Beograd", 60, 1.0),
            ("Loznica", 80, 1.5),
            ("Valjevo", 70, 1.0),
        ],
        ["Pančevo"] =
        [
            ("Beograd", 30, 0.5),
            ("Zrenjanin", 60, 1.0),
        ],
        ["Zrenjanin"] =
        [
            ("Novi Sad", 70, 1.0),
            ("Pančevo", 60, 1.0),
            ("Kikinda", 80, 1.5),
        ],
        ["Sombor"] =
        [
            ("Novi Sad", 100, 2.0),
            ("Subotica", 60, 1.0),
        ],
        ["Leskovac"] =
        [
            ("Niš", 50, 1.0),
            ("Vranje", 50, 1.0),
        ],
        ["Vranje"] =
        [
            ("Niš", 90, 1.5),
            ("Leskovac", 50, 1.0),
        ],
        ["Pirot"] =
        [
            ("Niš", 60, 1.0),
        ],
        ["Loznica"] =
        [
            ("Šabac", 80, 1.5),
            ("Valjevo", 60, 1.0),
        ],
        ["Valjevo"] =
        [
            ("Šabac", 70, 1.0),
            ("Loznica", 60, 1.0),
            ("Beograd", 100, 2.0),
        ],
        ["Kikinda"] =
        [
            ("Zrenjanin", 80, 1.5),
        ],
        ["Bar"] =
        [
            ("Podgorica", 80, 2.0),
        ],
        ["Podgorica"] =
        [
            ("Bar", 80, 2.0),
            ("Beograd", 400, 8.0),
        ],
    };

    /// <summary>
    /// Finds the cheapest route using Dijkstra on cost.
    /// </summary>
    public RouteResult? FindCheapestRoute(string from, string to, double weightTons)
    {
        var result = Dijkstra(from, to, useCost: true);
        if (result == null) return null;

        return new RouteResult
        {
            Path = result.Value.Path,
            TotalCost = result.Value.Weight * weightTons,
            TotalTimeHours = result.Value.SecondaryWeight,
            OptimizationType = "Cheapest"
        };
    }

    /// <summary>
    /// Finds the fastest route using Dijkstra on time.
    /// </summary>
    public RouteResult? FindFastestRoute(string from, string to, double weightTons)
    {
        var result = Dijkstra(from, to, useCost: false);
        if (result == null) return null;

        return new RouteResult
        {
            Path = result.Value.Path,
            TotalCost = result.Value.SecondaryWeight * weightTons,
            TotalTimeHours = result.Value.Weight,
            OptimizationType = "Fastest"
        };
    }

    public List<string> GetAvailableCities() => [.. Graph.Keys.Order()];

    private (List<string> Path, double Weight, double SecondaryWeight)? Dijkstra(string from, string to, bool useCost)
    {
        // Normalize city names
        from = NormalizeCity(from);
        to = NormalizeCity(to);

        if (!Graph.ContainsKey(from) || !Graph.ContainsKey(to))
            return null;

        var dist = new Dictionary<string, double>();
        var secondary = new Dictionary<string, double>();
        var prev = new Dictionary<string, string?>();
        var visited = new HashSet<string>();

        foreach (var city in Graph.Keys)
        {
            dist[city] = double.MaxValue;
            secondary[city] = 0;
            prev[city] = null;
        }

        dist[from] = 0;

        // Priority queue: (distance, city)
        var pq = new SortedSet<(double d, string city)>(Comparer<(double d, string city)>.Create(
            (a, b) => a.d != b.d ? a.d.CompareTo(b.d) : string.Compare(a.city, b.city, StringComparison.Ordinal)));

        pq.Add((0, from));

        while (pq.Count > 0)
        {
            var min = pq.Min;
            pq.Remove(min);
            var (d, u) = min;

            if (visited.Contains(u)) continue;
            visited.Add(u);

            if (u == to) break;

            if (!Graph.TryGetValue(u, out var neighbors)) continue;

            foreach (var (v, cost, hours) in neighbors)
            {
                var weight = useCost ? cost : hours;
                var altWeight = useCost ? hours : cost;

                var newDist = dist[u] + weight;
                if (newDist < dist[v])
                {
                    pq.Remove((dist[v], v));
                    dist[v] = newDist;
                    secondary[v] = secondary[u] + altWeight;
                    prev[v] = u;
                    pq.Add((newDist, v));
                }
            }
        }

        if (dist[to] == double.MaxValue) return null;

        // Reconstruct path
        var path = new List<string>();
        for (var at = to; at != null; at = prev[at])
            path.Add(at);
        path.Reverse();

        return (path, dist[to], secondary[to]);
    }

    private static string NormalizeCity(string city)
    {
        // Try to find a case-insensitive match in the graph
        var match = Graph.Keys.FirstOrDefault(k =>
            string.Equals(k, city, StringComparison.OrdinalIgnoreCase));
        return match ?? city;
    }
}
