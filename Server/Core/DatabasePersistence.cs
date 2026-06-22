using System.Data.Common;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server.Core;

/// <summary>
/// Provides entity CRUD persistence against the user-configured database (MSSQL, Postgres, MySQL).
/// Replaces the former RocksDB-based LoadEntityData/SaveEntityData pattern.
/// </summary>
public static class DatabasePersistence
{
    /// <summary>
    /// Creates a new DbConnection from the setup config.
    /// Returns null if the system is not yet configured.
    /// </summary>
    public static DbConnection CreateConnection()
    {
        var config = SetupConfig.Load();
        if (!config.IsConfigured || config.Database == null)
            return null;

        var cs = config.Database.GetConnectionString();
        return config.Database.Type?.ToLower() switch
        {
            "mssql" => new Microsoft.Data.SqlClient.SqlConnection(cs),
            "postgresql" => new Npgsql.NpgsqlConnection(cs),
            "mysql" => new MySqlConnector.MySqlConnection(cs),
            "oracle" => CreateOracleConnection(cs),
            _ => null
        };
    }

    // Reflection-based Oracle loader — avoids direct type reference so the Oracle
    // assembly is only loaded when an Oracle connection is actually requested.
    private static DbConnection CreateOracleConnection(string cs)
    {
        try
        {
            var asm = System.Reflection.Assembly.Load("Oracle.ManagedDataAccess");
            var type = asm.GetType("Oracle.ManagedDataAccess.Client.OracleConnection", throwOnError: true)!;
            return (DbConnection)Activator.CreateInstance(type, cs)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Oracle.ManagedDataAccess assembly failed to load: " + ex.Message +
                ". Ensure Oracle.ManagedDataAccess.Core NuGet package is deployed.", ex);
        }
    }

    // Checks by type name so no direct Oracle assembly reference is needed.
    internal static bool IsOracleConnection(DbConnection conn)
        => conn?.GetType().FullName?.StartsWith("Oracle.") == true;

    // ── Generic JSON entity operations ──────────────────────────────────
    // Each "entity table" stores rows as JSON blobs in a dedicated table,
    // keyed by (UserId, Id).  The tables were created during setup.

    // ── Scripts (SqlScripts + CodeScripts combined) ─────────────────────

    public static List<JObject> LoadScripts(string userId, string language)
    {
        using var conn = CreateConnection();
        if (conn == null) return new List<JObject>();

        conn.Open();
        string tableName = language == "csharp" ? "CodeScripts" : "SqlScripts";
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        var rows = conn.Query($"SELECT * FROM {tableName} WHERE UserId = @UserId",
            new { UserId = dbUserId }).ToList();
        var list = rows.Select(r => JObject.Parse(JsonConvert.SerializeObject(r))).ToList();
        return  list.Cast<JObject>().ToList();;
    }

    public static void SaveScript(string userId, JObject scriptObj, string language)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        var id = scriptObj["id"]?.ToString() ?? scriptObj["Id"]?.ToString();
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            scriptObj["id"] = id;
        }

        var name = scriptObj["name"]?.ToString() ?? scriptObj["Name"]?.ToString() ?? "Untitled";
        var code = scriptObj["code"]?.ToString() ?? scriptObj["Code"]?.ToString() ?? "";
        var dbConnId = scriptObj["database"]?.ToString() ?? scriptObj["DatabaseConnectionId"]?.ToString() ?? "";

        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);

        if (language == "csharp")
        {
            conn.Execute(@"DELETE FROM CodeScripts WHERE Id = @Id AND UserId = @UserId",
                new { Id = dbId, UserId = dbUserId });
            conn.Execute(@"INSERT INTO CodeScripts (Id, UserId, Name, Language, Code) 
                          VALUES (@Id, @UserId, @Name, @Language, @Code)",
                new { Id = dbId, UserId = dbUserId, Name = name, Language = language, Code = code });
        }
        else
        {
            conn.Execute(@"DELETE FROM SqlScripts WHERE Id = @Id AND UserId = @UserId",
                new { Id = dbId, UserId = dbUserId });
            conn.Execute(@"INSERT INTO SqlScripts (Id, UserId, Name, Language, Code, DatabaseConnectionId) 
                          VALUES (@Id, @UserId, @Name, @Language, @Code, @DatabaseConnectionId)",
                new { Id = dbId, UserId = dbUserId, Name = name, Language = language, Code = code, DatabaseConnectionId = dbConnId });
        }
    }

    public static void DeleteScript(string userId, string id, string language)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        
        string tableName = language == "csharp" ? "CodeScripts" : "SqlScripts";
        conn.Execute($"DELETE FROM {tableName} WHERE Id = @Id AND UserId = @UserId",
            new { Id = dbId, UserId = dbUserId });
    }

    // ── DatabaseConnections ─────────────────────────────────────────────

    public static List<JObject> LoadDatabaseConnections(string userId)
    {
        using var conn = CreateConnection();
        if (conn == null) return new List<JObject>();

        conn.Open();
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        var rows = conn.Query("SELECT * FROM DatabaseConnections WHERE UserId = @UserId OR IsGlobal = @True",
            new { UserId = dbUserId, True = true }).ToList();
        var list = rows.Select(r => JObject.Parse(JsonConvert.SerializeObject(r))).ToList();
        return  list.Cast<JObject>().ToList();;
    }

    public static void SaveDatabaseConnection(string userId, JObject connObj)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        var id = connObj["id"]?.ToString() ?? connObj["Id"]?.ToString();
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            connObj["id"] = id;
        }

        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);

        conn.Execute("DELETE FROM DatabaseConnections WHERE Id = @Id AND UserId = @UserId",
            new { Id = dbId, UserId = dbUserId });

        conn.Execute(@"INSERT INTO DatabaseConnections 
            (Id, UserId, Name, Type, Host, Port, DatabaseName, Username, Password, ConnectionString, IsGlobal, SharedWith) 
            VALUES (@Id, @UserId, @Name, @Type, @Host, @Port, @DatabaseName, @Username, @Password, @ConnectionString, @IsGlobal, @SharedWith)",
            new
            {
                Id = dbId,
                UserId = dbUserId,
                Name = connObj["name"]?.ToString() ?? connObj["Name"]?.ToString() ?? "",
                Type = connObj["type"]?.ToString() ?? connObj["Type"]?.ToString() ?? "",
                Host = connObj["host"]?.ToString() ?? connObj["Host"]?.ToString() ?? "",
                Port = (int)(connObj["port"] ?? connObj["Port"] ?? 0),
                DatabaseName = connObj["database"]?.ToString() ?? connObj["DatabaseName"]?.ToString() ?? "",
                Username = connObj["username"]?.ToString() ?? connObj["Username"]?.ToString() ?? "",
                Password = connObj["password"]?.ToString() ?? connObj["Password"]?.ToString() ?? "",
                ConnectionString = connObj["connectionString"]?.ToString() ?? connObj["ConnectionString"]?.ToString() ?? "",
                IsGlobal = (bool)(connObj["isGlobal"] ?? connObj["IsGlobal"] ?? false),
                SharedWith = connObj["sharedWith"]?.ToString() ?? connObj["SharedWith"]?.ToString() ?? ""
            });
    }

    public static void DeleteDatabaseConnection(string userId, string id)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        
        conn.Execute("DELETE FROM DatabaseConnections WHERE Id = @Id AND UserId = @UserId",
            new { Id = dbId, UserId = dbUserId });
    }

    // ── Generic JSON-blob entities (Datasets, Excels, Dashboards, Reports) ──────

    private static readonly HashSet<string> ShareableTables = new() { "Dashboards", "Reports" };

    public static List<JObject> LoadEntities(string userId, string tableName)
    {
        using var conn = CreateConnection();
        if (conn == null) return new List<JObject>();

        conn.Open();
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        var rows = conn.Query($"SELECT * FROM {tableName} WHERE UserId = @UserId",
            new { UserId = dbUserId }).ToList();
        var list = rows.Select(r => JObject.Parse(JsonConvert.SerializeObject(r))).ToList();
        return list.Cast<JObject>().ToList();
    }

    public static void SaveEntity(string userId, string tableName, JObject obj)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        var id = obj["id"]?.ToString() ?? obj["Id"]?.ToString();
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            obj["id"] = id;
        }

        var name = obj["name"]?.ToString() ?? obj["Name"]?.ToString() ?? "Untitled";
        var config = JsonConvert.SerializeObject(obj);

        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);

        conn.Execute($"DELETE FROM {tableName} WHERE Id = @Id AND UserId = @UserId",
            new { Id = dbId, UserId = dbUserId });

        if (ShareableTables.Contains(tableName))
        {
            var isPublic = (obj["isPublic"] ?? obj["IsPublic"])?.Value<bool>() ?? false;
            var shareToken = (obj["shareToken"] ?? obj["ShareToken"])?.ToString();
            conn.Execute($@"INSERT INTO {tableName} (Id, UserId, Name, Config, IsPublic, ShareToken)
                            VALUES (@Id, @UserId, @Name, @Config, @IsPublic, @ShareToken)",
                new { Id = dbId, UserId = dbUserId, Name = name, Config = config, IsPublic = isPublic, ShareToken = shareToken });
        }
        else
        {
            conn.Execute($@"INSERT INTO {tableName} (Id, UserId, Name, Config)
                            VALUES (@Id, @UserId, @Name, @Config)",
                new { Id = dbId, UserId = dbUserId, Name = name, Config = config });
        }
    }

    public static void DeleteEntity(string userId, string tableName, string id)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);

        conn.Execute($"DELETE FROM {tableName} WHERE Id = @Id AND UserId = @UserId",
            new { Id = dbId, UserId = dbUserId });
    }

    // ── Public sharing ──────────────────────────────────────────────────

    public static string GenerateShareToken(string userId, string tableName, string id)
    {
        using var conn = CreateConnection();
        if (conn == null) return null;

        conn.Open();
        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);

        var token = Guid.NewGuid().ToString("N")[..16];
        conn.Execute($"UPDATE {tableName} SET ShareToken = @Token, IsPublic = @True WHERE Id = @Id AND UserId = @UserId",
            new { Token = token, True = true, Id = dbId, UserId = dbUserId });
        return token;
    }

    public static void RevokeShareToken(string userId, string tableName, string id)
    {
        using var conn = CreateConnection();
        if (conn == null) return;

        conn.Open();
        object dbId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? id : Guid.Parse(id);
        object dbUserId = (conn is MySqlConnector.MySqlConnection || IsOracleConnection(conn)) ? userId : Guid.Parse(userId);

        conn.Execute($"UPDATE {tableName} SET ShareToken = NULL, IsPublic = @False WHERE Id = @Id AND UserId = @UserId",
            new { False = false, Id = dbId, UserId = dbUserId });
    }

    public static JObject LoadEntityByShareToken(string tableName, string shareToken)
    {
        using var conn = CreateConnection();
        if (conn == null) return null;

        conn.Open();
        var row = conn.QueryFirstOrDefault($"SELECT * FROM {tableName} WHERE ShareToken = @Token",
            new { Token = shareToken });
        if (row == null) return null;
        return JObject.Parse(JsonConvert.SerializeObject(row));
    }

    // ── Reports (DB-backed UserReport) ─────────────────────────────────

    public static List<JObject> LoadReports(string userId)
        => LoadEntities(userId, "Reports");

    public static void SaveReport(string userId, JObject reportObj)
        => SaveEntity(userId, "Reports", reportObj);

    public static void DeleteReport(string userId, string id)
        => DeleteEntity(userId, "Reports", id);
}
