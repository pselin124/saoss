using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Saoss.Network.Discovery;

/// <summary>Where a <see cref="LanBrowser"/> sends its requests.</summary>
public static class BroadcastTargets
{
    /// <summary>
    /// The limited broadcast 255.255.255.255, the directed broadcast of every IPv4 network this machine is on (some
    /// systems send the limited broadcast out of one interface only) and 127.0.0.1 for a host on this machine.
    /// </summary>
    public static IReadOnlyList<IPEndPoint> ForPort(int port)
    {
        IEnumerable<IPAddress> addresses = [IPAddress.Broadcast, .. DirectedBroadcasts(), IPAddress.Loopback];
        return [.. addresses.Distinct().Select(address => new IPEndPoint(address, port))];
    }

    /// <summary>The broadcast address of the subnet of <paramref name="address"/> with <paramref name="mask"/>.</summary>
    public static IPAddress DirectedBroadcast(IPAddress address, IPAddress mask)
    {
        var bytes = address.GetAddressBytes();
        var maskBytes = mask.GetAddressBytes();
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(bytes[i] | ~maskBytes[i]);
        }

        return new IPAddress(bytes);
    }

    private static List<IPAddress> DirectedBroadcasts()
    {
        try
        {
            return
            [
                .. NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(u.Address) && u.PrefixLength is > 0 and < 31)
                    .Select(u => DirectedBroadcast(u.Address, u.IPv4Mask)),
            ];
        }
        catch (NetworkInformationException)
        {
            return [];
        }
    }
}
