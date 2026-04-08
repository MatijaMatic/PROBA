using LogisticsAI.Api.Models;
using LogisticsAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RouteController : ControllerBase
{
    private readonly RouteService _routeService;
    private readonly ILogger<RouteController> _logger;

    public RouteController(RouteService routeService, ILogger<RouteController> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    /// <summary>
    /// Returns both the cheapest and fastest routes between two cities.
    /// </summary>
    [HttpPost("optimize")]
    public IActionResult OptimizeRoute([FromBody] RouteOptimizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.From) || string.IsNullOrWhiteSpace(request.To))
            return BadRequest(new { error = "From and To cities are required." });

        if (request.WeightTons <= 0)
            return BadRequest(new { error = "WeightTons must be greater than 0." });

        var cheapest = _routeService.FindCheapestRoute(request.From, request.To, request.WeightTons);
        var fastest = _routeService.FindFastestRoute(request.From, request.To, request.WeightTons);

        if (cheapest == null && fastest == null)
        {
            return NotFound(new { error = $"No route found between '{request.From}' and '{request.To}'." });
        }

        return Ok(new
        {
            cheapestRoute = cheapest,
            fastestRoute = fastest
        });
    }

    /// <summary>
    /// Returns the list of all available cities in the route graph.
    /// </summary>
    [HttpGet("cities")]
    public IActionResult GetCities()
    {
        return Ok(new { cities = _routeService.GetAvailableCities() });
    }
}
