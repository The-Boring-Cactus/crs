using System.Text.Json;

namespace Server.Core;

public class ReportsCache : IReportsCache
{
    private readonly IReportsCache _implementation;

    public ReportsCache(CacheConfiguration config)
    {
        _implementation = config.ProviderType switch
        {
            CacheProviderType.PostgreSQL => new PostgreSQLReportsCache(config),
            CacheProviderType.MSSQL => new MSSQLReportsCache(config),
            _ => throw new ArgumentException($"Unsupported cache provider: {config.ProviderType}")
        };
    }

    // Constructor for backward compatibility (uses MSSQL by default with provided connection string)
    public ReportsCache(string connectionStringOrPath)
    {
        // Try to determine if it's a connection string or a path
        var config = new CacheConfiguration();

        if (connectionStringOrPath.Contains("Server=") || connectionStringOrPath.Contains("Data Source="))
        {
            // It's a connection string - use MSSQL
            config.ProviderType = CacheProviderType.MSSQL;
            config.ConnectionString = connectionStringOrPath;
        }
        else if (connectionStringOrPath.Contains("Host=") || connectionStringOrPath.Contains("host="))
        {
            // It's a PostgreSQL connection string
            config.ProviderType = CacheProviderType.PostgreSQL;
            config.ConnectionString = connectionStringOrPath;
        }
        else
        {
            // Default to MSSQL with local server
            config.ProviderType = CacheProviderType.MSSQL;
            config.ConnectionString = $"Server=.;Database={connectionStringOrPath};Integrated Security=true";
        }

        _implementation = config.ProviderType switch
        {
            CacheProviderType.PostgreSQL => new PostgreSQLReportsCache(config),
            CacheProviderType.MSSQL => new MSSQLReportsCache(config),
            _ => throw new ArgumentException($"Unsupported cache provider: {config.ProviderType}")
        };
    }

    public async Task InitializeAsync()
    {
        await _implementation.InitializeAsync();
    }

    public async Task<T> GetOrExecuteAsync<T>(
        string queryKey,
        Func<Task<T>> queryExecutor,
        TimeSpan maxAge)
    {
        return await _implementation.GetOrExecuteAsync(queryKey, queryExecutor, maxAge);
    }

    public void InvalidateQuery(string queryKey)
    {
        _implementation.InvalidateQuery(queryKey);
    }

    public CacheMetrics GetMetrics()
    {
        return _implementation.GetMetrics();
    }

    public void Dispose()
    {
        _implementation?.Dispose();
    }
}
