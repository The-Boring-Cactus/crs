namespace Server.Core;

public class ReportExecutionResult
{
    public string ReportId { get; set; }
    public object Data { get; set; }
    public DateTime ExecutedAt { get; set; }
}
