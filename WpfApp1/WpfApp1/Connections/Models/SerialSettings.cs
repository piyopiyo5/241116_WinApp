using System.IO.Ports;

namespace WpfApp1.Connections.Models;

public class SerialSettings
{
    public string PortName { get; set; } = string.Empty;
    public int BaudRate { get; set; } = 9600;
    public Parity Parity { get; set; } = Parity.None;
    public int DataBits { get; set; } = 8;
    public StopBits StopBits { get; set; } = StopBits.One;
    public bool RtsEnable { get; set; } = true;
    public bool DtrEnable { get; set; } = true;

    public SerialSettings()
    {
    }

    public SerialSettings(string portName, int baudRate = 9600)
    {
        PortName = portName;
        BaudRate = baudRate;
    }

    public static string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }
}
