using Server.Helpers;

namespace Server;

public class HeartbeatMessage : BaseMessage
{
    public long ServerTimestamp { get; set; }
    public string Status { get; set; } = "alive";
        
    public HeartbeatMessage()
    {
        Type = MessageType.Heartbeat;
        ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}