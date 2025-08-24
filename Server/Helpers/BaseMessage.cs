#nullable enable
namespace Server.Helpers;

public class BaseMessage
{
    public string? Id { get; set; } 
    public MessageType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? SenderId { get; set; }
    public string? ReceiverId { get; set; }
}