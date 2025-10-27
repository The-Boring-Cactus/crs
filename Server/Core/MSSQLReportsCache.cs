using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Server.Core;

public class MSSQLReportsCache : IReportsCache
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CacheMetrics _metrics;
    private bool _initialized;

    public MSSQLReportsCache(CacheConfiguration config)
    {
        _connectionString = config.ConnectionString;
        _tableName = config.TableName;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        _metrics = new CacheMetrics();
        _initialized = false;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var createTableSql = $@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{_tableName}')
            BEGIN
                CREATE TABLE {_tableName} (
                    [Key] NVARCHAR(500) PRIMARY KEY,
                    [Data] NVARCHAR(MAX) NOT NULL,
                    [Timestamp] BIGINT NOT NULL,
                    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE()
                );
                CREATE INDEX IX_{_tableName}_Timestamp ON {_tableName}([Timestamp]);
            END
        ";

        await using var command = new SqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();

        _initialized = true;
    }

    public async Task<T> GetOrExecuteAsync<T>(
        string queryKey,
        Func<Task<T>> queryExecutor,
        TimeSpan maxAge)
    {
        var startTime = DateTime.UtcNow;
        var cacheEntry = await GetCacheEntryAsync<T>(queryKey);

        if (cacheEntry != null && !IsExpired(cacheEntry.Timestamp, maxAge))
        {
            _metrics.CacheHits++;
            _metrics.UpdateAverageQueryTime(DateTime.UtcNow - startTime);
            return cacheEntry.Data;
        }

        _metrics.CacheMisses++;
        var freshData = await queryExecutor();
        await StoreCacheEntryAsync(queryKey, freshData);
        _metrics.UpdateAverageQueryTime(DateTime.UtcNow - startTime);

        return freshData;
    }

    private async Task<CacheEntry<T>?> GetCacheEntryAsync<T>(string key)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT [Data], [Timestamp] FROM {_tableName} WHERE [Key] = @Key";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Key", key);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var dataJson = reader.GetString(0);
                var timestamp = reader.GetInt64(1);

                var data = JsonSerializer.Deserialize<T>(dataJson, _jsonOptions);
                return new CacheEntry<T>
                {
                    Data = data,
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp),
                    Key = key
                };
            }

            return null;
        }
        catch
        {
            // If deserialization fails, remove the entry
            await InvalidateQueryAsync(key);
            return null;
        }
    }

    private async Task StoreCacheEntryAsync<T>(string key, T data)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dataJson = JsonSerializer.Serialize(data, _jsonOptions);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"
            MERGE {_tableName} AS target
            USING (SELECT @Key AS [Key]) AS source
            ON target.[Key] = source.[Key]
            WHEN MATCHED THEN
                UPDATE SET [Data] = @Data, [Timestamp] = @Timestamp, [CreatedAt] = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT ([Key], [Data], [Timestamp])
                VALUES (@Key, @Data, @Timestamp);
        ";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@Data", dataJson);
        command.Parameters.AddWithValue("@Timestamp", timestamp);

        await command.ExecuteNonQueryAsync();
    }

    private bool IsExpired(DateTimeOffset timestamp, TimeSpan maxAge)
    {
        return DateTimeOffset.UtcNow - timestamp > maxAge;
    }

    public void InvalidateQuery(string queryKey)
    {
        InvalidateQueryAsync(queryKey).GetAwaiter().GetResult();
    }

    private async Task InvalidateQueryAsync(string queryKey)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $"DELETE FROM {_tableName} WHERE [Key] = @Key";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Key", queryKey);

        await command.ExecuteNonQueryAsync();
    }

    public CacheMetrics GetMetrics()
    {
        return _metrics;
    }

    public void Dispose()
    {
        // No persistent connections to dispose
    }
}
