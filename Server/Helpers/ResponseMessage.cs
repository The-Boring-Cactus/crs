using Server.Helpers;

namespace Server;

public class ResponseMessage : BaseMessage
{
    public string RequestId { get; set; }
    public MessageStatus Status { get; set; }
    public object Data { get; set; }
    public string ErrorMessage { get; set; }
        
    public ResponseMessage()
    {
        Type = MessageType.Response;
    }
}
