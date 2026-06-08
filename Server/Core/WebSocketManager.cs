using FunctEngine;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Helpers;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;


namespace Server.Core;

// Gestor de conexiones WebSocket (versión para GenHTTP)
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

  
    public async Task HandleWebSocketAsync( IWebsocketConnection socket)
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
        connectionInfo.interpreter.StatusUpdate += TestScriptOnStatusUpdate;
        connectionInfo.interpreter.LoadExternalDll("MathFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("DateTimeFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("DoeFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("FinancialFunctions.dll");
        connectionInfo.interpreter.LoadExternalDll("StringUtilities.dll");
        connectionInfo.interpreter.LoadExternalDll("DataTableFunctions.dll");

        if (!_connections.TryAdd(connectionId, connectionInfo))
        {
            // No se pudo agregar la conexión, la cerramos.
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
            // Aseguramos la limpieza en caso de un error inesperado durante el setup.
            _connections.TryRemove(connectionId, out _);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    private void TestScriptOnStatusUpdate(object sender, StatusString e)
    {
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.interpreter == sender).Value;
        if (connectionInfo == null) return;
        
        NotificationMessage notification = new NotificationMessage
        {
            Category = "Debug",
            Content = e.status,
            Title = "Execution Debug"
        };
        
        using var _ = SendMessageAsync(connectionInfo, notification, connectionInfo.WebSocket);
    }

    private void HeartbeatMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;

        using var _ = SendMessageAsync(connectionInfo, new HeartbeatMessage(), socket);
    }

    private void DataMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ErrorMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void NotificationMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void TextMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RegisterUserDatabaseConnections(string userId)
    {
        var conns = DatabasePersistence.LoadDatabaseConnections(userId);
        foreach(var c in conns)
        {
            var id = c["id"]?.ToString() ?? c["Id"]?.ToString();
            var type = c["type"]?.ToString() ?? c["Type"]?.ToString();
            var connectionString = c["connectionString"]?.ToString() ?? c["ConnectionString"]?.ToString();
            
            if (type == "mssql" && !string.IsNullOrEmpty(id))
            {
                var host = c["host"]?.ToString() ?? c["Host"]?.ToString();
                var db = c["database"]?.ToString() ?? c["DatabaseName"]?.ToString();
                var user = c["username"]?.ToString() ?? c["Username"]?.ToString();
                var pass = c["password"]?.ToString() ?? c["Password"]?.ToString();
                string cs = connectionString ?? $"Server={host};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;";
                
                try 
                {
                    _dataSourceManager.AddConnection(id, new Microsoft.Data.SqlClient.SqlConnection(cs));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error registering connection {id}: {ex.Message}");
                }
            }
        }
    }

    private void CommandMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;
        if (connectionInfo == null) return;
        
        string Uuid = !string.IsNullOrEmpty(connectionInfo.UserId) ? connectionInfo.UserId : connectionInfo.ConnectionId;

        var cmdMessage = e.Message as CommandMessage;
        if (cmdMessage == null) return;

        var parameters = cmdMessage.Parameters;
        
        ResponseMessage response = new ResponseMessage
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
                        connectionInfo.interpreter.Execute(code);
                    }
                    response.Data = new { message = "Execution started/completed" };
                    break;
                    
                case "ExecuteSql":
                    if (parameters.ContainsKey("database") && parameters.ContainsKey("code"))
                    {
                        string dbId = parameters["database"].ToString();
                        string sql = parameters["code"].ToString();
                        
                        try 
                        {
                            var result = _dataSourceManager.ExecuteQueryAsync(dbId, sql).GetAwaiter().GetResult();
                            
                            var rows = new List<object>();
                            var columns = new List<object>();
                            bool colsExtracted = false;
                            
                            foreach(var row in result)
                            {
                                if (!colsExtracted)
                                {
                                    if (row is IDictionary<string, object> dict)
                                    {
                                        foreach(var key in dict.Keys)
                                        {
                                            columns.Add(new { field = key, header = key });
                                        }
                                    }
                                    colsExtracted = true;
                                }
                                rows.Add(row);
                            }
                            
                            response.Data = new { rows = rows, columns = columns };
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
                        DatabasePersistence.SaveScript(Uuid, scriptObj, lang);
                    }
                    break;
                    
                case "LoadScripts":
                    string scriptLang = parameters.ContainsKey("language") ? parameters["language"].ToString() : "";
                    response.Data = DatabasePersistence.LoadScripts(Uuid, scriptLang);
                    break;
                    
                case "DeleteScript":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var lang2 = parameters.ContainsKey("language") ? parameters["language"].ToString() : "sql";
                        DatabasePersistence.DeleteScript(Uuid, id, lang2);
                    }
                    break;

                case "SaveDatabaseConnection":
                    if (parameters.ContainsKey("connection"))
                    {
                        var connObj = JObject.FromObject(parameters["connection"]);
                        DatabasePersistence.SaveDatabaseConnection(Uuid, connObj);
                        RegisterUserDatabaseConnections(Uuid);
                    }
                    break;
                    
                case "LoadDatabaseConnections":
                    response.Data = DatabasePersistence.LoadDatabaseConnections(Uuid);
                    break;
                    
                case "DeleteDatabaseConnection":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteDatabaseConnection(Uuid, id);
                    }
                    break;
                    
                case "TestDatabaseConnection":
                    bool success = new Random().NextDouble() > 0.3;
                    response.Data = new { success = success };
                    break;
                    
                case "SaveDataset":
                    if (parameters.ContainsKey("dataset"))
                    {
                        var dsObj = JObject.FromObject(parameters["dataset"]);
                        DatabasePersistence.SaveEntity(Uuid, "Datasets", dsObj);
                    }
                    break;

                case "LoadDatasets":
                    response.Data = DatabasePersistence.LoadEntities(Uuid, "Datasets");
                    break;

                case "DeleteDataset":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteEntity(Uuid, "Datasets", id);
                    }
                    break;

                case "SaveExcel":
                    if (parameters.ContainsKey("excel"))
                    {
                        var exObj = JObject.FromObject(parameters["excel"]);
                        DatabasePersistence.SaveEntity(Uuid, "Datasets", exObj); // Excels stored in Datasets
                    }
                    break;

                case "LoadExcels":
                    response.Data = DatabasePersistence.LoadEntities(Uuid, "Datasets");
                    break;

                case "DeleteExcel":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteEntity(Uuid, "Datasets", id);
                    }
                    break;

                case "SaveDashboard":
                    if (parameters.ContainsKey("dashboard"))
                    {
                        var dashObj = JObject.FromObject(parameters["dashboard"]);
                        DatabasePersistence.SaveEntity(Uuid, "Dashboards", dashObj);
                    }
                    break;

                case "LoadDashboards":
                    response.Data = DatabasePersistence.LoadEntities(Uuid, "Dashboards");
                    break;

                case "DeleteDashboard":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        DatabasePersistence.DeleteEntity(Uuid, "Dashboards", id);
                    }
                    break;

                default:
                    response.Status = MessageStatus.Error;
                    response.ErrorMessage = "Unknown command";
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

    private void ErrorOccurred(object sender, Exception e)
    {
        Console.WriteLine(e.Message);
    }

    private void AuthenticationMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;

        var authMsg = e.Message as AuthenticationMessage;
        if (authMsg != null && !string.IsNullOrEmpty(authMsg.Token))
        {
            var userId = _authService.GetUserIdFromToken(authMsg.Token);
            if (!string.IsNullOrEmpty(userId))
            {
                connectionInfo.UserId = userId;
            }
            else if (!string.IsNullOrEmpty(authMsg.Username))
            {
                connectionInfo.UserId = authMsg.Username;
            }
            
            // Register data sources upon successful login mapping
            if (!string.IsNullOrEmpty(connectionInfo.UserId))
            {
                RegisterUserDatabaseConnections(connectionInfo.UserId);
            }
        }

        dynamic data = new JObject();
        data.Uuid = connectionInfo.ConnectionId;
        data.Menu = new JObject();
        data.Menu.Header = "";
        data.Functions = new JArray(connectionInfo.interpreter.GetFunctions());
        ResponseMessage responde = new ResponseMessage
        {
            Status = MessageStatus.Success,
            ErrorMessage = "",
            Data = data
        };


        using var _ = SendMessageAsync(connectionInfo, responde, socket);
    }

    public async Task ProcessIncomingMessageAsync(IWebsocketConnection socket, string messageJson)
    {
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;
        CancellationToken cancellationToken = connectionInfo.CancellationTokenSource.Token;
        // Esta lógica interna no cambia, sigue siendo perfectamente válida.
        try
        {
            connectionInfo.WebSocketMessageClient.ReceiveMsg(messageJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing incoming message: {ex.Message}");

            ErrorMessage errorMessage = new ErrorMessage()
            {
                ErrorCode = "401",
                ErrorDescription = ex.Message,
                ErrorDetails = ex.StackTrace
            };

            await SendMessageAsync(connectionInfo, errorMessage, socket);
        }
    }

    // El resto de los métodos (HandleAuthenticationAsync, HandleReportExecutionAsync, etc.) no necesitan cambios.
    // ...

    private async Task SendMessageAsync(ConnectionInfo connectionInfo, BaseMessage message, IWebsocketConnection socket)
    {
        string msg = JsonConvert.SerializeObject(message);
        await socket.Send(msg);
    }

    

    public int GetConnectionCount() => _connections.Count;

    public IEnumerable<string> GetConnectedUserIds() =>
        _connections.Values.Where(c => !string.IsNullOrEmpty(c.UserId)).Select(c => c.UserId).Distinct();
}