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
        
        _timer = new Timer(async _ => await WarmupReports(), null, 
            TimeSpan.Zero, TimeSpan.FromMinutes(15));
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
            Console.WriteLine($"Error in warmup process: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
    }
}