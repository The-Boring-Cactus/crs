using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.OpenApi;
using Server.Core;
using System.Net;
using GenHTTP.Modules.ApiBrowsing;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cache = new ReportsCache("report-system");
        DatabasePersistence.RunMigrations();
        var dataSource = new DataSourceManager(cache);
        var authService = new AuthService(cache);
        var reportsService = new UserReportsService(cache, dataSource);
        var backgroundWorker = new ReportsBackgroundWorker(cache, reportsService);

        var webSocketManager = new WebSocketManager(authService, reportsService, dataSource);

        var setupController = new SetupController();
        var authController = new AuthController(authService);
        var reportsController = new ReportsController(reportsService, cache, authService);
        var publicController = new PublicController();

        var websocketHandler = Websocket.Create()
            .OnOpen(async (socket) => await webSocketManager.HandleWebSocketAsync(socket))
            .OnMessage(async (socket, message) => await webSocketManager.ProcessIncomingMessageAsync(socket, message));

        // Resolve Resources relative to the executable so the server works regardless
        // of the working directory (critical in the published/deployed layout).
        var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        if (!Directory.Exists(resourcesPath))
            resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

        // StaticWebsite automatically serves index files and provides sitemap/robots support.
        // For SPA deep-link routing (direct browser navigation to /pages/...) in production,
        // place nginx or Caddy in front and rewrite unmatched paths to index.html.
        var spa = StaticWebsite.From(ResourceTree.FromDirectory(resourcesPath));

        ushort port = ushort.TryParse(Environment.GetEnvironmentVariable("CRS_PORT"), out var p) ? p : (ushort)9876;
        bool isDev = Environment.GetEnvironmentVariable("CRS_ENV") != "production";

        var layout = Layout.Create()
            .Add(CorsPolicy.Permissive())
            .Add("/srv", websocketHandler)
            .Add("/api/reports", ServiceResource.From(reportsController).Build())
            .Add("/api/auth", ServiceResource.From(authController).Build())
            .Add("/api/setup", ServiceResource.From(setupController).Build())
            .Add("/api/public", ServiceResource.From(publicController).Build())
            .Add("/", spa)
            .AddOpenApi()
            .AddSwaggerUI()
            .AddRedoc()
            .AddScalar();

        var builder = Host.Create()
            .Handler(layout)
            .Defaults();

        if (isDev) builder = builder.Development();

        _ = await builder.Console().Bind(IPAddress.Any, port).RunAsync();

        backgroundWorker.Dispose();
        cache.Dispose();
    }
}
