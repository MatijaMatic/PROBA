using Microsoft.Data.SqlClient;

namespace LogisticsAI.Api.Services;

public class SqlService
{
    private readonly string _connectionString;
    private readonly ILogger<SqlService> _logger;

    // Allowed SQL statement prefixes (read-only)
    private static readonly string[] AllowedPrefixes = ["SELECT", "WITH"];

    // Dangerous keywords that must not appear in generated SQL
    private static readonly string[] DangerousKeywords =
    [
        "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE",
        "TRUNCATE", "EXEC", "EXECUTE", "MERGE", "BULK", "OPENROWSET",
        "OPENDATASOURCE", "xp_", "sp_"
    ];

    public SqlService(IConfiguration config, ILogger<SqlService> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is not configured.");
        _logger = logger;
    }

    public bool IsSafeQuery(string sql)
    {
        var trimmed = sql.Trim();

        // Must start with SELECT or WITH (for CTEs)
        var startsWithAllowed = AllowedPrefixes.Any(p =>
            trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (!startsWithAllowed) return false;

        // Must not contain dangerous keywords as whole words
        var upperSql = trimmed.ToUpperInvariant();
        foreach (var keyword in DangerousKeywords)
        {
            // Check as whole word boundary (surrounded by non-alpha chars or at string edge)
            var idx = upperSql.IndexOf(keyword, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var before = idx == 0 || !char.IsLetterOrDigit(upperSql[idx - 1]);
                var after = (idx + keyword.Length >= upperSql.Length)
                    || !char.IsLetterOrDigit(upperSql[idx + keyword.Length]);
                if (before && after) return false;
            }
        }

        return true;
    }

    public async Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(string sql)
    {
        var result = new List<Dictionary<string, object?>>();

        _logger.LogInformation("Executing SQL: {Sql}", sql);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 30;

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            result.Add(row);
        }

        _logger.LogInformation("Query returned {RowCount} rows", result.Count);

        return result;
    }
}
