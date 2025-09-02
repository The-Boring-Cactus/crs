using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.OpenApi;

using Microsoft.Data.SqlClient;
using Server.Core;
using System.Net;
using GenHTTP.Modules.ApiBrowsing;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cache = new ReportsCache("report-system");
        var dataSource = new DataSourceManager(cache);
        var authService = new AuthService(cache);
        var reportsService = new UserReportsService(cache, dataSource);
        var backgroundWorker = new ReportsBackgroundWorker(cache, reportsService);
        var webSocketManager = new WebSocketManager(authService, reportsService); 

        // Configurar conexiones de base de datos
        dataSource.AddConnection("mssql-main",
            new SqlConnection("Server=.;Database=Reports;Integrated Security=true"));

        // Configurar servicios web
        var authController = new AuthController(authService);
        var reportsController = new ReportsController(reportsService, cache);

        var protectedApi = Layout.Create()
            .Add("api/reports", ServiceResource.From(reportsController));


        var protectedSection = new AuthMiddleware(protectedApi.Build(), authService);

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
                           .Add("/srv", websocketHandler)
                           .Add("/api/auth", ServiceResource.From(authController).Build())
                           .Add( protectedSection)
                           .Add("/", files)
                           .AddOpenApi()
                           .AddSwaggerUI()
                           .AddRedoc()
                           )
                   .Defaults()
                   .Development()
                   .Console()
                           .Bind(IPAddress.Any, 8080);

        await server.RunAsync();

        // Cleanup
        backgroundWorker.Dispose();
        cache.Dispose();
    }
}