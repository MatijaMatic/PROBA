using OpenAI;
using OpenAI.Chat;

namespace LogisticsAI.Api.Services;

public class AiService
{
    private readonly string _apiKey;
    private readonly ILogger<AiService> _logger;

    private const string SchemaDescription = @"
Database schema (SQL Server):
Table: Transports
  - Id INT PRIMARY KEY IDENTITY
  - Carrier NVARCHAR(100)       -- e.g. 'DHL', 'FedEx', 'DB Cargo'
  - RouteFrom NVARCHAR(100)     -- departure city
  - RouteTo NVARCHAR(100)       -- destination city
  - DepartureTime DATETIME
  - ArrivalTime DATETIME
  - Delay BIT                   -- 1 = delayed, 0 = on time
  - Cost FLOAT                  -- transport cost in EUR
  - CargoType NVARCHAR(100)     -- type of cargo
  - WeightTons FLOAT            -- cargo weight in tons
  - Season NVARCHAR(20)         -- 'Spring','Summer','Autumn','Winter'
  - WeatherCondition NVARCHAR(50) -- e.g. 'Clear','Rain','Snow','Fog'
";

    public AiService(IConfiguration config, ILogger<AiService> logger)
    {
        _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is not configured.");
        _logger = logger;
    }

    public async Task<string> GenerateSql(string userQuestion)
    {
        var client = new OpenAIClient(_apiKey);

        var systemPrompt = $@"You are an expert SQL generator for SQL Server databases.

{SchemaDescription}

Rules:
- Return ONLY the SQL query, nothing else.
- No markdown, no code blocks, no explanation.
- Use only SELECT statements (read-only).
- Use SQL Server syntax (TOP instead of LIMIT, GETDATE() for current date, etc.).
- Always alias computed columns for clarity.
- For percentage or probability, use CAST(... AS FLOAT) / CAST(... AS FLOAT) * 100.
- For date ranges, use DATEADD(DAY, -N, GETDATE()).

Examples:
Q: Which carrier has the most delays in the last 30 days?
A: SELECT TOP 1 Carrier, COUNT(*) AS DelayCount FROM Transports WHERE Delay = 1 AND DepartureTime >= DATEADD(DAY, -30, GETDATE()) GROUP BY Carrier ORDER BY DelayCount DESC

Q: What is the average transport cost per ton for the Belgrade-Bar route?
A: SELECT AVG(Cost / NULLIF(WeightTons, 0)) AS AvgCostPerTon FROM Transports WHERE RouteFrom = 'Beograd' AND RouteTo = 'Bar'

Q: Show top 10 clients by cargo volume this year
A: SELECT TOP 10 Carrier, SUM(WeightTons) AS TotalWeight FROM Transports WHERE YEAR(DepartureTime) = YEAR(GETDATE()) GROUP BY Carrier ORDER BY TotalWeight DESC
";

        var userPrompt = $"Generate SQL for: {userQuestion}";

        var chatClient = client.GetChatClient("gpt-4o-mini");

        var response = await chatClient.CompleteChatAsync(
            new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            });

        var sql = response.Value.Content[0].Text.Trim();

        _logger.LogInformation("Generated SQL for question '{Question}': {Sql}", userQuestion, sql);

        return sql;
    }
}
