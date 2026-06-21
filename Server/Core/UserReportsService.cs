using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server.Core;

public class UserReportsService : IUserReportsService
{
    private readonly ReportsCache _cache;
    private readonly DataSourceManager _dataSource;

    public UserReportsService(ReportsCache cache, DataSourceManager dataSource)
    {
        _cache = cache;
        _dataSource = dataSource;
    }

    public async Task<UserReport> CreateReportAsync(string userId, UserReport report)
    {
        report.ReportId = report.ReportId ?? Guid.NewGuid().ToString();
        report.UserId = userId;
        report.CreatedAt = DateTime.UtcNow;
        report.IsActive = true;

        var obj = ToJObject(report);
        DatabasePersistence.SaveReport(userId, obj);
        _cache.InvalidateQuery($"user-reports:{userId}");

        await Task.CompletedTask;
        return report;
    }

    public async Task<UserReport> UpdateReportAsync(string userId, UserReport report)
    {
        var existing = await GetReportAsync(userId, report.ReportId);
        if (existing == null || existing.UserId != userId)
            throw new UnauthorizedAccessException("Report not found or access denied");

        report.UserId = userId;
        var obj = ToJObject(report);
        DatabasePersistence.SaveReport(userId, obj);

        _cache.InvalidateQuery($"user-reports:{userId}");
        _cache.InvalidateQuery($"report-data:{report.ReportId}");

        await Task.CompletedTask;
        return report;
    }

    public async Task<bool> DeleteReportAsync(string userId, string reportId)
    {
        var existing = await GetReportAsync(userId, reportId);
        if (existing == null || existing.UserId != userId)
            return false;

        DatabasePersistence.DeleteReport(userId, reportId);
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
            return DatabasePersistence.LoadReports(userId)
                .Select(r => FromJObject(r))
                .Where(r => r != null && r.IsActive);
        }, TimeSpan.FromMinutes(5));
    }

    public async Task<IEnumerable<UserReport>> GetPublicReportsAsync()
    {
        return await _cache.GetOrExecuteAsync("public-reports", async () =>
        {
            await Task.CompletedTask;
            using var conn = DatabasePersistence.CreateConnection();
            if (conn == null) return Enumerable.Empty<UserReport>();
            conn.Open();
            var rows = Dapper.SqlMapper.Query(conn, "SELECT * FROM Reports WHERE IsPublic = @True", new { True = true }).ToList();
            return rows.Select(r => FromJObject(JObject.Parse(JsonConvert.SerializeObject(r))))
                       .Where(r => r != null && r.IsActive)
                       .Cast<UserReport>();
        }, TimeSpan.FromMinutes(10));
    }

    public async Task<IEnumerable<UserReport>> GetSharedReportsAsync(string userId)
    {
        return await _cache.GetOrExecuteAsync($"shared-reports:{userId}", async () =>
        {
            await Task.CompletedTask;
            // Load all public reports; shared-with filtering is handled via Config JSON
            var publicReports = await GetPublicReportsAsync();
            return publicReports.Where(r => r.SharedWithUsers.Contains(userId));
        }, TimeSpan.FromMinutes(5));
    }

    public async Task<UserReport> GetReportAsync(string userId, string reportId)
    {
        await Task.CompletedTask;
        var all = DatabasePersistence.LoadReports(userId);
        var row = all.FirstOrDefault(r =>
            (r["id"]?.ToString() ?? r["Id"]?.ToString() ?? r["reportid"]?.ToString() ?? r["ReportId"]?.ToString()) == reportId);
        return row != null ? FromJObject(row) : null;
    }

    public async Task<bool> CanAccessReportAsync(string userId, string reportId)
    {
        await Task.CompletedTask;
        var report = await GetReportAsync(userId, reportId);
        if (report == null || !report.IsActive) return false;
        if (report.UserId == userId) return true;
        if (report.IsPublic) return true;
        return report.SharedWithUsers.Contains(userId);
    }

    public async Task<object> ExecuteReportAsync(string userId, string reportId, bool forceRefresh = false)
    {
        if (!await CanAccessReportAsync(userId, reportId))
            throw new UnauthorizedAccessException("Access denied to this report");

        var report = await GetReportAsync(userId, reportId);
        var cacheKey = $"report-data:{reportId}";

        if (forceRefresh)
            _cache.InvalidateQuery(cacheKey);

        return await _cache.GetOrExecuteAsync(cacheKey, async () =>
            await _dataSource.ExecuteQueryAsync(report.ConnectionKey, report.SqlQuery, report.Parameters, report.CacheAge),
            report.CacheAge);
    }

    public async Task<object> ExecuteReportWithProgressAsync(string userId, string reportId, bool forceRefresh,
        IProgress<QueryExecutionStatus> progress, CancellationToken cancellationToken)
    {
        progress?.Report(new QueryExecutionStatus { RequestId = reportId, Status = "started", ProgressPercentage = 0 });

        if (!await CanAccessReportAsync(userId, reportId))
            throw new UnauthorizedAccessException("Access denied to this report");

        var report = await GetReportAsync(userId, reportId);
        var cacheKey = $"report-data:{reportId}";

        if (forceRefresh)
            _cache.InvalidateQuery(cacheKey);

        progress?.Report(new QueryExecutionStatus { RequestId = reportId, Status = "progress", ProgressPercentage = 30 });

        var result = await _cache.GetOrExecuteAsync(cacheKey, async () =>
            await _dataSource.ExecuteQueryWithProgressAsync(
                report.ConnectionKey, report.SqlQuery, report.Parameters, report.CacheAge, progress, cancellationToken),
            report.CacheAge);

        progress?.Report(new QueryExecutionStatus { RequestId = reportId, Status = "completed", ProgressPercentage = 100, Data = result });
        return result;
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static JObject ToJObject(UserReport report)
    {
        var obj = JObject.FromObject(report);
        obj["id"] = report.ReportId;
        obj["name"] = report.Name ?? "Untitled";
        return obj;
    }

    private static UserReport FromJObject(JObject row)
    {
        try
        {
            var configJson = row["config"]?.ToString() ?? row["Config"]?.ToString();
            if (!string.IsNullOrEmpty(configJson))
                return JsonConvert.DeserializeObject<UserReport>(configJson);

            // Fallback: map columns directly
            return new UserReport
            {
                ReportId = row["id"]?.ToString() ?? row["Id"]?.ToString(),
                UserId = row["userid"]?.ToString() ?? row["UserId"]?.ToString(),
                Name = row["name"]?.ToString() ?? row["Name"]?.ToString(),
                IsActive = true
            };
        }
        catch { return null; }
    }
}
