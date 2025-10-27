using FunctEngine;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Helpers;
using Server.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;


namespace Server.Core;

// Gestor de conexiones WebSocket (versión para GenHTTP)
public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections;
    private readonly IAuthService _authService;
    private readonly IUserReportsService _reportsService;
    private readonly IWorkspaceService _workspaceService;

    public WebSocketManager(IAuthService authService, IUserReportsService reportsService, IWorkspaceService workspaceService)
    {
        _connections = new ConcurrentDictionary<string, ConnectionInfo>();
        _authService = authService;
        _reportsService = reportsService;
        _workspaceService = workspaceService;
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
        dynamic answer = new JObject();
        answer.TypeMsg = "Debug";
        answer.data = e.status; ;
        var sz = answer.ToString();
        
    }

    private void HeartbeatMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;

        using var _ = SendMessageAsync(connectionInfo, new HeartbeatMessage(), socket);
    }

    private async void DataMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;

        if (e.Message is DataMessage dataMsg)
        {
            try
            {
                var response = dataMsg.DataType.ToLower() switch
                {
                    "save_workspace" => await HandleSaveWorkspace(dataMsg, connectionInfo),
                    "load_workspace" => await HandleLoadWorkspace(dataMsg, connectionInfo),
                    "list_workspaces" => await HandleListWorkspaces(dataMsg, connectionInfo),
                    "delete_workspace" => await HandleDeleteWorkspace(dataMsg, connectionInfo),
                    _ => CreateErrorResponse($"Unknown data type: {dataMsg.DataType}")
                };

                await SendMessageAsync(connectionInfo, response, socket);
            }
            catch (Exception ex)
            {
                await SendMessageAsync(connectionInfo, CreateErrorResponse(ex.Message), socket);
            }
        }
    }

    private async Task<ResponseMessage> HandleSaveWorkspace(DataMessage dataMsg, ConnectionInfo connectionInfo)
    {
        var workspaceJson = dataMsg.Payload.ToString();
        var workspace = System.Text.Json.JsonSerializer.Deserialize<WorkspaceData>(workspaceJson);

        if (workspace == null)
        {
            return CreateErrorResponse("Invalid workspace data");
        }

        workspace.UserId = connectionInfo.UserId ?? connectionInfo.ConnectionId;
        var savedWorkspace = await _workspaceService.SaveWorkspaceAsync(workspace);

        return new ResponseMessage
        {
            Status = MessageStatus.Success,
            Data = savedWorkspace
        };
    }

    private async Task<ResponseMessage> HandleLoadWorkspace(DataMessage dataMsg, ConnectionInfo connectionInfo)
    {
        var workspaceId = dataMsg.Metadata.GetValueOrDefault("workspaceId", "").ToString();
        var userId = connectionInfo.UserId ?? connectionInfo.ConnectionId;

        var workspace = await _workspaceService.GetWorkspaceAsync(workspaceId, userId);

        if (workspace == null)
        {
            return CreateErrorResponse("Workspace not found");
        }

        return new ResponseMessage
        {
            Status = MessageStatus.Success,
            Data = workspace
        };
    }

    private async Task<ResponseMessage> HandleListWorkspaces(DataMessage dataMsg, ConnectionInfo connectionInfo)
    {
        var workspaceType = dataMsg.Metadata.GetValueOrDefault("workspaceType", null)?.ToString();
        var userId = connectionInfo.UserId ?? connectionInfo.ConnectionId;

        var workspaces = await _workspaceService.ListWorkspacesAsync(userId, workspaceType);

        return new ResponseMessage
        {
            Status = MessageStatus.Success,
            Data = workspaces
        };
    }

    private async Task<ResponseMessage> HandleDeleteWorkspace(DataMessage dataMsg, ConnectionInfo connectionInfo)
    {
        var workspaceId = dataMsg.Metadata.GetValueOrDefault("workspaceId", "").ToString();
        var userId = connectionInfo.UserId ?? connectionInfo.ConnectionId;

        var success = await _workspaceService.DeleteWorkspaceAsync(workspaceId, userId);

        return new ResponseMessage
        {
            Status = success ? MessageStatus.Success : MessageStatus.Error,
            Data = new { success, workspaceId }
        };
    }

    private ResponseMessage CreateErrorResponse(string errorMessage)
    {
        return new ResponseMessage
        {
            Status = MessageStatus.Error,
            ErrorMessage = errorMessage
        };
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

    private void CommandMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ErrorOccurred(object sender, Exception e)
    {
        Console.WriteLine(e.Message);
    }

    private void AuthenticationMessage(object sender, MessageReceivedEventArgs e)
    {
        IWebsocketConnection socket = e.WebSocket;
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;

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