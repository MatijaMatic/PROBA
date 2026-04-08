using LogisticsAI.Api.Models;
using LogisticsAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly AiService _aiService;
    private readonly SqlService _sqlService;
    private readonly ILogger<AiController> _logger;

    public AiController(AiService aiService, SqlService sqlService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _sqlService = sqlService;
        _logger = logger;
    }

    /// <summary>
    /// Accepts a natural language question and returns the generated SQL plus query results.
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question cannot be empty." });

        string sql;
        try
        {
            sql = await _aiService.GenerateSql(request.Question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SQL for question: {Question}", SanitizeForLog(request.Question));
            return StatusCode(502, new { error = "AI service error.", detail = ex.Message });
        }

        // Security: allow only SELECT / WITH (CTEs)
        if (!_sqlService.IsSafeQuery(sql))
        {
            _logger.LogWarning("Unsafe SQL rejected: {Sql}", sql);
            return BadRequest(new { error = "Only read-only SELECT queries are allowed.", generatedSql = sql });
        }

        try
        {
            var result = await _sqlService.ExecuteQueryAsync(sql);
            return Ok(new { sql, rowCount = result.Count, result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL execution failed: {Sql}", sql);
            return StatusCode(500, new { error = "SQL execution failed.", detail = ex.Message, generatedSql = sql });
        }
    }

    /// <summary>
    /// Removes newline characters from user input before logging to prevent log-injection attacks.
    /// </summary>
    private static string SanitizeForLog(string value) =>
        value.Replace("\r", "\\r", StringComparison.Ordinal)
             .Replace("\n", "\\n", StringComparison.Ordinal);
}
