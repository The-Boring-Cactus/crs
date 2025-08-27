using System.Net;
using System.Text;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Websockets;
using Server;
using Server.Core;

var cache = new ReportsCache("./reports-cache");
var dataSource = new DataSourceManager(cache);
var authService = new AuthService(cache);
var reportsService = new UserReportsService(cache, dataSource);
var backgroundWorker = new ReportsBackgroundWorker(cache, reportsService);
var webSocketManager = new WebSocketManager(authService, reportsService);

var authController = new AuthController(authService);
var reportsController = new ReportsController(reportsService, cache);

var websocketHandler = Websocket.Create()
    .OnOpen((socket) =>
    {
        // WebSocket abierto
        return ValueTask.CompletedTask;
    })
    .OnMessage(async (socket, message) =>
    {
        // Manejar mensajes recibidos
        try
        {
            var messageText = message;
            await HandleWebSocketMessage(webSocketManager, socket, messageText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling WebSocket message: {ex.Message}");
        }
    })
    .OnClose((socket) =>
    {
        // WebSocket cerrado
        return ValueTask.CompletedTask;
    });

var server = Host.Create()
    .Handler(
        Layout.Create()
            .Add(CorsPolicy.Permissive())
            .Add(new AuthMiddleware(authService))
            .Add("ws", websocketHandler) // WebSocket endpoint
            .Add("api/auth", 
                Layout.Create()
                    .AddService<AuthController>("auth")
            )
            .Add("api/reports", Webservice.From(reportsController))
            .Port(8080)
            .Bind(IPAddress.Any);
        
Console.WriteLine("GenHTTP Reports Server starting on http://localhost:8080");
        
// Ejecutar servidor
await server.RunAsync();
        
// Cleanup
backgroundWorker.Dispose();
cache.Dispose();