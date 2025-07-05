namespace FunctEngine
{
    public delegate void StatusUpdateHandler(object sender, StatusString e);

    public class StatusString : EventArgs
    {
        public string status;
        public StatusString(string status)
        {
            this.status = status;
        }
    }
}
