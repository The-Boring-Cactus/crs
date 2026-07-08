using FunctEngine;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Helpers;
using System.Collections.Concurrent;

namespace Server.Core;

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections;
    private readonly IAuthService _authService;
    private readonly IUserReportsService _reportsService;
    private readonly DataSourceManager _dataSourceManager;

    public WebSocketManager(IAuthService authService, IUserReportsService reportsService, DataSourceManager dataSourceManager)
    {
        _connections = new ConcurrentDictionary<string, ConnectionInfo>();
        _authService = authService;
        _reportsService = reportsService;
        _dataSourceManager = dataSourceManager;
    }

    public async Task HandleWebSocketAsync(IWebsocketConnection socket)
    {
        var connectionId = Guid.NewGuid().ToString();
        var cancellationTokenSource = new CancellationTokenSource();

        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = connectionId,
            ConnectedAt = DateTime.UtcNow,
            WebSocket = socket,
            CancellationTokenSource = cancellationTokenSource,
            WebSocketMessageClient = new WebSocketMessageClient(socket),
            interpreter = new CodeEngine(connectionId)
        };

        connectionInfo.WebSocketMessageClient.AuthenticationMessageReceived += AuthenticationMessage;
        connectionInfo.WebSocketMessageClient.CommandMessageReceived += CommandMessage;
        connectionInfo.WebSocketMessageClient.TextMessageReceived += TextMessage;
        connectionInfo.WebSocketMessageClient.NotificationMessageReceived += NotificationMessage;
        connectionInfo.WebSocketMessageClient.ErrorMessageReceived += ErrorMessage;
        connectionInfo.WebSocketMessageClient.DataMessageReceived += DataMessage;
        connectionInfo.WebSocketMessageClient.HeartbeatMessageReceived += HeartbeatMessage;
        connectionInfo.WebSocketMessageClient.ErrorOccurred += ErrorOccurred;
        connectionInfo.interpreter.StatusUpdate += InterpreterStatusUpdate;
        connectionInfo.interpreter.OutputEmitted += InterpreterOutputEmitted;
        connectionInfo.interpreter.LoadExternalDll("MathFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("DateTimeFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("DoeFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("FinancialFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("StringUtilities.dll");
        connectionInfo.interpreter.LoadExternalDll("DataTableFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("TimeSeriesFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("NonParametricFunctions.dll");

        if (!_connections.TryAdd(connectionId, connectionInfo))
        {
            socket.Close();
            return;
        }

        try
        {
            await SendMessageAsync(connectionInfo, new HeartbeatMessage(), socket);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error for connection {connectionId}: {ex.Message}");
            _connections.TryRemove(connectionId, out _);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    private void InterpreterStatusUpdate(object sender, StatusString e)
    {
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.interpreter == sender).Value;
        if (connectionInfo == null) return;

        var notification = new NotificationMessage
        {
            Category = "Debug",
            Content = e.status,
            Title = "Execution Debug"
        };

        using var _ = SendMessageAsync(connectionInfo, notification, connectionInfo.WebSocket);
    }

    private void InterpreterOutputEmitted(object sender, OutputEmittedEventArgs e)
    {
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.interpreter == sender).Value;
        if (connectionInfo == null) return;

        var dataMsg = new DataMessage
        {
            DataType = e.OutputType,
            Payload = e.Payload
        };

        using var _ = SendMessageAsync(connectionInfo, dataMsg, connectionInfo.WebSocket);
    }

    private void HeartbeatMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;
        using var _ = SendMessageAsync(connectionInfo, new HeartbeatMessage(), socket);
    }

    private void DataMessage(object sender, MessageReceivedEventArgs e)
    {
        // Client-to-server data messages are logged; no action needed currently
        Console.WriteLine($"DataMessage received from client");
    }

    private void ErrorMessage(object sender, MessageReceivedEventArgs e)
    {
        var errMsg = e.Message as ErrorMessage;
        Console.WriteLine($"ErrorMessage received: {errMsg?.ErrorDescription}");
    }

    private void NotificationMessage(object sender, MessageReceivedEventArgs e)
    {
        // Client-to-server notifications are logged; no action needed currently
        Console.WriteLine($"NotificationMessage received from client");
    }

    private void TextMessage(object sender, MessageReceivedEventArgs e)
    {
        // Client-to-server text messages are logged; no action needed currently
        Console.WriteLine($"TextMessage received from client");
    }

    private void RegisterUserDatabaseConnections(string userId)
    {
        var conns = DatabasePersistence.LoadDatabaseConnections(userId);
        foreach (var c in conns)
        {
            var id = c["id"]?.ToString() ?? c["Id"]?.ToString();
            var type = (c["type"]?.ToString() ?? c["Type"]?.ToString() ?? "").ToLower();
            var connectionString = c["connectionString"]?.ToString() ?? c["ConnectionString"]?.ToString();
            if (string.IsNullOrWhiteSpace(connectionString)) connectionString = null;

            if (string.IsNullOrEmpty(id)) continue;

            var host = c["host"]?.ToString() ?? c["Host"]?.ToString();
            var db = c["databasename"]?.ToString() ?? c["DatabaseName"]?.ToString();
            var user = c["username"]?.ToString() ?? c["Username"]?.ToString();
            var pass = c["password"]?.ToString() ?? c["Password"]?.ToString();
            int.TryParse((c["port"]?.ToString() ?? c["Port"]?.ToString()), out int port);

            try
            {
                System.Data.IDbConnection conn = type switch
                {
                    "mssql" => new Microsoft.Data.SqlClient.SqlConnection(
                        connectionString ?? $"Server={host},{port};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;"),
                    "postgresql" => new Npgsql.NpgsqlConnection(
                        connectionString ?? $"Host={host};Port={port};Database={db};Username={user};Password={pass};"),
                    "mysql" => new MySqlConnector.MySqlConnection(
                        connectionString ?? $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};"),
                    _ => null
                };

                if (conn != null)
                    _dataSourceManager.AddConnection(id, conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering connection {id}: {ex.Message}");
            }
        }
    }

    private void RegisterConnectionsToInterpreter(ConnectionInfo connectionInfo, string userId)
    {
        var conns = DatabasePersistence.LoadDatabaseConnections(userId);
        foreach (var c in conns)
        {
            var id = c["id"]?.ToString() ?? c["Id"]?.ToString();
            var type = (c["type"]?.ToString() ?? c["Type"]?.ToString() ?? "").ToLower();
            var connectionString = c["connectionString"]?.ToString() ?? c["ConnectionString"]?.ToString();
            if (string.IsNullOrWhiteSpace(connectionString)) connectionString = null;
            var name = c["name"]?.ToString() ?? c["Name"]?.ToString() ?? id;

            if (string.IsNullOrEmpty(id)) continue;

            var host = c["host"]?.ToString() ?? c["Host"]?.ToString();
            var db = c["database"]?.ToString() ?? c["DatabaseName"]?.ToString();
            var user = c["username"]?.ToString() ?? c["Username"]?.ToString();
            var pass = c["password"]?.ToString() ?? c["Password"]?.ToString();
            int.TryParse((c["port"]?.ToString() ?? c["Port"]?.ToString()), out int port);

            try
            {
                System.Data.IDbConnection conn = type switch
                {
                    "mssql" => new Microsoft.Data.SqlClient.SqlConnection(
                        connectionString ?? $"Server={host},{port};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;"),
                    "postgresql" => new Npgsql.NpgsqlConnection(
                        connectionString ?? $"Host={host};Port={port};Database={db};Username={user};Password={pass};"),
                    "mysql" => new MySqlConnector.MySqlConnection(
                        connectionString ?? $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};"),
                    _ => null
                };

                if (conn != null)
                {
                    // Register by both ID and name so scripts can reference either
                    connectionInfo.interpreter.RegisterDatabaseConnection(id, conn);
                    if (!string.IsNullOrEmpty(name) && name != id)
                        connectionInfo.interpreter.RegisterDatabaseConnection(name, conn);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering interpreter connection {id}: {ex.Message}");
            }
        }
    }

    private void RegisterProjectFunctions(ConnectionInfo connectionInfo, string userId, string projectId)
    {
        // ExecuteScript(scriptName) — runs a saved SQL script by name on its stored connection
        connectionInfo.interpreter.RegisterExternalFunction("ExecuteScript", args =>
        {
            if (args.Length == 0) return new List<object>();
            string scriptName = args[0]?.ToString() ?? "";

            var scripts = DatabasePersistence.LoadScripts(userId, "sql", projectId);
            var script = scripts.FirstOrDefault(s =>
                string.Equals(s["name"]?.ToString() ?? s["Name"]?.ToString() ?? "",
                              scriptName, StringComparison.OrdinalIgnoreCase));

            if (script == null)
            {
                connectionInfo.interpreter.PrintCore($"ExecuteScript: script '{scriptName}' not found");
                return new List<object>();
            }

            string sqlCode = script["code"]?.ToString() ?? script["Code"]?.ToString() ?? "";
            string dbId = script["databaseconnectionid"]?.ToString()
                       ?? script["DatabaseConnectionId"]?.ToString()
                       ?? script["database"]?.ToString()
                       ?? script["Database"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(sqlCode))
            {
                connectionInfo.interpreter.PrintCore($"ExecuteScript: script '{scriptName}' has no code");
                return new List<object>();
            }
            if (string.IsNullOrEmpty(dbId))
            {
                connectionInfo.interpreter.PrintCore($"ExecuteScript: script '{scriptName}' has no database connection");
                return new List<object>();
            }

            return connectionInfo.interpreter.ExecuteDatabaseQuery(dbId, sqlCode);
        });

        // ReadDataset(name) — returns rows from a saved Dataset as List<Dictionary>
        connectionInfo.interpreter.RegisterExternalFunction("ReadDataset", args =>
        {
            if (args.Length == 0) return new List<object>();
            string name = args[0]?.ToString() ?? "";
            return LoadProjectDataRows(connectionInfo, userId, projectId, name, "ReadDataset");
        });

        // ReadSpreadsheet(name) — returns rows from a saved Excel spreadsheet as List<Dictionary>
        connectionInfo.interpreter.RegisterExternalFunction("ReadSpreadsheet", args =>
        {
            if (args.Length == 0) return new List<object>();
            string name = args[0]?.ToString() ?? "";
            return LoadProjectDataRows(connectionInfo, userId, projectId, name, "ReadSpreadsheet");
        });
    }

    private static List<object> LoadProjectDataRows(ConnectionInfo connectionInfo, string userId, string projectId, string name, string caller)
    {
        var entities = DatabasePersistence.LoadEntities(userId, "Datasets", projectId);
        var entity = entities.FirstOrDefault(e =>
            string.Equals(e["name"]?.ToString() ?? e["Name"]?.ToString() ?? "",
                          name, StringComparison.OrdinalIgnoreCase));

        if (entity == null)
        {
            connectionInfo.interpreter.PrintCore($"{caller}: '{name}' not found");
            return new List<object>();
        }

        string configStr = entity["config"]?.ToString() ?? entity["Config"]?.ToString() ?? "";
        if (string.IsNullOrEmpty(configStr)) return new List<object>();

        try
        {
            var config = Newtonsoft.Json.Linq.JObject.Parse(configStr);
            var dataArray = config["data"] as Newtonsoft.Json.Linq.JArray;
            if (dataArray == null) return new List<object>();

            return dataArray.Select(row =>
                (object)(row is Newtonsoft.Json.Linq.JObject rowObj
                    ? rowObj.ToObject<Dictionary<string, object>>()
                    : new Dictionary<string, object>())
            ).ToList();
        }
        catch (Exception ex)
        {
            connectionInfo.interpreter.PrintCore($"{caller}: error parsing data for '{name}': {ex.Message}");
            return new List<object>();
        }
    }

    private void CommandMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;
        if (connectionInfo == null) return;

        string uuid = !string.IsNullOrEmpty(connectionInfo.UserId) ? connectionInfo.UserId : connectionInfo.ConnectionId;
        // Console.WriteLine($"{uuid}: {e.Message.ToString()}");
        var cmdMessage = e.Message as CommandMessage;
        
        if (cmdMessage == null) return;

        var parameters = cmdMessage.Parameters;
        // Console.WriteLine( JsonConvert.SerializeObject(parameters));
        var response = new ResponseMessage
        {
            RequestId = cmdMessage.Id,
            Status = MessageStatus.Success,
            ErrorMessage = ""
        };

        try
        {
            switch (cmdMessage.Command)
            {
                case "ExecuteCs":
                    if (parameters.ContainsKey("code"))
                    {
                        string code = parameters["code"].ToString();
                        string execProjectId = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;

                        // Extract variable values passed by the client { varName: value }
                        var varDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        if (parameters.ContainsKey("variables") && parameters["variables"] is Newtonsoft.Json.Linq.JObject varsObj)
                        {
                            foreach (var kv in varsObj)
                                varDict[kv.Key] = kv.Value?.ToString() ?? "";
                        }

                        RegisterConnectionsToInterpreter(connectionInfo, uuid);
                        RegisterProjectFunctions(connectionInfo, uuid, execProjectId);

                        // GetVar('name') lets scripts read the current variable values
                        connectionInfo.interpreter.RegisterExternalFunction("GetVar", args =>
                        {
                            if (args.Length == 0) return "";
                            var name = args[0]?.ToString() ?? "";
                            return varDict.TryGetValue(name, out var v) ? v : "";
                        });

                        // Run on a background thread so long scripts don't block the WebSocket pump
                        Task.Run(() =>
                        {
                            try
                            {
                                connectionInfo.interpreter.Execute(code);

                                // Send completion notification
                                var done = new NotificationMessage
                                {
                                    Category = "ExecutionComplete",
                                    Content = "Script execution finished.",
                                    Title = "Done"
                                };
                                using var _1 = SendMessageAsync(connectionInfo, done, socket);
                            }
                            catch (Exception ex)
                            {
                                var errResp = new ResponseMessage
                                {
                                    RequestId = cmdMessage.Id,
                                    Status = MessageStatus.Error,
                                    ErrorMessage = ex.Message
                                };
                                using var _2 = SendMessageAsync(connectionInfo, errResp, socket);
                            }
                        });
                    }
                    response.Data = new { message = "Execution started" };
                    break;

                case "ExecuteSql":
                    if (parameters.ContainsKey("database") && parameters.ContainsKey("code"))
                    {
                        string dbId = parameters["database"].ToString();
                        string sql = parameters["code"].ToString();

                        try
                        {
                            var result = _dataSourceManager.ExecuteQueryAsync(dbId, sql).GetAwaiter().GetResult();

                            var rows = new List<Dictionary<string, object>>();
                            var columns = new List<object>();
                            bool colsExtracted = false;

                            foreach (var row in result)
                            {
                                // ReportsCache round-trips through System.Text.Json, turning
                                // Dapper DapperRow (IDictionary) into JsonElement on cache hit.
                                // Normalise both cases to a plain dictionary with CLR values.
                                var rowDict = RowToDictionary(row);

                                if (!colsExtracted)
                                {
                                    foreach (var key in rowDict.Keys)
                                        columns.Add(new { field = key, header = key });
                                    colsExtracted = true;
                                }
                                rows.Add(rowDict);
                            }

                            response.Data = new { rows, columns };
                        }
                        catch (Exception dbEx)
                        {
                            response.Status = MessageStatus.Error;
                            response.ErrorMessage = dbEx.Message;
                        }
                    }
                    else
                    {
                        response.Status = MessageStatus.Error;
                        response.ErrorMessage = "Missing database or code parameter";
                    }
                    break;

                case "SaveScript":
                    if (parameters.ContainsKey("script"))
                    {
                        var scriptObj = JObject.FromObject(parameters["script"]);
                        var lang = scriptObj["language"]?.ToString() ?? "sql";
                        DatabasePersistence.SaveScript(uuid, scriptObj, lang);
                    }
                    break;

                case "LoadScripts":
                {
                    string scriptLang = parameters.ContainsKey("language") ? parameters["language"].ToString() : "";
                    var pid = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;
                    response.Data = DatabasePersistence.LoadScripts(uuid, scriptLang, pid);
                    break;
                }

                case "DeleteScript":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var lang2 = parameters.ContainsKey("language") ? parameters["language"].ToString() : "sql";
                        DatabasePersistence.DeleteScript(uuid, id, lang2);
                    }
                    break;

                case "SaveDatabaseConnection":
                    if (parameters.ContainsKey("connection"))
                    {
                        var connObj = JObject.FromObject(parameters["connection"]);
                        DatabasePersistence.SaveDatabaseConnection(uuid, connObj);
                        RegisterUserDatabaseConnections(uuid);
                    }
                    break;

                case "LoadDatabaseConnections":
                {
                    var pid = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;
                    response.Data = DatabasePersistence.LoadDatabaseConnections(uuid, pid);
                    break;
                }

                case "DeleteDatabaseConnection":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteDatabaseConnection(uuid, id);
                    }
                    break;

                case "TestDatabaseConnection":
                    if (parameters.ContainsKey("connection"))
                    {
                        var connObj = JObject.FromObject(parameters["connection"]);
                        var testResult = TestConnection(connObj);
                        response.Data = testResult;
                    }
                    else
                    {
                        response.Data = new { success = false, message = "No connection parameters provided" };
                    }
                    break;

                case "SaveDataset":
                    if (parameters.ContainsKey("dataset"))
                    {
                        var dsObj = JObject.FromObject(parameters["dataset"]);
                        DatabasePersistence.SaveEntity(uuid, "Datasets", dsObj);
                    }
                    break;

                case "LoadDatasets":
                {
                    var pid = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;
                    response.Data = DatabasePersistence.LoadEntities(uuid, "Datasets", pid);
                    break;
                }

                case "DeleteDataset":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteEntity(uuid, "Datasets", id);
                    }
                    break;

                case "SaveExcel":
                    if (parameters.ContainsKey("excel"))
                    {
                        var exObj = JObject.FromObject(parameters["excel"]);
                        DatabasePersistence.SaveEntity(uuid, "Datasets", exObj);
                    }
                    break;

                case "LoadExcels":
                {
                    var pid = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;
                    response.Data = DatabasePersistence.LoadEntities(uuid, "Datasets", pid);
                    break;
                }

                case "DeleteExcel":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteEntity(uuid, "Datasets", id);
                    }
                    break;

                case "SaveDashboard":
                    if (parameters.ContainsKey("dashboard"))
                    {
                        var dashObj = JObject.FromObject(parameters["dashboard"]);
                        DatabasePersistence.SaveEntity(uuid, "Dashboards", dashObj);
                        response.Data = dashObj;
                    }
                    break;

                case "LoadDashboards":
                {
                    var pid = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;
                    response.Data = DatabasePersistence.LoadEntities(uuid, "Dashboards", pid);
                    break;
                }

                case "DeleteDashboard":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteEntity(uuid, "Dashboards", id);
                    }
                    break;

                case "ShareDashboard":
                {
                    var id = parameters.ContainsKey("id") ? parameters["id"].ToString() : null;
                    var enable = parameters.ContainsKey("enable") && Convert.ToBoolean(parameters["enable"]);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (enable)
                        {
                            var token = DatabasePersistence.GenerateShareToken(uuid, "Dashboards", id);
                            response.Data = new { shareToken = token };
                        }
                        else
                        {
                            DatabasePersistence.RevokeShareToken(uuid, "Dashboards", id);
                            response.Data = new { shareToken = (string)null, shareUrl = (string)null };
                        }
                    }
                    break;
                }

                case "LoadProjects":
                    response.Data = DatabasePersistence.LoadProjects(uuid);
                    break;

                case "SaveProject":
                    if (parameters.ContainsKey("project"))
                    {
                        var projObj = JObject.FromObject(parameters["project"]);
                        DatabasePersistence.SaveProject(uuid, projObj);
                        response.Data = new { id = projObj["id"]?.ToString() };
                    }
                    break;

                case "DeleteProject":
                    if (parameters.ContainsKey("id"))
                        DatabasePersistence.DeleteProject(uuid, parameters["id"].ToString());
                    break;

                case "LoadVariables":
                {
                    var pid = parameters.ContainsKey("projectId") ? parameters["projectId"]?.ToString() : null;
                    response.Data = DatabasePersistence.LoadVariables(uuid, pid);
                    break;
                }

                case "SaveVariable":
                    if (parameters.ContainsKey("variable"))
                    {
                        var varObj = JObject.FromObject(parameters["variable"]);
                        DatabasePersistence.SaveVariable(uuid, varObj);
                        response.Data = new { id = varObj["id"]?.ToString() };
                    }
                    break;

                case "DeleteVariable":
                    if (parameters.ContainsKey("id"))
                        DatabasePersistence.DeleteVariable(uuid, parameters["id"].ToString());
                    break;

                case "ResolveDropdownQuery":
                    if (parameters.ContainsKey("database") && parameters.ContainsKey("query"))
                    {
                        string dbId2 = parameters["database"].ToString();
                        string qry = parameters["query"].ToString();
                        try
                        {
                            var qResult = _dataSourceManager.ExecuteQueryAsync(dbId2, qry).GetAwaiter().GetResult();
                            var opts = new List<string>();
                            foreach (var row in qResult)
                            {
                                Dictionary<string, object> rd = RowToDictionary(row);
                                if (rd.Count > 0) opts.Add(rd.Values.First()?.ToString() ?? "");
                            }
                            response.Data = opts;
                        }
                        catch (Exception qEx)
                        {
                            response.Status = MessageStatus.Error;
                            response.ErrorMessage = qEx.Message;
                        }
                    }
                    break;

                case "LoadReports":
                    response.Data = DatabasePersistence.LoadReports(uuid);
                    break;

                case "SaveReport":
                    if (parameters.ContainsKey("report"))
                    {
                        var repObj = JObject.FromObject(parameters["report"]);
                        DatabasePersistence.SaveReport(uuid, repObj);
                        response.Data = new { id = repObj["id"]?.ToString() };
                    }
                    break;

                case "DeleteReport":
                    if (parameters.ContainsKey("id"))
                        DatabasePersistence.DeleteReport(uuid, parameters["id"].ToString());
                    break;

                case "ShareReport":
                {
                    var id = parameters.ContainsKey("id") ? parameters["id"].ToString() : null;
                    var enable = parameters.ContainsKey("enable") && Convert.ToBoolean(parameters["enable"]);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (enable)
                        {
                            var token = DatabasePersistence.GenerateShareToken(uuid, "Reports", id);
                            response.Data = new { shareToken = token };
                        }
                        else
                        {
                            DatabasePersistence.RevokeShareToken(uuid, "Reports", id);
                            response.Data = new { shareToken = (string)null };
                        }
                    }
                    break;
                }

                case "UpdateUserProfile":
                {
                    var displayName = parameters.ContainsKey("displayName") ? parameters["displayName"]?.ToString() : null;
                    if (!string.IsNullOrWhiteSpace(displayName))
                    {
                        var ok = _authService.UpdateUserProfileAsync(uuid, displayName).GetAwaiter().GetResult();
                        if (!ok) { response.Status = MessageStatus.Error; response.ErrorMessage = "Update failed"; }
                    }
                    break;
                }

                case "ChangePassword":
                {
                    var oldPw = parameters.ContainsKey("oldPassword") ? parameters["oldPassword"]?.ToString() : null;
                    var newPw = parameters.ContainsKey("newPassword") ? parameters["newPassword"]?.ToString() : null;
                    if (string.IsNullOrWhiteSpace(oldPw) || string.IsNullOrWhiteSpace(newPw))
                    {
                        response.Status = MessageStatus.Error;
                        response.ErrorMessage = "Both old and new passwords are required";
                    }
                    else
                    {
                        var ok = _authService.ChangePasswordAsync(uuid, oldPw, newPw).GetAwaiter().GetResult();
                        if (!ok) { response.Status = MessageStatus.Error; response.ErrorMessage = "Current password is incorrect"; }
                    }
                    break;
                }

                default:
                    response.Status = MessageStatus.Error;
                    response.ErrorMessage = $"Unknown command: {cmdMessage.Command}";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.Status = MessageStatus.Error;
            response.ErrorMessage = ex.Message;
        }

        using var _ = SendMessageAsync(connectionInfo, response, socket);
    }

    private object TestConnection(JObject connObj)
    {
        var type = (connObj["type"]?.ToString() ?? connObj["Type"]?.ToString() ?? "").ToLower();
        var host = connObj["host"]?.ToString() ?? connObj["Host"]?.ToString();
        var database = connObj["database"]?.ToString() ?? connObj["DatabaseName"]?.ToString();
        var user = connObj["username"]?.ToString() ?? connObj["Username"]?.ToString();
        var pass = connObj["password"]?.ToString() ?? connObj["Password"]?.ToString();
        var connectionString = connObj["connectionString"]?.ToString() ?? connObj["ConnectionString"]?.ToString();
        int.TryParse(connObj["port"]?.ToString() ?? connObj["Port"]?.ToString(), out int port);
        connectionString = connectionString == "" ? null : connectionString;
        try
        {
            System.Data.IDbConnection conn;
            var cs = connectionString ?? type switch
            {
                "mssql"       => $"Server={host},{port};Database={database};User Id={user};Password={pass};TrustServerCertificate=True;Connect Timeout=5;",
                "postgresql"  => $"Host={host};Port={port};Database={database};Username={user};Password={pass};Timeout=5;",
                "mysql"       => $"Server={host};Port={port};Database={database};Uid={user};Pwd={pass};Connect Timeout=5;",
                "oracle"      => $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={database})));User Id={user};Password={pass};",
                _             => throw new InvalidOperationException($"Unsupported database type: {type}")
            };

            try
            {
                conn = type switch
                {
                    "mssql"      => new Microsoft.Data.SqlClient.SqlConnection(cs),
                    "postgresql" => new Npgsql.NpgsqlConnection(cs),
                    "mysql"      => new MySqlConnector.MySqlConnection(cs),
                    "oracle"     => CreateOracleTestConnection(cs),
                    _            => throw new InvalidOperationException($"Unsupported database type: {type}")
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Oracle"))
            {
                return new { success = false, message = ex.Message };
            }

            using (conn)
            {
                conn.Open();
                conn.Close();
            }

            return new { success = true, message = "Connection successful" };
        }
        catch (Exception ex)
        {
            return new { success = false, message = ex.Message };
        }
    }

    private static Dictionary<string, object> RowToDictionary(dynamic row)
    {
        if (row is IDictionary<string, object> dapperRow)
            return new Dictionary<string, object>(dapperRow);

        if (row is System.Text.Json.JsonElement { ValueKind: System.Text.Json.JsonValueKind.Object } jsonObj)
        {
            var d = new Dictionary<string, object>();
            foreach (var prop in jsonObj.EnumerateObject())
                d[prop.Name] = JsonElementToClr(prop.Value);
            return d;
        }

        // Last-resort: round-trip through Newtonsoft to get a plain dict
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(row);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
               ?? new Dictionary<string, object>();
    }

    private static object JsonElementToClr(System.Text.Json.JsonElement el) => el.ValueKind switch
    {
        System.Text.Json.JsonValueKind.String  => el.GetString(),
        System.Text.Json.JsonValueKind.True    => (object)true,
        System.Text.Json.JsonValueKind.False   => false,
        System.Text.Json.JsonValueKind.Null    => null,
        System.Text.Json.JsonValueKind.Number  => el.TryGetInt64(out var l) ? (object)l : el.GetDouble(),
        _                                      => el.ToString()
    };

    private static System.Data.IDbConnection CreateOracleTestConnection(string cs)
    {
        try
        {
            var asm = System.Reflection.Assembly.Load("Oracle.ManagedDataAccess");
            var type = asm.GetType("Oracle.ManagedDataAccess.Client.OracleConnection", throwOnError: true)!;
            return (System.Data.IDbConnection)Activator.CreateInstance(type, cs)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Oracle client not available: " + ex.Message +
                ". Ensure Oracle.ManagedDataAccess.Core NuGet package is deployed.", ex);
        }
    }

    private void ErrorOccurred(object sender, Exception e)
    {
        Console.WriteLine($"WebSocket client error: {e.Message}");
    }

    private void AuthenticationMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;

        var authMsg = e.Message as AuthenticationMessage;
        string jwt = null;

        if (authMsg != null)
        {
            if (!string.IsNullOrEmpty(authMsg.Token))
            {
                var userId = _authService.GetUserIdFromToken(authMsg.Token);
                if (!string.IsNullOrEmpty(userId))
                {
                    connectionInfo.UserId = userId;
                    jwt = authMsg.Token;
                }
            }
            else if (!string.IsNullOrEmpty(authMsg.Username))
            {
                jwt = _authService.AuthenticateAsync(new LoginRequest { Username = authMsg.Username, Password = authMsg.Password })
                    .GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(jwt))
                    connectionInfo.UserId = _authService.GetUserIdFromToken(jwt);
            }

            if (!string.IsNullOrEmpty(connectionInfo.UserId))
                RegisterUserDatabaseConnections(connectionInfo.UserId);
        }

        dynamic data = new JObject();
        data.Uuid = connectionInfo.ConnectionId;
        data.Menu = new JObject();
        data.Menu.Header = "";
        data.Functions = new JArray(connectionInfo.interpreter.GetFunctions());
        if (jwt != null) data.Token = jwt;

        var response = new ResponseMessage
        {
            Status = jwt != null ? MessageStatus.Success : MessageStatus.Error,
            ErrorMessage = jwt != null ? "" : "Authentication failed",
            Data = data
        };

        using var _ = SendMessageAsync(connectionInfo, response, socket);
    }

    public async Task ProcessIncomingMessageAsync(IWebsocketConnection socket, string messageJson)
    {
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;
        if (connectionInfo == null) return;

        try
        {
            connectionInfo.WebSocketMessageClient.ReceiveMsg(messageJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing incoming message: {ex.Message}");
            var errorMessage = new ErrorMessage
            {
                ErrorCode = "500",
                ErrorDescription = ex.Message,
                ErrorDetails = ex.StackTrace
            };
            await SendMessageAsync(connectionInfo, errorMessage, socket);
        }
    }

    private async Task SendMessageAsync(ConnectionInfo connectionInfo, BaseMessage message, IWebsocketConnection socket)
    {
        string msg = JsonConvert.SerializeObject(message);
        await socket.Send(msg);
    }

    public int GetConnectionCount() => _connections.Count;

    public IEnumerable<string> GetConnectedUserIds() =>
        _connections.Values.Where(c => !string.IsNullOrEmpty(c.UserId)).Select(c => c.UserId).Distinct();
}
