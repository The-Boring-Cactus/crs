using Dapper;
using System.Data;
namespace Server.Core;

public class DataSourceManager
{
    private readonly ReportsCache _cache;
    private readonly Dictionary<string, IDbConnection> _connections;
    
    public DataSourceManager(ReportsCache cache)
    {
        _cache = cache;
        _connections = new Dictionary<string, IDbConnection>();
    }
    
    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(
        string connectionKey,
        string sql,
        object parameters = null,
        TimeSpan? cacheAge = null)
    {
        var queryKey = GenerateQueryKey(connectionKey, sql, parameters);
        var maxAge = cacheAge ?? TimeSpan.FromMinutes(15);
        
        return await _cache.GetOrExecuteAsync(queryKey, async () =>
        {
            var connection = GetConnection(connectionKey);
            return await connection.QueryAsync(sql, parameters);
        }, maxAge);
    }
    
    public void InvalidateQuery(string queryKey)
    {
        _cache.InvalidateQuery(queryKey);
    }
    
    private string GenerateQueryKey(string connectionKey, string sql, object parameters)
    {
        var paramHash = parameters?.GetHashCode().ToString() ?? "no-params";
        return $"{connectionKey}:{sql.GetHashCode()}:{paramHash}";
    }
    
    private IDbConnection GetConnection(string key)
    {
        if (!_connections.ContainsKey(key))
            throw new ArgumentException($"Connection '{key}' not configured");
            
        var connection = _connections[key];
        if (connection.State != ConnectionState.Open)
            connection.Open();
            
        return connection;
    }
    
    public void AddConnection(string key, IDbConnection connection)
    {
        _connections[key] = connection;
    }

    public async Task<object> ExecuteQueryWithProgressAsync(string reportConnectionKey, string reportSqlQuery, Dictionary<string, object> reportParameters, TimeSpan reportCacheAge, IProgress<QueryExecutionStatus> progress, CancellationToken cancellationToken)
    {
        var connection = GetConnection(reportConnectionKey);

        var command = new CommandDefinition(reportSqlQuery,reportParameters, null, reportCacheAge.Seconds,null, CommandFlags.Buffered, cancellationToken );
        return await connection.QueryFirstAsync(command);
    }
}