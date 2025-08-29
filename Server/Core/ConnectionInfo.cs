

using GenHTTP.Modules.Websockets;
using System.Net.WebSockets;

namespace Server.Core;

public class ConnectionInfo
{
    public string UserId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public IWebsocketConnection WebSocket { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
    public WebSocketMessageClient WebSocketMessageClient { get; set; }
}