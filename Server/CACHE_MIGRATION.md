# Cache Migration: RocksDB to PostgreSQL/MSSQL

## Overview

The ReportsCache system has been migrated from RocksDbSharp to support both PostgreSQL and MSSQL databases based on configuration. This provides better scalability, easier management, and integration with existing database infrastructure.

## Changes Made

### New Files Created

1. **CacheConfiguration.cs** - Configuration model for cache provider settings
2. **IReportsCache.cs** - Interface abstraction for cache implementations
3. **PostgreSQLReportsCache.cs** - PostgreSQL implementation of the cache
4. **MSSQLReportsCache.cs** - MSSQL implementation of the cache
5. **appsettings.json** - Configuration file for cache provider selection

### Modified Files

1. **ReportsCache.cs** - Refactored to use factory pattern with provider-based implementations
2. **Program.cs** - Updated to load configuration and initialize cache properly
3. **Server.csproj** - Removed RocksDb packages, added Npgsql and Microsoft.Data.SqlClient

## Configuration

### appsettings.json Structure

```json
{
  "CacheConfiguration": {
    "ProviderType": "MSSQL",
    "ConnectionString": "Server=.;Database=ReportsCache;Integrated Security=true",
    "TableName": "ReportsCache"
  }
}
```

### Supported Provider Types

- **MSSQL** - Microsoft SQL Server
- **PostgreSQL** - PostgreSQL database

### Configuration Examples

#### MSSQL (Local Server)
```json
{
  "CacheConfiguration": {
    "ProviderType": "MSSQL",
    "ConnectionString": "Server=.;Database=ReportsCache;Integrated Security=true",
    "TableName": "ReportsCache"
  }
}
```

#### MSSQL (Remote Server with Credentials)
```json
{
  "CacheConfiguration": {
    "ProviderType": "MSSQL",
    "ConnectionString": "Server=myserver.database.windows.net;Database=ReportsCache;User Id=myuser;Password=mypassword;",
    "TableName": "ReportsCache"
  }
}
```

#### PostgreSQL
```json
{
  "CacheConfiguration": {
    "ProviderType": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=reports_cache;Username=postgres;Password=yourpassword",
    "TableName": "reports_cache"
  }
}
```

## Database Schema

Both implementations automatically create the necessary table structure on initialization.

### MSSQL Schema
```sql
CREATE TABLE ReportsCache (
    [Key] NVARCHAR(500) PRIMARY KEY,
    [Data] NVARCHAR(MAX) NOT NULL,
    [Timestamp] BIGINT NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE()
);
CREATE INDEX IX_ReportsCache_Timestamp ON ReportsCache([Timestamp]);
```

### PostgreSQL Schema
```sql
CREATE TABLE reports_cache (
    key VARCHAR(500) PRIMARY KEY,
    data TEXT NOT NULL,
    timestamp BIGINT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_reports_cache_timestamp ON reports_cache(timestamp);
```

## Migration Steps

1. **Install Required NuGet Packages**
   ```bash
   dotnet add package Npgsql --version 8.0.5
   dotnet add package Microsoft.Data.SqlClient --version 5.2.2
   ```

2. **Remove Old RocksDb Packages**
   ```bash
   dotnet remove package RocksDbSharp
   dotnet remove package RocksDbNative
   ```

3. **Create Configuration File**
   - Copy `appsettings.json` to your Server directory
   - Update connection strings and provider type as needed

4. **Update Database**
   - The cache table will be created automatically on first run
   - Ensure your database exists and the connection string is correct

5. **Deploy Configuration**
   - Ensure `appsettings.json` is deployed with your application
   - Update connection strings for different environments (dev, staging, production)

## Backward Compatibility

The `ReportsCache` class maintains a backward-compatible constructor that accepts a string parameter. This constructor will:
- Detect if the string is a connection string (contains "Server=" or "Host=")
- Default to MSSQL if no clear provider is detected
- Use the string as a database name with local server if it's not a connection string

Example:
```csharp
// Old way (still works, uses MSSQL with local server)
var cache = new ReportsCache("report-system");

// New way (recommended)
var config = new CacheConfiguration
{
    ProviderType = CacheProviderType.MSSQL,
    ConnectionString = "Server=.;Database=ReportsCache;Integrated Security=true"
};
var cache = new ReportsCache(config);
await cache.InitializeAsync();
```

## Benefits

1. **Database Independence** - Switch between PostgreSQL and MSSQL without code changes
2. **Better Scalability** - Leverage enterprise database features for clustering and replication
3. **Easier Management** - Use familiar database tools for monitoring and maintenance
4. **Backup & Recovery** - Utilize existing database backup strategies
5. **Security** - Benefit from database-level security features and access controls
6. **No File System Dependencies** - Works in containerized and cloud environments without local storage

## Performance Considerations

- Both implementations use async operations for better performance
- Indexes are created on the timestamp column for efficient cache expiration queries
- Connection pooling is handled automatically by the database drivers
- Consider adding appropriate indexes based on your query patterns

## Troubleshooting

### Cache not initializing
- Ensure `InitializeAsync()` is called after creating the ReportsCache instance
- Check connection string is valid and database is accessible
- Verify database user has CREATE TABLE permissions

### Connection errors
- Test connection string independently using database tools
- Check firewall rules for database server
- Verify database server is running and accepting connections

### Performance issues
- Review database server resources
- Check for missing indexes
- Consider increasing connection pool size in connection string
- Monitor database query performance using profiling tools
