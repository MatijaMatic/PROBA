namespace LogisticsAI.Api.Models;

public class RouteOptimizationRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string CargoType { get; set; } = string.Empty;
    public double WeightTons { get; set; }
    public string WagonType { get; set; } = string.Empty;
}

public class RouteResult
{
    public List<string> Path { get; set; } = new();
    public double TotalCost { get; set; }
    public double TotalTimeHours { get; set; }
    public string OptimizationType { get; set; } = string.Empty;
}
