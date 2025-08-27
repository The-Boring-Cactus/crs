namespace Server.Core;

public class QueryExecutionRequest
{
    public string RequestId { get; set; }
    public string ReportId { get; set; }
    public bool ForceRefresh { get; set; }
    public string Token { get; set; } // Para autenticación via WS
}
