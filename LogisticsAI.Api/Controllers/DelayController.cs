using LogisticsAI.Api.Models;
using LogisticsAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DelayController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<DelayController> _logger;

    public DelayController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<DelayController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Predicts the probability of delay for a transport by calling the Python ML service.
    /// </summary>
    [HttpPost("predict")]
    public async Task<IActionResult> PredictDelay([FromBody] DelayPredictionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RouteFrom) || string.IsNullOrWhiteSpace(request.RouteTo))
            return BadRequest(new { error = "RouteFrom and RouteTo are required." });

        var pythonApiUrl = _config["PythonApi:BaseUrl"] ?? "http://localhost:8000";
        var endpoint = $"{pythonApiUrl.TrimEnd('/')}/predict";

        try
        {
            var client = _httpClientFactory.CreateClient("PythonApi");
            var response = await client.PostAsJsonAsync(endpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Python API returned {Status}: {Body}", response.StatusCode, errorBody);
                return StatusCode((int)response.StatusCode, new { error = "ML service error.", detail = errorBody });
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot reach Python ML service at {Endpoint}", endpoint);
            return StatusCode(503, new { error = "ML service is unavailable.", detail = ex.Message });
        }
    }
}
