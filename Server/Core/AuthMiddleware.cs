#nullable enable
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using System.Linq;
namespace Server.Core;

public class AuthMiddleware : IConcern
{
    private readonly IAuthService _authService;

    public IHandler Parent { get; }

    public IHandler Content => Parent;

    public AuthMiddleware(IHandler parent, IAuthService authService)
    {
        Parent = parent;
        _authService = authService;
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse> HandleAsync(IRequest request)
    {
        var publicPaths = new[] { "/api/auth/login", "/api/auth/register", "/api/reports/public", "/" };
        var requestPath = request.Target.Path.ToString();

        if (publicPaths.Any(path => requestPath.Equals(path, StringComparison.OrdinalIgnoreCase) || (path == "/" && path != requestPath)))
        {
            // Llamada asíncrona al siguiente manejador
            return await Parent.HandleAsync(request);
        }

        if (!request.Headers.TryGetValue("Authorization", out var authHeader) || !authHeader.StartsWith("Bearer "))
        {
            // La creación de la respuesta es síncrona, se puede retornar directamente.
            return request.Respond()
                .Status(ResponseStatus.Unauthorized)
                .Content("{\"message\":\"Authorization header required\"}")
                .Type(new FlexibleContentType(ContentType.ApplicationJson))
                .Build();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Uso de 'await' para la validación asíncrona del token
        if (!await _authService.ValidateTokenAsync(token))
        {
            return request.Respond()
                .Status(ResponseStatus.Unauthorized)
                .Content("{\"message\":\"Invalid or expired token\"}")
                .Type(new FlexibleContentType(ContentType.ApplicationJson))
                .Build();
        }

        // Uso de 'await' para obtener el ID del usuario
        var userId =  _authService.GetUserIdFromToken(token);
        request.Properties["UserId"] = userId;

        // Llamada asíncrona final al siguiente manejador
        return await Parent.HandleAsync(request);
    }
}