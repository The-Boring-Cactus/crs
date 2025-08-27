namespace Server.Core;

public interface IUserReportsService
{
    Task<UserReport> CreateReportAsync(string userId, UserReport report);
    Task<UserReport> UpdateReportAsync(string userId, UserReport report);
    Task<bool> DeleteReportAsync(string userId, string reportId);
    Task<IEnumerable<UserReport>> GetUserReportsAsync(string userId);
    Task<IEnumerable<UserReport>> GetPublicReportsAsync();
    Task<IEnumerable<UserReport>> GetSharedReportsAsync(string userId);
    Task<UserReport> GetReportAsync(string userId, string reportId);
    Task<bool> CanAccessReportAsync(string userId, string reportId);
    Task<object> ExecuteReportAsync(string userId, string reportId, bool forceRefresh = false);
    Task<object> ExecuteReportWithProgressAsync(string userId, string reportId, bool forceRefresh, 
        IProgress<QueryExecutionStatus> progress, CancellationToken cancellationToken);
}