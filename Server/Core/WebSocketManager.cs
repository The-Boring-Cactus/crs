using System.Text.Json;
using System.Text;
using GenHTTP.Api.Protocol;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace Server.Core;

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections;
    private readonly IAuthService _authService;
    private readonly IUserReportsService _reportsService;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public WebSocketManager(IAuthService authService, IUserReportsService reportsService)
    {
        _connections = new ConcurrentDictionary<string, ConnectionInfo>();
        _authService = authService;
        _reportsService = reportsService;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };
    }
    
    public async Task HandleWebSocketAsync(IRequest request, WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        var cancellationTokenSource = new CancellationTokenSource();
        
        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = connectionId,
            ConnectedAt = DateTime.UtcNow,
            WebSocket = webSocket,
            CancellationTokenSource = cancellationTokenSource
        };
        
        _connections.TryAdd(connectionId, connectionInfo);
        
        try
        {
            await SendMessageAsync(connectionInfo, new WebSocketMessage
            {
                Type = "connection_established",
                Data = new { connectionId, connectedAt = DateTime.UtcNow },
                RequestId = "system"
            });
            
            await ProcessWebSocketMessagesAsync(connectionInfo, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error for connection {connectionId}: {ex.Message}");
        }
        finally
        {
            _connections.TryRemove(connectionId, out _);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }
    }
    
    private async Task ProcessWebSocketMessagesAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var webSocket = connectionInfo.WebSocket;
        
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                    break;
                }
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessIncomingMessageAsync(connectionInfo, messageJson, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing WebSocket message: {ex.Message}");
                await SendErrorMessageAsync(connectionInfo, "Error processing message", "system");
            }
        }
    }
    
    private async Task ProcessIncomingMessageAsync(ConnectionInfo connectionInfo, string messageJson, CancellationToken cancellationToken)
    {
        try
        {
            var message = JsonSerializer.Deserialize<WebSocketMessage>(messageJson, _jsonOptions);
            
            switch (message.Type.ToLower())
            {
                case "authenticate":
                    await HandleAuthenticationAsync(connectionInfo, message);
                    break;
                    
                case "execute_report":
                    await HandleReportExecutionAsync(connectionInfo, message, cancellationToken);
                    break;
                    
                case "cancel_execution":
                    await HandleCancelExecutionAsync(connectionInfo, message);
                    break;
                    
                case "ping":
                    await SendMessageAsync(connectionInfo, new WebSocketMessage
                    {
                        Type = "pong",
                        Data = new { timestamp = DateTime.UtcNow },
                        RequestId = message.RequestId
                    });
                    break;
                    
                default:
                    await SendErrorMessageAsync(connectionInfo, $"Unknown message type: {message.Type}", message.RequestId);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing incoming message: {ex.Message}");
            await SendErrorMessageAsync(connectionInfo, "Invalid message format", "unknown");
        }
    }
    
    private async Task HandleAuthenticationAsync(ConnectionInfo connectionInfo, WebSocketMessage message)
    {
        try
        {
            var authData = JsonSerializer.Deserialize<JsonElement>(message.Data.ToString());
            var token = authData.GetProperty("token").GetString();
            
            if (await _authService.ValidateTokenAsync(token))
            {
                var userId = _authService.GetUserIdFromToken(token);
                connectionInfo.UserId = userId;
                
                await SendMessageAsync(connectionInfo, new WebSocketMessage
                {
                    Type = "authentication_success",
                    Data = new { userId, authenticatedAt = DateTime.UtcNow },
                    RequestId = message.RequestId
                });
            }
            else
            {
                await SendErrorMessageAsync(connectionInfo, "Invalid token", message.RequestId);
            }
        }
        catch (Exception ex)
        {
            await SendErrorMessageAsync(connectionInfo, $"Authentication error: {ex.Message}", message.RequestId);
        }
    }
    
    private async Task HandleReportExecutionAsync(ConnectionInfo connectionInfo, WebSocketMessage message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(connectionInfo.UserId))
        {
            await SendErrorMessageAsync(connectionInfo, "Not authenticated", message.RequestId);
            return;
        }
        
        try
        {
            var executionData = JsonSerializer.Deserialize<QueryExecutionRequest>(message.Data.ToString(), _jsonOptions);
            
            // Crear progress reporter que envía updates via WebSocket
            var progress = new Progress<QueryExecutionStatus>(status =>
            {
                status.RequestId = executionData.RequestId;
                _ = SendMessageAsync(connectionInfo, new WebSocketMessage
                {
                    Type = "execution_progress",
                    Data = status,
                    RequestId = executionData.RequestId
                });
            });
            
            // Ejecutar reporte de forma asíncrona
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await _reportsService.ExecuteReportWithProgressAsync(
                        connectionInfo.UserId,
                        executionData.ReportId,
                        executionData.ForceRefresh,
                        progress,
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    await SendErrorMessageAsync(connectionInfo, $"Execution failed: {ex.Message}", executionData.RequestId);
                }
            }, cancellationToken);
            
            await SendMessageAsync(connectionInfo, new WebSocketMessage
            {
                Type = "execution_started",
                Data = new { requestId = executionData.RequestId, reportId = executionData.ReportId },
                RequestId = executionData.RequestId
            });
        }
        catch (Exception ex)
        {
            await SendErrorMessageAsync(connectionInfo, $"Error starting execution: {ex.Message}", message.RequestId);
        }
    }
    
    private async Task HandleCancelExecutionAsync(ConnectionInfo connectionInfo, WebSocketMessage message)
    {
        try
        {
            connectionInfo.CancellationTokenSource.Cancel();
            
            await SendMessageAsync(connectionInfo, new WebSocketMessage
            {
                Type = "execution_cancelled",
                Data = new { cancelledAt = DateTime.UtcNow },
                RequestId = message.RequestId
            });
        }
        catch (Exception ex)
        {
            await SendErrorMessageAsync(connectionInfo, $"Error cancelling execution: {ex.Message}", message.RequestId);
        }
    }
    
    private async Task SendMessageAsync(ConnectionInfo connectionInfo, WebSocketMessage message)
    {
        if (connectionInfo.WebSocket.State != WebSocketState.Open)
            return;
        
        try
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            await connectionInfo.WebSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending WebSocket message: {ex.Message}");
        }
    }
    
    private async Task SendErrorMessageAsync(ConnectionInfo connectionInfo, string errorMessage, string requestId)
    {
        await SendMessageAsync(connectionInfo, new WebSocketMessage
        {
            Type = "error",
            Data = new { error = errorMessage, timestamp = DateTime.UtcNow },
            RequestId = requestId
        });
    }
    
    public int GetConnectionCount() => _connections.Count;
    
    public IEnumerable<string> GetConnectedUserIds() => 
        _connections.Values.Where(c => !string.IsNullOrEmpty(c.UserId)).Select(c => c.UserId).Distinct();
}