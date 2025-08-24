using Server.Helpers;

namespace Server;

public class DataMessage : BaseMessage
{
    public string DataType { get; set; }
    public object Payload { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
        
    public DataMessage()
    {
        Type = MessageType.Data;
    }
}