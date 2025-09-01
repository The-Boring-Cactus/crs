using Server.Helpers;

namespace Server;

public class AuthenticationMessage : BaseMessage
{
    public string Password { get; set; }
    public string Username { get; set; }
    public long ServerTimestamp { get; set; }
    public string Token { get; set; }

    public AuthenticationMessage()
    {
        Type = MessageType.Authentication;
        ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}