namespace WpfApp1.Connections.Interfaces;

public interface ILogger
{
    void LogInfo(string message);
    void LogError(string message, Exception ex);
    void LogData(string direction, byte[] data);
}
