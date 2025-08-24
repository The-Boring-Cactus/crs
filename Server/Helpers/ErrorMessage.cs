using Server.Helpers;

namespace Server;

public class ErrorMessage : BaseMessage
{
    public string ErrorCode { get; set; }
    public string ErrorDescription { get; set; }
    public object ErrorDetails { get; set; }
        
    public ErrorMessage()
    {
        Type = MessageType.Error;
    }
}
