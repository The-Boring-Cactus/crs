using Server.Helpers;

namespace Server;

public class NotificationMessage : BaseMessage
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Category { get; set; }
    public int Priority { get; set; } = 1; // 1-5, siendo 5 la más alta
        
    public NotificationMessage()
    {
        Type = MessageType.Notification;
    }
}