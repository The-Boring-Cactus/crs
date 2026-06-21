namespace FunctEngine
{
    public class OutputEmittedEventArgs : EventArgs
    {
        public string OutputType { get; }
        public object Payload { get; }
        public string ConnectionId { get; }

        public OutputEmittedEventArgs(string outputType, object payload, string connectionId)
        {
            OutputType = outputType;
            Payload = payload;
            ConnectionId = connectionId;
        }
    }
}
