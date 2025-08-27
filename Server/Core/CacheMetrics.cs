namespace Server.Core;

public class CacheMetrics
{
    private readonly object _lock = new object();
    private TimeSpan _totalQueryTime = TimeSpan.Zero;
    private int _queryCount = 0;
    
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public TimeSpan AverageQueryTime { get; private set; }
    public long CacheSizeBytes { get; set; }
    
    public double HitRate => (CacheHits + CacheMisses) > 0 ? CacheHits / (double)(CacheHits + CacheMisses) * 100 : 0;
    
    public void UpdateAverageQueryTime(TimeSpan queryTime)
    {
        lock (_lock)
        {
            _totalQueryTime += queryTime;
            _queryCount++;
            AverageQueryTime = TimeSpan.FromTicks(_totalQueryTime.Ticks / _queryCount);
        }
    }
}