using System.Net.WebSockets;
using GenHTTP.Modules.Websockets;
using Server.Helpers;

namespace Server;

public class WebSocketMessageClient: IDisposable
{
    private IWebsocketConnection _webSocket;
    public event EventHandler<MessageReceivedEventArgs> TextMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> CommandMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> ResponseMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> NotificationMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> ErrorMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> AuthenticationMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> DataMessageReceived;
    public event EventHandler<MessageReceivedEventArgs> HeartbeatMessageReceived;
    public event EventHandler<Exception> ErrorOccurred;

    public WebSocketMessageClient(IWebsocketConnection webSocket)
    {
        _webSocket = webSocket;
    }

    public void ReceiveMsg(string json)
    {
        try
        {
            var message = MessageSerializer.Deserialize(json);
            HandleReceivedMessage(message);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }
    private void HandleReceivedMessage(BaseMessage message)
    {
        var eventArgs = new MessageReceivedEventArgs(message, _webSocket);

       

        // Eventos específicos por tipo
        switch (message.Type)
        {
            case MessageType.Text:
                TextMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Command:
                CommandMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Response:
                ResponseMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Notification:
                NotificationMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Error:
                ErrorMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Authentication:
                AuthenticationMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Data:
                DataMessageReceived?.Invoke(this, eventArgs);
                break;
            case MessageType.Heartbeat:
                HeartbeatMessageReceived?.Invoke(this, eventArgs);
                break;
        }
    }
    public void Dispose()
    {
        ;
    }
}