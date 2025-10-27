namespace Server.Core;

public interface IReportsCache : IDisposable
{
    Task<T> GetOrExecuteAsync<T>(string queryKey, Func<Task<T>> queryExecutor, TimeSpan maxAge);
    void InvalidateQuery(string queryKey);
    CacheMetrics GetMetrics();
    Task InitializeAsync();
}
