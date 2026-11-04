using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Saoss.Network.Protocol;

namespace Saoss.Network.Transport;

/// <summary>
/// Listens for clients on a TCP port on all IPv4 interfaces (the original's port 9999). Accepted connections queue
/// up until the game thread takes them with <see cref="TryAccept"/>.
/// </summary>
public sealed class NetHost : IDisposable
{
    private readonly TcpListener _listener;
    private readonly ConnectionOptions _options;
    private readonly ConcurrentQueue<TcpConnection> _accepted = new();
    private readonly CancellationTokenSource _cancel = new();

    /// <summary>Starts listening; port 0 picks a free port (see <see cref="Port"/>).</summary>
    public NetHost(int port = ProtocolVersion.DefaultPort, ConnectionOptions? options = null)
    {
        _options = options ?? ConnectionOptions.Default;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _ = Task.Run(AcceptLoopAsync);
    }

    /// <summary>The port listened on.</summary>
    public int Port { get; }

    /// <summary>Takes the next accepted connection, if any.</summary>
    public bool TryAccept([NotNullWhen(true)] out TcpConnection? connection) => _accepted.TryDequeue(out connection);

    /// <summary>Stops listening; connections already taken stay open.</summary>
    public void Dispose()
    {
        _cancel.Cancel();
        _listener.Stop();
        while (_accepted.TryDequeue(out var connection))
        {
            connection.Close();
        }
    }

    private async Task AcceptLoopAsync()
    {
        try
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync(_cancel.Token);
                _accepted.Enqueue(new TcpConnection(client, _options));
            }
        }
        catch (Exception exception) when (exception is SocketException or ObjectDisposedException or OperationCanceledException)
        {
            // Stopped.
        }
    }
}
