using System.IO;
using WpfApp1.Connections.Interfaces;

namespace WpfApp1.Connections.Logging;

public class FileLogger : ILogger
{
    private readonly string _logPath;
    private readonly object _lock = new();

    public FileLogger(string logPath)
    {
        _logPath = logPath;
        var directory = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void LogInfo(string message)
    {
        WriteToFile($"[INFO] {message}");
    }

    public void LogError(string message, Exception ex)
    {
        WriteToFile($"[ERROR] {message}\nException: {ex}");
    }

    public void LogData(string direction, byte[] data)
    {
        var hexString = BitConverter.ToString(data);
        WriteToFile($"[DATA] {direction}: {hexString}");
    }

    private void WriteToFile(string message)
    {
        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"{timestamp} {message}";
            File.AppendAllLines(_logPath, new[] { logMessage });
        }
    }
}
