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

    public WebSocketManager(IAuthService authService, IUserReportsService reportsService)
    {
        _connections = new ConcurrentDictionary<string, ConnectionInfo>();
        _authService = authService;
        _reportsService = reportsService;
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