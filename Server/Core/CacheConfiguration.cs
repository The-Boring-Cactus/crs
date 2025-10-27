namespace Server.Core;

public class CacheConfiguration
{
    public CacheProviderType ProviderType { get; set; } = CacheProviderType.PostgreSQL;
    public string ConnectionString { get; set; } = string.Empty;
    public string TableName { get; set; } = "ReportsCache";
}

public enum CacheProviderType
{
    PostgreSQL,
    MSSQL
}
