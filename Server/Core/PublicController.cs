#nullable enable
using Dapper;
using FunctEngine;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Server.Core;

public class PublicController
{
    // ── Public dashboard (includes variable definitions so the view needs only one request) ──

    [ResourceMethod("dashboard/:token")]
    public async ValueTask<object> GetPublicDashboard(string token)
    {
        var row = DatabasePersistence.LoadEntityByShareToken("Dashboards", token);
        if (row == null)
            throw new ProviderException(ResponseStatus.NotFound, "Dashboard not found or not shared");

        var isPublic = (row["ispublic"] ?? row["IsPublic"])?.Value<bool>() ?? false;
        if (!isPublic)
            throw new ProviderException(ResponseStatus.NotFound, "Dashboard not found or not shared");

        var userId    = (row["userid"]    ?? row["UserId"])?.ToString()    ?? "";
        var projectId = (row["projectid"] ?? row["ProjectId"])?.ToString();
        var configJson = row["config"]?.ToString() ?? row["Config"]?.ToString();

        // Load variable definitions (with resolved dropdown options) for the dashboard's project.
        var variables = await BuildVariableDefs(userId, projectId);

        return new
        {
            id         = row["id"]?.ToString()   ?? row["Id"]?.ToString(),
            name       = row["name"]?.ToString() ?? row["Name"]?.ToString(),
            shareToken = token,
            config     = configJson,
            variables  // frontend uses these for bound Select/InputText widgets
        };
    }

    // ── Re-execute a SqlWidget with current variable values ────────────────

    [ResourceMethod(RequestMethod.Post, "dashboard/:token/refresh-widget")]
    public async ValueTask<object> RefreshPublicWidget(string token, [FromBody] RefreshWidgetRequest request)
    {
        var dashRow = LoadAndValidateDashboard(token);
        var userId  = (dashRow["userid"] ?? dashRow["UserId"])?.ToString() ?? "";

        var config     = JObject.Parse((dashRow["config"] ?? dashRow["Config"])?.ToString() ?? "{}");
        var components = config["components"] as JArray;
        if (components == null)
            return new { rows = Array.Empty<object>(), columns = Array.Empty<object>() };

        var widget = components.OfType<JObject>()
            .FirstOrDefault(c => c["i"]?.ToString() == request.WidgetId);

        if (widget == null || widget["type"]?.ToString() != "SqlWidget")
            throw new ProviderException(ResponseStatus.BadRequest, "Widget not found or not a SQL widget");

        var databaseId = widget["databaseId"]?.ToString() ?? "";
        var sqlCode    = widget["sqlCode"]?.ToString()    ?? "";

        if (string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(sqlCode))
            throw new ProviderException(ResponseStatus.BadRequest, "Widget has no database or SQL configured");

        var substituted = SubstituteVariables(sqlCode, request.Variables ?? new Dictionary<string, string>());

        using var conn = OpenOwnerConnection(userId, databaseId);
        conn.Open();

        var results   = conn.Query(substituted).ToList();
        var rowList   = new List<Dictionary<string, object>>();
        List<string>? columnNames = null;

        foreach (dynamic r in results)
        {
            Dictionary<string, object> rd = r is IDictionary<string, object> d
                ? new Dictionary<string, object>(d)
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(r))
                  ?? new Dictionary<string, object>();

            columnNames ??= rd.Keys.ToList();
            rowList.Add(rd);
        }

        var columns = (columnNames ?? new List<string>())
            .Select(c => new { field = c, header = c })
            .ToArray();

        return new { rows = rowList, columns };
    }

    // ── Execute a SQL query for a Select widget's options ─────────────────
    // Used by the public view to populate SQL-sourced Select dropdowns using
    // the dashboard owner's database connection without exposing credentials.

    [ResourceMethod(RequestMethod.Post, "dashboard/:token/select-options")]
    public async ValueTask<object> GetPublicSelectOptions(string token, [FromBody] SelectOptionsRequest request)
    {
        var dashRow = LoadAndValidateDashboard(token);
        var userId  = (dashRow["userid"] ?? dashRow["UserId"])?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(request.DatabaseId) || string.IsNullOrWhiteSpace(request.Query))
            return new { options = Array.Empty<string>() };

        var options = await ResolveQueryFirstColumn(userId, request.DatabaseId, request.Query);
        return new { options };
    }

    // ── Execute a saved C# script for a "CS Script Output" widget ──────────
    // Always runs the script's *current* saved code (never a frozen copy),
    // using the dashboard owner's database connections and datasets, without
    // requiring an authenticated WebSocket session or exposing credentials.

    [ResourceMethod(RequestMethod.Post, "dashboard/:token/run-script")]
    public ValueTask<object> RunPublicScript(string token, [FromBody] RunScriptRequest request)
    {
        var dashRow   = LoadAndValidateDashboard(token);
        var userId    = (dashRow["userid"] ?? dashRow["UserId"])?.ToString() ?? "";
        var projectId = (dashRow["projectid"] ?? dashRow["ProjectId"])?.ToString();

        if (string.IsNullOrWhiteSpace(request.ScriptId))
            return ValueTask.FromResult<object>(new { outputs = Array.Empty<object>() });

        var scripts = DatabasePersistence.LoadScripts(userId, "csharp", projectId);
        var script  = scripts.FirstOrDefault(s => (s["id"]?.ToString() ?? s["Id"]?.ToString()) == request.ScriptId);
        var code    = (script?["code"] ?? script?["Code"])?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(code))
            return ValueTask.FromResult<object>(new { outputs = Array.Empty<object>() });

        var variables = request.Variables ?? new Dictionary<string, string>();
        var outputs   = new List<object>();

        using var engine = new CodeEngine(Guid.NewGuid().ToString());
        LoadEngineDlls(engine);
        RegisterOwnerConnections(engine, userId);
        RegisterOwnerFunctions(engine, userId, projectId);

        engine.RegisterExternalFunction("GetVar", args =>
        {
            if (args.Length == 0) return "";
            var name = args[0]?.ToString() ?? "";
            return variables.TryGetValue(name, out var v) ? v : "";
        });

        engine.OutputEmitted += (_, e) => outputs.Add(new { dataType = e.OutputType, payload = e.Payload });

        try
        {
            engine.Execute(code);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PublicController] RunPublicScript failed (scriptId={request.ScriptId}): {ex.Message}");
            return ValueTask.FromResult<object>(new { outputs, error = ex.Message });
        }

        return ValueTask.FromResult<object>(new { outputs });
    }

    // ── Public report ──────────────────────────────────────────────────────

    [ResourceMethod("report/:token")]
    public ValueTask<object> GetPublicReport(string token)
    {
        var row = DatabasePersistence.LoadEntityByShareToken("Reports", token);
        if (row == null)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found or not shared");

        var isPublic = (row["ispublic"] ?? row["IsPublic"])?.Value<bool>() ?? false;
        if (!isPublic)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found or not shared");

        var configJson = row["config"]?.ToString() ?? row["Config"]?.ToString();

        return ValueTask.FromResult<object>(new
        {
            id         = row["id"]?.ToString()   ?? row["Id"]?.ToString(),
            name       = row["name"]?.ToString() ?? row["Name"]?.ToString(),
            shareToken = token,
            config     = configJson
        });
    }

    // ── Shared helpers ─────────────────────────────────────────────────────

    private static JObject LoadAndValidateDashboard(string token)
    {
        var row = DatabasePersistence.LoadEntityByShareToken("Dashboards", token);
        if (row == null || !((row["ispublic"] ?? row["IsPublic"])?.Value<bool>() ?? false))
            throw new ProviderException(ResponseStatus.NotFound, "Dashboard not found or not shared");
        return row;
    }

    // Builds the list of variable definitions with resolved dropdown options.
    private static async Task<List<object>> BuildVariableDefs(string userId, string? projectId)
    {
        var variables = DatabasePersistence.LoadVariables(userId, projectId);
        var result    = new List<object>();

        foreach (var v in variables)
        {
            var name         = (v["name"]          ?? v["Name"])?.ToString()         ?? "";
            var type         = (v["type"]          ?? v["Type"])?.ToString()         ?? "input";
            var defaultValue = (v["defaultvalue"]  ?? v["defaultValue"]  ?? v["DefaultValue"])?.ToString()  ?? "";
            var dropSrc      = (v["dropdownsource"] ?? v["dropdownSource"] ?? v["DropdownSource"])?.ToString() ?? "static";
            var dropValues   = (v["dropdownvalues"] ?? v["dropdownValues"] ?? v["DropdownValues"])?.ToString()  ?? "";
            var dropQuery    = (v["dropdownquery"]  ?? v["dropdownQuery"]  ?? v["DropdownQuery"])?.ToString()   ?? "";
            var dropConnId   = (v["dropdownconnectionid"] ?? v["dropdownConnectionId"] ?? v["DropdownConnectionId"])?.ToString() ?? "";

            List<string> options = new();

            if (type == "dropdown")
            {
                if (dropSrc == "sql" && !string.IsNullOrEmpty(dropQuery) && !string.IsNullOrEmpty(dropConnId))
                    options = await ResolveQueryFirstColumn(userId, dropConnId, dropQuery);
                else
                    options = dropValues.Split(',')
                        .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
            }

            result.Add(new { name, type, defaultValue, options });
        }

        return result;
    }

    // Creates and opens a connection belonging to the dashboard owner.
    private static System.Data.IDbConnection OpenOwnerConnection(string userId, string connectionId)
    {
        var connections = DatabasePersistence.LoadDatabaseConnections(userId);
        var cfg = connections.FirstOrDefault(c =>
            (c["id"]?.ToString() ?? c["Id"]?.ToString()) == connectionId)
            ?? throw new ProviderException(ResponseStatus.NotFound, "Database connection not found");
        return BuildConnection(cfg);
    }

    private static System.Data.IDbConnection BuildConnection(JObject cfg)
    {
        var dbType  = (cfg["type"]?.ToString() ?? cfg["Type"]?.ToString() ?? "").ToLower();
        var connStr = cfg["connectionstring"]?.ToString() ?? cfg["ConnectionString"]?.ToString();
        if (string.IsNullOrWhiteSpace(connStr)) connStr = null;
        var host    = cfg["host"]?.ToString()         ?? cfg["Host"]?.ToString();
        var dbName  = cfg["databasename"]?.ToString() ?? cfg["DatabaseName"]?.ToString();
        var dbUser  = cfg["username"]?.ToString()     ?? cfg["Username"]?.ToString();
        var dbPass  = cfg["password"]?.ToString()     ?? cfg["Password"]?.ToString();
        int.TryParse(cfg["port"]?.ToString() ?? cfg["Port"]?.ToString(), out int port);

        return dbType switch
        {
            "mssql"      => new Microsoft.Data.SqlClient.SqlConnection(
                                connStr ?? $"Server={host},{port};Database={dbName};User Id={dbUser};Password={dbPass};TrustServerCertificate=True;"),
            "postgresql" => new Npgsql.NpgsqlConnection(
                                connStr ?? $"Host={host};Port={port};Database={dbName};Username={dbUser};Password={dbPass};"),
            "mysql"      => new MySqlConnector.MySqlConnection(
                                connStr ?? $"Server={host};Port={port};Database={dbName};Uid={dbUser};Pwd={dbPass};"),
            _            => throw new InvalidOperationException($"Unsupported database type: {dbType}")
        };
    }

    // Loads the same built-in function DLLs the authenticated WebSocket session loads.
    private static void LoadEngineDlls(CodeEngine engine)
    {
        engine.LoadExternalDll("MathFunctions.dll");
        engine.LoadExternalDll("DateTimeFunctions.dll");
        engine.LoadExternalDll("DoeFunctions.dll");
        engine.LoadExternalDll("FinancialFunctions.dll");
        engine.LoadExternalDll("StringUtilities.dll");
        engine.LoadExternalDll("DataTableFunctions.dll");
    }

    // Registers every database connection belonging to the dashboard owner,
    // by both id and name, mirroring WebSocketManager.RegisterConnectionsToInterpreter.
    private static void RegisterOwnerConnections(CodeEngine engine, string userId)
    {
        var connections = DatabasePersistence.LoadDatabaseConnections(userId);
        foreach (var cfg in connections)
        {
            var id   = cfg["id"]?.ToString()   ?? cfg["Id"]?.ToString();
            var name = cfg["name"]?.ToString() ?? cfg["Name"]?.ToString() ?? id;
            if (string.IsNullOrEmpty(id)) continue;

            try
            {
                var conn = BuildConnection(cfg);
                engine.RegisterDatabaseConnection(id, conn);
                if (!string.IsNullOrEmpty(name) && name != id)
                    engine.RegisterDatabaseConnection(name, conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PublicController] Error registering connection {id}: {ex.Message}");
            }
        }
    }

    // Registers ExecuteScript/ReadDataset/ReadSpreadsheet scoped to the dashboard
    // owner's own data, mirroring WebSocketManager.RegisterProjectFunctions.
    private static void RegisterOwnerFunctions(CodeEngine engine, string userId, string? projectId)
    {
        engine.RegisterExternalFunction("ExecuteScript", args =>
        {
            if (args.Length == 0) return new List<object>();
            string scriptName = args[0]?.ToString() ?? "";

            var scripts = DatabasePersistence.LoadScripts(userId, "sql", projectId);
            var script = scripts.FirstOrDefault(s =>
                string.Equals(s["name"]?.ToString() ?? s["Name"]?.ToString() ?? "",
                              scriptName, StringComparison.OrdinalIgnoreCase));
            if (script == null) return new List<object>();

            string sqlCode = script["code"]?.ToString() ?? script["Code"]?.ToString() ?? "";
            string dbId = script["databaseconnectionid"]?.ToString()
                       ?? script["DatabaseConnectionId"]?.ToString()
                       ?? script["database"]?.ToString()
                       ?? script["Database"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(sqlCode) || string.IsNullOrEmpty(dbId)) return new List<object>();

            return engine.ExecuteDatabaseQuery(dbId, sqlCode);
        });

        engine.RegisterExternalFunction("ReadDataset", args =>
            args.Length == 0 ? new List<object>() : LoadProjectDataRows(userId, projectId, args[0]?.ToString() ?? ""));

        engine.RegisterExternalFunction("ReadSpreadsheet", args =>
            args.Length == 0 ? new List<object>() : LoadProjectDataRows(userId, projectId, args[0]?.ToString() ?? ""));
    }

    private static List<object> LoadProjectDataRows(string userId, string? projectId, string name)
    {
        var entities = DatabasePersistence.LoadEntities(userId, "Datasets", projectId);
        var entity = entities.FirstOrDefault(e =>
            string.Equals(e["name"]?.ToString() ?? e["Name"]?.ToString() ?? "", name, StringComparison.OrdinalIgnoreCase));
        if (entity == null) return new List<object>();

        string configStr = entity["config"]?.ToString() ?? entity["Config"]?.ToString() ?? "";
        if (string.IsNullOrEmpty(configStr)) return new List<object>();

        try
        {
            var config = JObject.Parse(configStr);
            var dataArray = config["data"] as JArray;
            if (dataArray == null) return new List<object>();

            return dataArray.Select(row =>
                (object)(row is JObject rowObj
                    ? rowObj.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>()
                    : new Dictionary<string, object>())
            ).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PublicController] LoadProjectDataRows failed ('{name}'): {ex.Message}");
            return new List<object>();
        }
    }

    // Runs a query and returns the first column of every row as a string list.
    private static async Task<List<string>> ResolveQueryFirstColumn(string userId, string connectionId, string query)
    {
        try
        {
            using var conn = OpenOwnerConnection(userId, connectionId);
            conn.Open();
            var rows = conn.Query(query).ToList();
            var opts = new List<string>();
            foreach (dynamic row in rows)
            {
                Dictionary<string, object> rd = row is IDictionary<string, object> d
                    ? new Dictionary<string, object>(d)
                    : JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(row))
                      ?? new Dictionary<string, object>();
                if (rd.Count > 0)
                {
                    Dictionary<string, object> typed = rd;
                    opts.Add(typed.Values.First()?.ToString() ?? "");
                }
            }
            return await Task.FromResult(opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PublicController] ResolveQueryFirstColumn failed (connectionId={connectionId}): {ex.Message}");
            return new List<string>();
        }
    }

    private static string SubstituteVariables(string sql, Dictionary<string, string> vars)
    {
        return Regex.Replace(sql, @"\{\{(\w+)\}\}", m =>
        {
            var name = m.Groups[1].Value;
            if (!vars.TryGetValue(name, out var val) || val == null) return "''";
            if (double.TryParse(val, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                return val;
            return $"'{val.Replace("'", "''")}'";
        });
    }
}

public class RefreshWidgetRequest
{
    public string WidgetId { get; set; } = "";
    public Dictionary<string, string>? Variables { get; set; }
}

public class SelectOptionsRequest
{
    public string DatabaseId { get; set; } = "";
    public string Query { get; set; } = "";
}

public class RunScriptRequest
{
    public string ScriptId { get; set; } = "";
    public Dictionary<string, string>? Variables { get; set; }
}
