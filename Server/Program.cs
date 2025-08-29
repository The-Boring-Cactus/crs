using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Websockets;

using Microsoft.Data.SqlClient;
using Server.Core;
using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cache = new ReportsCache("./reports-cache");
        var dataSource = new DataSourceManager(cache);
        var authService = new AuthService(cache);
        var reportsService = new UserReportsService(cache, dataSource);
        var backgroundWorker = new ReportsBackgroundWorker(cache, reportsService);
        var webSocketManager = new WebSocketManager(authService, reportsService); // ¡Ahora sí lo usaremos!

        // Configurar conexiones de base de datos
        dataSource.AddConnection("mssql-main",
            new SqlConnection("Server=.;Database=Reports;Integrated Security=true"));

        // Configurar servicios web
        var authController = new AuthController(authService);
        var reportsController = new ReportsController(reportsService, cache);


        var websocketHandler = Websocket.Create()
                   .OnOpen(async (socket) =>
                   {

                       await webSocketManager.HandleWebSocketAsync(socket);
                   }).OnMessage(async (socket, message) =>
                       {
                           await webSocketManager.ProcessIncomingMessageAsync(socket, message);
                       }
                    );
        var files = Resources.From(ResourceTree.FromDirectory("Resources"));

        var server = Host.Create()
                   .Handler(
                       Layout.Create()
                           .Add(CorsPolicy.Permissive())
                           .Add(new AuthMiddleware(authService))
                           .Add("ws", websocketHandler)
                           .Add("api/auth", ServiceResource.From(authController))
                           .Add("api/reports", ServiceResource.From(reportsController))
                           .Add("/", files))
                           .Bind(IPAddress.Any, 8080);

        await server.RunAsync();

        // Cleanup
        backgroundWorker.Dispose();
        cache.Dispose();
    }
}