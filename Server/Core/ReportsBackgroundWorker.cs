namespace Server.Core;

public class ReportsBackgroundWorker
{
    private readonly ReportsCache _cache;
    private readonly IUserReportsService _reportsService;
    private readonly Timer _timer;
    
    public ReportsBackgroundWorker(ReportsCache cache, IUserReportsService reportsService)
    {
        _cache = cache;
        _reportsService = reportsService;

        // Delay warmup start until after setup wizard (database tables exist)
        // Start timer after 30 seconds to let server initialize, then run every 15 minutes
        _timer = new Timer(async _ => await WarmupReports(), null,
            TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(15));
    }
    
    private async Task WarmupReports()
    {
        try
        {
            // En producción, obtener usuarios activos
            // Por ahora warming up reportes públicos
            var publicReports = await _reportsService.GetPublicReportsAsync();

            foreach (var report in publicReports.Where(r => r.IsActive))
            {
                try
                {
                    await _reportsService.ExecuteReportAsync("system", report.ReportId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error warming up report {report.ReportId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // Silently skip if database isn't configured yet (table doesn't exist)
            // Only log if it's a different type of error
            if (!ex.Message.Contains("does not exist") && !ex.Message.Contains("relation"))
            {
                Console.WriteLine($"Error in warmup process: {ex.Message}");
            }
        }
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
    }
}