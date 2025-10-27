using Npgsql;
using System.Text.Json;

namespace Server.Core;

public class PostgreSQLReportsCache : IReportsCache
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CacheMetrics _metrics;
    private bool _initialized;

    public PostgreSQLReportsCache(CacheConfiguration config)
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

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {_tableName} (
                key VARCHAR(500) PRIMARY KEY,
                data TEXT NOT NULL,
                timestamp BIGINT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX IF NOT EXISTS idx_{_tableName}_timestamp ON {_tableName}(timestamp);
        ";

        await using var command = new NpgsqlCommand(createTableSql, connection);
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
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT data, timestamp FROM {_tableName} WHERE key = @Key";
            await using var command = new NpgsqlCommand(sql, connection);
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

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"
            INSERT INTO {_tableName} (key, data, timestamp)
            VALUES (@Key, @Data, @Timestamp)
            ON CONFLICT (key)
            DO UPDATE SET data = @Data, timestamp = @Timestamp, created_at = CURRENT_TIMESTAMP
        ";

        await using var command = new NpgsqlCommand(sql, connection);
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
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $"DELETE FROM {_tableName} WHERE key = @Key";
        await using var command = new NpgsqlCommand(sql, connection);
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
