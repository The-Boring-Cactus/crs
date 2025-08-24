using Server.Helpers;

namespace Server;

public class CommandMessage : BaseMessage
{
    public string Command { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
        
    public CommandMessage()
    {
        Type = MessageType.Command;
    }
}
