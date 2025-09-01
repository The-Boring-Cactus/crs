namespace FunctEngine;


public delegate void StatusUpdateHandler(object sender, StatusString e);

public class StatusString : EventArgs
{
    public string status;
    public string connectionId;
    public StatusString(string status, string connectionId)
    {
        this.status = status;
        this.connectionId = connectionId;
    }
}
