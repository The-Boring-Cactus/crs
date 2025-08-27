namespace Server.Core;

public class WebSocketMessage
{
    public string Type { get; set; }
    public object Data { get; set; }
    public string RequestId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}