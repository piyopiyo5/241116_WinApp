namespace WpfApp1.Connections.Models;

public class ConnectionEventArgs : EventArgs
{
    public byte[]? Data { get; }
    public string Message { get; }
    public Exception? Error { get; }
    public DateTime Timestamp { get; }

    public ConnectionEventArgs(byte[]? data = null, string message = "", Exception? error = null)
    {
        Data = data;
        Message = message;
        Error = error;
        Timestamp = DateTime.Now;
    }
}
