using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using System.Runtime.CompilerServices;

namespace Server.Core;

public class ReportsController
{
    private readonly IUserReportsService _reportsService;
    private readonly ReportsCache _cache;
    private readonly IAuthService _authService;
    public ReportsController(IUserReportsService reportsService, ReportsCache cache, IAuthService authService)
    {
        _reportsService = reportsService;
        _cache = cache;
        _authService = authService;
    }

    public async Task<IRequest> ValidateHandeAsync(IRequest request)
    {
        
        if (!request.Headers.TryGetValue("Authorization", out var authHeader) || !authHeader.StartsWith("Bearer "))
        {
            
            throw new ProviderException(ResponseStatus.Forbidden, "Access denied");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        
        if (!await _authService.ValidateTokenAsync(token))
        {
            throw new ProviderException(ResponseStatus.Forbidden, "Access denied");
        }

        
        var userId = _authService.GetUserIdFromToken(token);
        request.Properties["UserId"] = userId;

        // Llamada asíncrona final al siguiente manejador
        return request;
    }

    [ResourceMethod("type/:reportid")]
    public async ValueTask<IEnumerable<UserReport>> myReports(IRequest request, string reportid)
    {
        if (reportid == "my")
        {
            var request2 =await ValidateHandeAsync(request);
            var userId = GetUserId(request2);
            return await _reportsService.GetUserReportsAsync(userId);
        }
        else if (reportid == "share")
        {
            var request2 = await ValidateHandeAsync(request);
            var userId = GetUserId(request2);
            return await _reportsService.GetSharedReportsAsync(userId);
        }
        else
        {
            return await _reportsService.GetPublicReportsAsync();
        } 
    }

    
    [ResourceMethod(RequestMethod.Post,"newreport")]
    public async ValueTask<UserReport> NewReport(IRequest request, [FromBody] UserReport report)
    {

        var request2 = await ValidateHandeAsync(request);
        var userId = GetUserId(request2);
        return await _reportsService.CreateReportAsync(userId, report);
    }
    
    [ResourceMethod(RequestMethod.Put,":id")]
    public async ValueTask<UserReport> UpdateReport(IRequest request, string id, [FromBody] UserReport report)
    {
        var request2 = await ValidateHandeAsync(request);
        var userId = GetUserId(request2);
        report.ReportId = id;
        
        try
        {
            return await _reportsService.UpdateReportAsync(userId, report);
        }
        catch (UnauthorizedAccessException)
        {
            throw new ProviderException(ResponseStatus.Forbidden, "Access denied");
        }
    }
    
    [ResourceMethod(RequestMethod.Delete, ":id")]
    public async ValueTask<object> DeleteReport(IRequest request, string id)
    {
        var request2 = await ValidateHandeAsync(request);
        var userId = GetUserId(request2);
        var success = await _reportsService.DeleteReportAsync(userId, id);
        
        if (!success)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found");
        
        return new { message = "Report deleted successfully" };
    }
    
    [ResourceMethod(":id")]
    public async ValueTask<UserReport> GetReport(IRequest request, string id)
    {
        var request2 = await ValidateHandeAsync(request);
        var userId = GetUserId(request2);

        if (!await _reportsService.CanAccessReportAsync(userId, id))
            throw new ProviderException(ResponseStatus.Forbidden, "Access denied");
        
        var report = await _reportsService.GetReportAsync(userId, id);
        if (report == null)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found");
        
        return report;
    }
    
    [ResourceMethod(RequestMethod.Post, ":id/execute")]
    public async ValueTask<ReportExecutionResult> ExecuteReport(IRequest request, string id)
    {
        var request2 = await ValidateHandeAsync(request);
        var userId = GetUserId(request2);
        var forceRefresh = request.Query["forceRefresh"] == "true";
        
        try
        {
            var result = await _reportsService.ExecuteReportAsync(userId, id, forceRefresh);
            return new ReportExecutionResult 
            { 
                ReportId = id, 
                Data = result, 
                ExecutedAt = DateTime.UtcNow 
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw new ProviderException(ResponseStatus.Forbidden, "Access denied");
        }
    }
    
    [ResourceMethod("metrics")]
    public object GetMetrics()
    {
        var metrics = _cache.GetMetrics();
        return new
        {
            cacheHits = metrics.CacheHits,
            cacheMisses = metrics.CacheMisses,
            hitRate = $"{metrics.HitRate:F2}%",
            averageQueryTime = $"{metrics.AverageQueryTime.TotalMilliseconds:F2}ms"
        };
    }
    
    private string GetUserId(IRequest request)
    {
        // Intentar obtener del Properties primero
        try
        {
            return request.Properties["UserId"].ToString();
            
        }
        catch
        {
        }

        // Fallback a Headers si Properties no está disponible
        if (request.Headers.TryGetValue("X-User-Id", out var userId))
        {
            return userId;
        }
        
        // Último fallback: extraer directamente del token
        if (request.Headers.TryGetValue("Authorization", out var authHeader) && 
            authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            return new AuthService(null).GetUserIdFromToken(token);
        }
        
        return null;
    }
}