using GenHTTP.Modules.Websockets;
using Server.Helpers;

namespace Server;

public class MessageReceivedEventArgs : EventArgs
{
    public BaseMessage Message { get; }
    public IWebsocketConnection WebSocket { get; }

    public MessageReceivedEventArgs(BaseMessage message, IWebsocketConnection webSocket)
    {
        Message = message;
        WebSocket = webSocket;
    }
}