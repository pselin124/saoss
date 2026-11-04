using System.Net;
using System.Net.Sockets;

namespace Saoss.Network.Transport;

/// <summary>Opens a connection to a host by name or address.</summary>
public static class NetClient
{
    /// <summary>Default time allowed for connecting.</summary>
    public static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Connects to <paramref name="host"/>, trying every address it resolves to, IPv4 first because hosts listen on
    /// IPv4 (otherwise "localhost" waits for ::1 to be refused). Throws <see cref="TimeoutException"/> after
    /// <paramref name="timeout"/> and <see cref="SocketException"/> when the host cannot be reached.
    /// </summary>
    public static async Task<TcpConnection> ConnectAsync(string host, int port, TimeSpan timeout, ConnectionOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        var client = new TcpClient();
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, timeoutSource.Token);
            await client.ConnectAsync([.. addresses.OrderBy(a => a.AddressFamily != AddressFamily.InterNetwork)], port, timeoutSource.Token);
            return new TcpConnection(client, options ?? ConnectionOptions.Default);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            client.Dispose();
            throw new TimeoutException($"Connecting to {host}:{port} timed out.");
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
