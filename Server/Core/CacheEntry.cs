namespace Server.Core;

public class CacheEntry<T>
{
    public T Data { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Key { get; set; }
}