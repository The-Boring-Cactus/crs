//using System.Text;
//using System.Text.Json;
//using GenHTTP.Modules.Websockets;
//using Server.Core;

//namespace Server;

//public class WebSocketManager
//{
//    public static async Task HandleWebSocketMessage(Core.WebSocketManager manager, IWebsocketConnection socket, string messageText)
//    {
//        try
//        {
//            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
//            var message = JsonSerializer.Deserialize<WebSocketMessage>(messageText, jsonOptions);
            
//            switch (message.Type.ToLower())
//            {
//                case "authenticate":
//                    await HandleAuthentication(manager, socket, message);
//                    break;
                    
//                case "execute_report":
//                    await HandleReportExecution(manager, socket, message);
//                    break;
                    
//                case "ping":
//                    await SendWebSocketMessage(socket, new WebSocketMessage
//                    {
//                        Type = "pong",
//                        Data = new { timestamp = DateTime.UtcNow },
//                        RequestId = message.RequestId
//                    });
//                    break;
                    
//                default:
//                    await SendWebSocketError(socket, $"Unknown message type: {message.Type}", message.RequestId);
//                    break;
//            }
//        }
//        catch (Exception ex)
//        {
//            await SendWebSocketError(socket, $"Error processing message: {ex.Message}", "unknown");
//        }
//    }
    
//    public static async Task HandleAuthentication(Core.WebSocketManager manager, IWebsocketConnection socket, WebSocketMessage message)
//    {
//        try
//        {
//            var authData = JsonSerializer.Deserialize<JsonElement>(message.Data.ToString());
//            var token = authData.GetProperty("token").GetString();
            
//            // Aquí necesitarías acceso al AuthService - por simplicidad, asumimos token válido
//            // En producción, validarías el token apropiadamente
            
//            await SendWebSocketMessage(socket, new WebSocketMessage
//            {
//                Type = "authentication_success",
//                Data = new { authenticated = true, timestamp = DateTime.UtcNow },
//                RequestId = message.RequestId
//            });
//        }
//        catch (Exception ex)
//        {
//            await SendWebSocketError(socket, $"Authentication failed: {ex.Message}", message.RequestId);
//        }
//    }
    
//    public static async Task HandleReportExecution(Core.WebSocketManager manager, IWebsocketConnection socket, WebSocketMessage message)
//    {
//        try
//        {
//            await SendWebSocketMessage(socket, new WebSocketMessage
//            {
//                Type = "execution_started",
//                Data = new { status = "started", message = "Execution started" },
//                RequestId = message.RequestId
//            });
            
//            // Simular progreso
//            for (int i = 0; i <= 100; i += 20)
//            {
//                await Task.Delay(500);
//                await SendWebSocketMessage(socket, new WebSocketMessage
//                {
//                    Type = "execution_progress",
//                    Data = new QueryExecutionStatus
//                    {
//                        RequestId = message.RequestId,
//                        Status = "progress",
//                        ProgressPercentage = i,
//                        Message = $"Processing... {i}%"
//                    },
//                    RequestId = message.RequestId
//                });
//            }
            
//            // Completado
//            await SendWebSocketMessage(socket, new WebSocketMessage
//            {
//                Type = "execution_progress",
//                Data = new QueryExecutionStatus
//                {
//                    RequestId = message.RequestId,
//                    Status = "completed",
//                    ProgressPercentage = 100,
//                    Message = "Execution completed",
//                    Data = new { result = "Sample data" }
//                },
//                RequestId = message.RequestId
//            });
//        }
//        catch (Exception ex)
//        {
//            await SendWebSocketError(socket, $"Execution failed: {ex.Message}", message.RequestId);
//        }
//    }
    
//    public static async Task SendWebSocketMessage(IWebsocketConnection socket, WebSocketMessage message)
//    {
//        try
//        {
//            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
//            var json = JsonSerializer.Serialize(message, jsonOptions);
            
//            await socket.Send(json);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error sending WebSocket message: {ex.Message}");
//        }
//    }
    
//    public static async Task SendWebSocketError(IWebsocketConnection socket, string errorMessage, string requestId)
//    {
//        await SendWebSocketMessage(socket, new WebSocketMessage
//        {
//            Type = "error",
//            Data = new { error = errorMessage, timestamp = DateTime.UtcNow },
//            RequestId = requestId
//        });
//    }
//}