using System.Net;
using System.Net.Sockets;

namespace Saoss.Network.Discovery;

/// <summary>
/// A non-blocking IPv4 UDP socket polled from the game thread. Receive and send errors are swallowed: discovery is
/// best effort and a lost packet is simply sent again a second later.
/// </summary>
internal sealed class DiscoverySocket : IDisposable
{
    // Windows reports an ICMP "port unreachable" for an earlier send as an error on the next receive unless told not to.
    private const int SioUdpConnReset = -1744830452;

    // Datagrams read per poll, so a flood cannot stall a frame.
    private const int MaxPacketsPerPoll = 64;

    private readonly Socket _socket;
    private readonly byte[] _buffer = new byte[DiscoveryProtocol.MaxPacketSize + 1];

    /// <summary>Binds to <paramref name="port"/> on all interfaces (0 picks a free port).</summary>
    public DiscoverySocket(int port, bool shareAddress)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            _socket.Blocking = false;
            _socket.EnableBroadcast = true;
            if (OperatingSystem.IsWindows())
            {
                _socket.IOControl(SioUdpConnReset, [0, 0, 0, 0], null);
            }

            if (shareAddress)
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Port = ((IPEndPoint)_socket.LocalEndPoint!).Port;
        }
        catch
        {
            _socket.Dispose();
            throw;
        }
    }

    /// <summary>The bound port.</summary>
    public int Port { get; }

    /// <summary>Hands every waiting datagram that fits the size limit to <paramref name="handle"/>.</summary>
    public void Poll(Action<IPEndPoint, ReadOnlySpan<byte>> handle)
    {
        for (var i = 0; i < MaxPacketsPerPoll && HasData(); i++)
        {
            EndPoint from = new IPEndPoint(IPAddress.Any, 0);
            int length;
            try
            {
                length = _socket.ReceiveFrom(_buffer, ref from);
            }
            catch (SocketException)
            {
                continue;
            }

            if (length <= DiscoveryProtocol.MaxPacketSize)
            {
                handle((IPEndPoint)from, _buffer.AsSpan(0, length));
            }
        }
    }

    /// <summary>Sends a datagram, ignoring failures such as an unreachable network.</summary>
    public void Send(byte[] packet, IPEndPoint target)
    {
        try
        {
            _socket.SendTo(packet, target);
        }
        catch (SocketException)
        {
            // Best effort.
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _socket.Dispose();

    private bool HasData()
    {
        try
        {
            return _socket.Available > 0;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
