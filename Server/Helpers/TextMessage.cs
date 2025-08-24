using Server.Helpers;

namespace Server;

public class TextMessage : BaseMessage
{
    public string Content { get; set; }
        
    public TextMessage()
    {
        Type = MessageType.Text;
    }
}