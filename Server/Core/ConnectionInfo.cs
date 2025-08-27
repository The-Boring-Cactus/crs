using System.Net.WebSockets;

namespace Server.Core;

public class ConnectionInfo
{
    public string UserId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public WebSocket WebSocket { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
}