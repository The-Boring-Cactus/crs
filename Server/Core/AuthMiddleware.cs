#nullable enable
using System.Collections;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace Server.Core;

public class AuthMiddleware : IConcern
{
    private readonly IAuthService _authService;
    
    public AuthMiddleware(IAuthService authService)
    {
        _authService = authService;
    }
    
    public ValueTask PrepareAsync(IRequest request, IHandler handler)
    {
        // Preparación opcional - no necesaria para este caso
        return ValueTask.CompletedTask;
    }
    
    public async ValueTask<IResponse?> HandleAsync(IRequest request, Func<IRequest, ValueTask<IResponse?>> next)
    {
        // Rutas públicas que no requieren autenticación
        var publicPaths = new[] { "/api/auth/login", "/api/auth/register", "/api/reports/public", "/" };
        var requestPath = request.Target.Path.ToString();
        
        if (publicPaths.Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
        {
            return await next(request);
        }
        
        // Verificar token de autorización
        if (!request.Headers.TryGetValue("Authorization", out var authHeader) || 
            string.IsNullOrEmpty(authHeader) || 
            !authHeader.StartsWith("Bearer "))
        {
            return request.Respond()
                         .Status(ResponseStatus.Unauthorized)
                         .Content("{\"message\":\"Authorization header required\"}")
                         .Type(ContentType.ApplicationJson)
                         .Build();
        }
        
        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (!await _authService.ValidateTokenAsync(token))
        {
            return request.Respond()
                         .Status(ResponseStatus.Unauthorized)
                         .Content("{\"message\":\"Invalid or expired token\"}")
                         .Type(ContentType.ApplicationJson)
                         .Build();
        }
        
        // Agregar userId al request para uso posterior
        var userId = _authService.GetUserIdFromToken(token);
        if (request.Properties != null)
        {
            request.Properties["UserId"] = userId;
        }

        if (!((IDictionary)request.Headers).IsReadOnly)
        {
            ((IDictionary)request.Headers).Add("X-User-Id", userId);
        }
        
        return await next(request);
    }

    public ValueTask PrepareAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask<IResponse> HandleAsync(IRequest request)
    {
        var publicPaths = new[] { "/api/auth/login", "/api/auth/register", "/api/reports/public", "/" };
        var requestPath = request.Target.Path.ToString();

        if (publicPaths.Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
        {
            return request.Respond().Status(ResponseStatus.Ok).Build(); 
        }

        // Verificar token de autorización
        if (!request.Headers.TryGetValue("Authorization", out var authHeader) ||
            string.IsNullOrEmpty(authHeader) ||
            !authHeader.StartsWith("Bearer "))
        {
            return request.Respond()
                         .Status(ResponseStatus.Unauthorized)
                         .Content("{\"message\":\"Authorization header required\"}")
                         .Type(ContentType.ApplicationJson)
                         .Build();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (!await _authService.ValidateTokenAsync(token))
        {
            return request.Respond()
                         .Status(ResponseStatus.Unauthorized)
                         .Content("{\"message\":\"Invalid or expired token\"}")
                         .Type(ContentType.ApplicationJson)
                         .Build();
        }

        // Agregar userId al request para uso posterior
        var userId = _authService.GetUserIdFromToken(token);
        if (request.Properties != null)
        {
            request.Properties["UserId"] = userId;
        }

        if (!((IDictionary)request.Headers).IsReadOnly)
        {
            ((IDictionary)request.Headers).Add("X-User-Id", userId);
        }

        return request.Respond().Status(ResponseStatus.Ok).Build();
    }

    public IHandler Content { get; }
}