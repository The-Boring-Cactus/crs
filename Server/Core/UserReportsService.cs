
namespace Server.Core;

public class UserReportsService : IUserReportsService
{
    private readonly ReportsCache _cache;
    private readonly DataSourceManager _dataSource;
    private readonly Dictionary<string, UserReport> _reports; // En producción usar DB
    
    public UserReportsService(ReportsCache cache, DataSourceManager dataSource)
    {
        _cache = cache;
        _dataSource = dataSource;
        _reports = new Dictionary<string, UserReport>();
    }
    
    public async Task<UserReport> CreateReportAsync(string userId, UserReport report)
    {
        report.ReportId = Guid.NewGuid().ToString();
        report.UserId = userId;
        report.CreatedAt = DateTime.UtcNow;
        
        _reports[report.ReportId] = report;
        _cache.InvalidateQuery($"user-reports:{userId}");
        
        await Task.CompletedTask;
        return report;
    }
    
    public async Task<UserReport> UpdateReportAsync(string userId, UserReport report)
    {
        if (!_reports.ContainsKey(report.ReportId) || _reports[report.ReportId].UserId != userId)
            throw new UnauthorizedAccessException("Report not found or access denied");
        
        report.UserId = userId;
        _reports[report.ReportId] = report;
        
        _cache.InvalidateQuery($"user-reports:{userId}");
        _cache.InvalidateQuery($"report-data:{report.ReportId}");
        
        await Task.CompletedTask;
        return report;
    }
    
    public async Task<bool> DeleteReportAsync(string userId, string reportId)
    {
        if (!_reports.ContainsKey(reportId) || _reports[reportId].UserId != userId)
            return false;
        
        _reports[reportId].IsActive = false;
        _cache.InvalidateQuery($"user-reports:{userId}");
        _cache.InvalidateQuery($"report-data:{reportId}");
        
        await Task.CompletedTask;
        return true;
    }
    
    public async Task<IEnumerable<UserReport>> GetUserReportsAsync(string userId)
    {
        return await _cache.GetOrExecuteAsync($"user-reports:{userId}", async () =>
        {
            await Task.CompletedTask;
            return _reports.Values.Where(r => r.UserId == userId && r.IsActive);
        }, TimeSpan.FromMinutes(5));
    }
    
    public async Task<IEnumerable<UserReport>> GetPublicReportsAsync()
    {
        return await _cache.GetOrExecuteAsync("public-reports", async () =>
        {
            await Task.CompletedTask;
            return _reports.Values.Where(r => r.IsPublic && r.IsActive);
        }, TimeSpan.FromMinutes(10));
    }
    
    public async Task<IEnumerable<UserReport>> GetSharedReportsAsync(string userId)
    {
        return await _cache.GetOrExecuteAsync($"shared-reports:{userId}", async () =>
        {
            await Task.CompletedTask;
            return _reports.Values.Where(r => r.SharedWithUsers.Contains(userId) && r.IsActive);
        }, TimeSpan.FromMinutes(5));
    }
    
    public async Task<UserReport> GetReportAsync(string userId, string reportId)
    {
        await Task.CompletedTask;
        return _reports.ContainsKey(reportId) ? _reports[reportId] : null;
    }
    
    public async Task<bool> CanAccessReportAsync(string userId, string reportId)
    {
        await Task.CompletedTask;
        
        if (!_reports.ContainsKey(reportId) || !_reports[reportId].IsActive)
            return false;
        
        var report = _reports[reportId];
        
        if (report.UserId == userId) return true;
        if (report.IsPublic) return true;
        return report.SharedWithUsers.Contains(userId);
    }
    
    public async Task<object> ExecuteReportAsync(string userId, string reportId, bool forceRefresh = false)
    {
        if (!await CanAccessReportAsync(userId, reportId))
            throw new UnauthorizedAccessException("Access denied to this report");
        
        var report = _reports[reportId];
        var cacheKey = $"report-data:{reportId}";
        
        if (forceRefresh)
        {
            _cache.InvalidateQuery(cacheKey);
        }
        
        var result = await _cache.GetOrExecuteAsync(cacheKey, async () =>
        {
            return await _dataSource.ExecuteQueryAsync(
                report.ConnectionKey,
                report.SqlQuery,
                report.Parameters,
                report.CacheAge
            );
        }, report.CacheAge);
        
        report.LastExecuted = DateTime.UtcNow;
        return result;
    }
    
    public async Task<object> ExecuteReportWithProgressAsync(string userId, string reportId, bool forceRefresh, 
        IProgress<QueryExecutionStatus> progress, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "started",
                Message = "Iniciando ejecución del reporte...",
                ProgressPercentage = 0
            });
            
            if (!await CanAccessReportAsync(userId, reportId))
                throw new UnauthorizedAccessException("Access denied to this report");
            
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "progress",
                Message = "Verificando permisos...",
                ProgressPercentage = 10
            });
            
            var report = _reports[reportId];
            var cacheKey = $"report-data:{reportId}";
            
            if (forceRefresh)
            {
                _cache.InvalidateQuery(cacheKey);
                progress?.Report(new QueryExecutionStatus
                {
                    RequestId = reportId,
                    Status = "progress",
                    Message = "Cache invalidado, ejecutando query fresco...",
                    ProgressPercentage = 20
                });
            }
            
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "progress",
                Message = "Ejecutando consulta en la base de datos...",
                ProgressPercentage = 30
            });
            
            var result = await _cache.GetOrExecuteAsync(cacheKey, async () =>
            {
                progress?.Report(new QueryExecutionStatus
                {
                    RequestId = reportId,
                    Status = "progress",
                    Message = "Procesando resultados...",
                    ProgressPercentage = 70
                });
                
                // Simular progreso para queries muy largas
                await Task.Delay(100, cancellationToken);
                
                return await _dataSource.ExecuteQueryWithProgressAsync(
                    report.ConnectionKey,
                    report.SqlQuery,
                    report.Parameters,
                    report.CacheAge,
                    progress,
                    cancellationToken
                );
            }, report.CacheAge);
            
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "progress",
                Message = "Finalizando...",
                ProgressPercentage = 90
            });
            
            report.LastExecuted = DateTime.UtcNow;
            
            var executionTime = DateTime.UtcNow - startTime;
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "completed",
                Message = "Reporte ejecutado exitosamente",
                ProgressPercentage = 100,
                Data = result,
                ExecutionTime = executionTime
            });
            
            return result;
        }
        catch (OperationCanceledException)
        {
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "error",
                Message = "Ejecución cancelada por el usuario",
                ProgressPercentage = -1
            });
            throw;
        }
        catch (Exception ex)
        {
            progress?.Report(new QueryExecutionStatus
            {
                RequestId = reportId,
                Status = "error",
                Message = $"Error ejecutando reporte: {ex.Message}",
                ProgressPercentage = -1
            });
            throw;
        }
    }
}