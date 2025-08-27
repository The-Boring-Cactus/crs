namespace Server.Core;

public class QueryExecutionStatus
{
    public string RequestId { get; set; }
    public string Status { get; set; } // "started", "progress", "completed", "error"
    public string Message { get; set; }
    public int ProgressPercentage { get; set; }
    public object Data { get; set; }
    public TimeSpan? ExecutionTime { get; set; }
}