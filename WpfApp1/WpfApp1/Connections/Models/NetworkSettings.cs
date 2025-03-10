using System.Net;
using System.Net.NetworkInformation;

namespace WpfApp1.Connections.Models;

public class NetworkSettings
{
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public IPAddress? LocalAddress { get; set; }

    public static List<NetworkInterface> GetAvailableInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .ToList();
    }

    public NetworkSettings()
    {
    }

    public NetworkSettings(string ipAddress, int port, IPAddress? localAddress = null)
    {
        IpAddress = ipAddress;
        Port = port;
        LocalAddress = localAddress;
    }
}
