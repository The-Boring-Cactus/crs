using RocksDbSharp;
using System.Text.Json;
using System.Text;

namespace Server.Core;

public class ReportsCache : IDisposable
{
    private readonly RocksDb _db;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CacheMetrics _metrics;
    
    public ReportsCache(string cachePath)
    {
        var options = new DbOptions()
            .SetCreateIfMissing(true)
            .SetCompression(Compression.Lz4)
            .SetWriteBufferSize(32 * 1024 * 1024)
            .SetMaxWriteBufferNumber(2)
            .SetTargetFileSizeBase(128 * 1024 * 1024);
            
        _db = RocksDb.Open(options, cachePath);
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        _metrics = new CacheMetrics();
    }
    
    public async Task<T> GetOrExecuteAsync<T>(
        string queryKey, 
        Func<Task<T>> queryExecutor, 
        TimeSpan maxAge)
    {
        var startTime = DateTime.UtcNow;
        var cacheEntry = GetCacheEntry<T>(queryKey);
        
        if (cacheEntry != null && !IsExpired(cacheEntry.Timestamp, maxAge))
        {
            _metrics.CacheHits++;
            _metrics.UpdateAverageQueryTime(DateTime.UtcNow - startTime);
            return cacheEntry.Data;
        }
        
        _metrics.CacheMisses++;
        var freshData = await queryExecutor();
        StoreCacheEntry(queryKey, freshData);
        _metrics.UpdateAverageQueryTime(DateTime.UtcNow - startTime);
        
        return freshData;
    }
    
    private CacheEntry<T> GetCacheEntry<T>(string key)
    {
        var entryKey = $"entry:{key}";
        var json = _db.Get(Encoding.UTF8.GetBytes(entryKey));
        if (json == null) return null;
        
        try 
        {
            var jsonString = Encoding.UTF8.GetString(json);
            return JsonSerializer.Deserialize<CacheEntry<T>>(jsonString, _jsonOptions);
        }
        catch
        {
            _db.Remove(Encoding.UTF8.GetBytes(entryKey));
            return null;
        }
    }
    
    private void StoreCacheEntry<T>(string key, T data)
    {
        var entry = new CacheEntry<T>
        {
            Data = data,
            Timestamp = DateTimeOffset.UtcNow,
            Key = key
        };
        
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        
        using var batch = new WriteBatch();
        batch.Put(Encoding.UTF8.GetBytes($"entry:{key}"), Encoding.UTF8.GetBytes(json));
        batch.Put(Encoding.UTF8.GetBytes($"timestamp:{key}"), Encoding.UTF8.GetBytes(entry.Timestamp.ToUnixTimeMilliseconds().ToString()));
        _db.Write(batch);
    }
    
    private bool IsExpired(DateTimeOffset timestamp, TimeSpan maxAge)
    {
        return DateTimeOffset.UtcNow - timestamp > maxAge;
    }
    
    public void InvalidateQuery(string queryKey)
    {
        using var batch = new WriteBatch();
        batch.Delete(Encoding.UTF8.GetBytes($"entry:{queryKey}"));
        batch.Delete(Encoding.UTF8.GetBytes($"timestamp:{queryKey}"));
        _db.Write(batch);
    }
    
    public CacheMetrics GetMetrics()
    {
        return _metrics;
    }
    
    public void Dispose()
    {
        _db?.Dispose();
    }
}