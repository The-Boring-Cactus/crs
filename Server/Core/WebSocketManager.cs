using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets;
using System.Collections.Concurrent;
using System.Net.WebSockets;
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
            WebSocketMessageClient = new WebSocketMessageClient(socket)
        };

        if (!_connections.TryAdd(connectionId, connectionInfo))
        {
            // No se pudo agregar la conexión, la cerramos.
            socket.Close();
            return;
        }

        try
        {
            await SendMessageAsync(connectionInfo, new WebSocketMessage
            {
                Type = "connection_established",
                Data = new { connectionId, connectedAt = DateTime.UtcNow },
                RequestId = "system"
            });

          
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



    public async Task ProcessIncomingMessageAsync(IWebsocketConnection socket, string messageJson)
    {
        ConnectionInfo connectionInfo = _connections.FirstOrDefault(s => s.Value.WebSocket == socket).Value;
        CancellationToken cancellationToken = connectionInfo.CancellationTokenSource.Token;
        // Esta lógica interna no cambia, sigue siendo perfectamente válida.
        try
        {
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing incoming message: {ex.Message}");
            await SendErrorMessageAsync(connectionInfo, socket);
        }
    }

    // El resto de los métodos (HandleAuthenticationAsync, HandleReportExecutionAsync, etc.) no necesitan cambios.
    // ...

    private async Task SendMessageAsync(ConnectionInfo connectionInfo, WebSocketMessage message)
    {
        
    }

    //private async Task HandleAuthenticationAsync(ConnectionInfo connectionInfo, WebSocketMessage message)
    //{
    //    try
    //    {
    //        var authData = JsonSerializer.Deserialize<JsonElement>(message.Data.ToString());
    //        var token = authData.GetProperty("token").GetString();

    //        if (await _authService.ValidateTokenAsync(token))
    //        {
    //            var userId = _authService.GetUserIdFromToken(token);
    //            connectionInfo.UserId = userId;

    //            await SendMessageAsync(connectionInfo, new WebSocketMessage
    //            {
    //                Type = "authentication_success",
    //                Data = new { userId, authenticatedAt = DateTime.UtcNow },
    //                RequestId = message.RequestId
    //            });
    //        }
    //        else
    //        {
    //            await SendErrorMessageAsync(connectionInfo, "Invalid token", message.RequestId);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await SendErrorMessageAsync(connectionInfo, $"Authentication error: {ex.Message}", message.RequestId);
    //    }
    //}

    //private async Task HandleReportExecutionAsync(ConnectionInfo connectionInfo, WebSocketMessage message, CancellationToken cancellationToken)
    //{
    //    if (string.IsNullOrEmpty(connectionInfo.UserId))
    //    {
    //        await SendErrorMessageAsync(connectionInfo, "Not authenticated", message.RequestId);
    //        return;
    //    }

    //    try
    //    {
    //        var executionData = JsonSerializer.Deserialize<QueryExecutionRequest>(message.Data.ToString(), _jsonOptions);

    //        // Crear progress reporter que envía updates via WebSocket
    //        var progress = new Progress<QueryExecutionStatus>(status =>
    //        {
    //            status.RequestId = executionData.RequestId;
    //            _ = SendMessageAsync(connectionInfo, new WebSocketMessage
    //            {
    //                Type = "execution_progress",
    //                Data = status,
    //                RequestId = executionData.RequestId
    //            });
    //        });

    //        // Ejecutar reporte de forma asíncrona
    //        _ = Task.Run(async () =>
    //        {
    //            try
    //            {
    //                var result = await _reportsService.ExecuteReportWithProgressAsync(
    //                    connectionInfo.UserId,
    //                    executionData.ReportId,
    //                    executionData.ForceRefresh,
    //                    progress,
    //                    cancellationToken
    //                );
    //            }
    //            catch (Exception ex)
    //            {
    //                await SendErrorMessageAsync(connectionInfo, $"Execution failed: {ex.Message}", executionData.RequestId);
    //            }
    //        }, cancellationToken);

    //        await SendMessageAsync(connectionInfo, new WebSocketMessage
    //        {
    //            Type = "execution_started",
    //            Data = new { requestId = executionData.RequestId, reportId = executionData.ReportId },
    //            RequestId = executionData.RequestId
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        await SendErrorMessageAsync(connectionInfo, $"Error starting execution: {ex.Message}", message.RequestId);
    //    }
    //}

    //private async Task HandleCancelExecutionAsync(ConnectionInfo connectionInfo, WebSocketMessage message)
    //{
    //    try
    //    {
    //        connectionInfo.CancellationTokenSource.Cancel();

    //        await SendMessageAsync(connectionInfo, new WebSocketMessage
    //        {
    //            Type = "execution_cancelled",
    //            Data = new { cancelledAt = DateTime.UtcNow },
    //            RequestId = message.RequestId
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        await SendErrorMessageAsync(connectionInfo, $"Error cancelling execution: {ex.Message}", message.RequestId);
    //    }
    //}
    //private async Task SendErrorMessageAsync(ConnectionInfo connectionInfo, string errorMessage, string requestId)
    //{
    //    await SendMessageAsync(connectionInfo, new WebSocketMessage
    //    {
    //        Type = "error",
    //        Data = new { error = errorMessage, timestamp = DateTime.UtcNow },
    //        RequestId = requestId
    //    });
    //}

    public int GetConnectionCount() => _connections.Count;

    public IEnumerable<string> GetConnectedUserIds() =>
        _connections.Values.Where(c => !string.IsNullOrEmpty(c.UserId)).Select(c => c.UserId).Distinct();
}