namespace LogisticsAI.Api.Models;

public class DelayPredictionRequest
{
    public string RouteFrom { get; set; } = string.Empty;
    public string RouteTo { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string DepartureHour { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string WeatherCondition { get; set; } = string.Empty;
    public string CargoType { get; set; } = string.Empty;
}
