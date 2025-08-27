using System.Runtime.CompilerServices;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace Server.Core;

public class ReportsController
{
    private readonly IUserReportsService _reportsService;
    private readonly ReportsCache _cache;
    
    public ReportsController(IUserReportsService reportsService, ReportsCache cache)
    {
        _reportsService = reportsService;
        _cache = cache;
    }
    
    [ResourceMethod("my-reports")]
    public async ValueTask<IEnumerable<UserReport>> GetMyReports(IRequest request)
    {
        var userId = GetUserId(request);
        return await _reportsService.GetUserReportsAsync(userId);
    }
    
    [ResourceMethod("public")]
    public async ValueTask<IEnumerable<UserReport>> GetPublicReports()
    {
        return await _reportsService.GetPublicReportsAsync();
    }
    
    [ResourceMethod("shared")]
    public async ValueTask<IEnumerable<UserReport>> GetSharedReports(IRequest request)
    {
        var userId = GetUserId(request);
        return await _reportsService.GetSharedReportsAsync(userId);
    }
    
    [ResourceMethod(RequestMethod.Post)]
    public async ValueTask<UserReport> CreateReport(IRequest request, [FromBody] UserReport report)
    {
        var userId = GetUserId(request);
        return await _reportsService.CreateReportAsync(userId, report);
    }
    
    [ResourceMethod(RequestMethod.Put, ":id")]
    public async ValueTask<UserReport> UpdateReport(IRequest request, string id, [FromBody] UserReport report)
    {
        var userId = GetUserId(request);
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
        var userId = GetUserId(request);
        var success = await _reportsService.DeleteReportAsync(userId, id);
        
        if (!success)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found");
        
        return new { message = "Report deleted successfully" };
    }
    
    [ResourceMethod(":id")]
    public async ValueTask<UserReport> GetReport(IRequest request, string id)
    {
        var userId = GetUserId(request);
        
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
        var userId = GetUserId(request);
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