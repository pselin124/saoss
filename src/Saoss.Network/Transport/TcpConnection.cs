using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading.Channels;
using Saoss.Network.Protocol;

namespace Saoss.Network.Transport;

/// <summary>
/// A framed message connection over TCP. Background tasks receive into a queue and send from a channel; the game
/// thread only calls the non-blocking <see cref="Send"/>, <see cref="TryReceive"/> and <see cref="Update"/>.
/// Messages that arrived before the connection closed can still be received afterwards.
/// </summary>
public sealed class TcpConnection : IDisposable
{
    // How long a graceful close waits for the peer to close its side after the last frame was sent.
    private static readonly TimeSpan LingerTime = TimeSpan.FromSeconds(1);

    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly ConnectionOptions _options;
    private readonly ConcurrentQueue<Message> _received = new();
    private readonly Channel<byte[]> _outgoing;
    private readonly CancellationTokenSource _cancel = new();
    private readonly Task _receiveTask;
    private long _lastReceived;
    private long _lastSent;
    private int _closeReason;
    private int _tornDown;

    /// <summary>Takes over a connected client and starts the background tasks.</summary>
    internal TcpConnection(TcpClient client, ConnectionOptions options)
    {
        _client = client;
        _client.NoDelay = true;
        _stream = client.GetStream();
        _options = options;
        _outgoing = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(options.SendQueueCapacity) { SingleReader = true });
        RemoteAddress = client.Client.RemoteEndPoint?.ToString() ?? string.Empty;
        _lastReceived = Stopwatch.GetTimestamp();
        _lastSent = _lastReceived;
        _receiveTask = Task.Run(ReceiveLoopAsync);
        _ = Task.Run(SendLoopAsync);
    }

    /// <summary>Address of the peer, for display.</summary>
    public string RemoteAddress { get; }

    /// <summary>Whether the connection is closed or closing; nothing more is sent.</summary>
    public bool IsClosed => Volatile.Read(ref _closeReason) != 0;

    /// <summary>Why the connection closed, or null while it is open.</summary>
    public CloseReason? CloseReason => IsClosed ? (CloseReason)Volatile.Read(ref _closeReason) : null;

    /// <summary>Queues a message; dropped silently once the connection is closed.</summary>
    public void Send(Message message) => SendFrame(MessageCodec.EncodeFrame(message));

    /// <summary>Takes the next received message, if any.</summary>
    public bool TryReceive([NotNullWhen(true)] out Message? message) => _received.TryDequeue(out message);

    /// <summary>Sends a heartbeat when idle and closes the connection when the peer has been silent too long.</summary>
    public void Update()
    {
        if (IsClosed)
        {
            return;
        }

        var now = Stopwatch.GetTimestamp();
        if (Stopwatch.GetElapsedTime(Volatile.Read(ref _lastReceived), now) > _options.Timeout)
        {
            Fail(Transport.CloseReason.TimedOut);
        }
        else if (Stopwatch.GetElapsedTime(_lastSent, now) >= _options.HeartbeatInterval)
        {
            Send(new HeartbeatMessage());
        }
    }

    /// <summary>Closes the connection at once; queued frames are dropped.</summary>
    public void Close()
    {
        MarkClosed(Transport.CloseReason.Local);
        TearDown();
    }

    /// <summary>Sends what is queued, then closes; use after a final message such as a rejection or goodbye.</summary>
    public void CloseAfterSending()
    {
        MarkClosed(Transport.CloseReason.Local);
        _outgoing.Writer.TryComplete();
    }

    /// <inheritdoc/>
    public void Dispose() => Close();

    /// <summary>Queues an encoded frame, so a broadcast is encoded only once.</summary>
    internal void SendFrame(byte[] frame)
    {
        if (IsClosed)
        {
            return;
        }

        if (!_outgoing.Writer.TryWrite(frame))
        {
            Fail(Transport.CloseReason.SendQueueFull);
            return;
        }

        _lastSent = Stopwatch.GetTimestamp();
    }

    private bool MarkClosed(CloseReason reason) => Interlocked.CompareExchange(ref _closeReason, (int)reason, 0) == 0;

    private void Fail(CloseReason reason)
    {
        MarkClosed(reason);
        TearDown();
    }

    private void TearDown()
    {
        if (Interlocked.Exchange(ref _tornDown, 1) != 0)
        {
            return;
        }

        _cancel.Cancel();
        _outgoing.Writer.TryComplete();
        _client.Dispose();
    }

    private async Task ReceiveLoopAsync()
    {
        var header = new byte[MessageCodec.FrameHeaderSize];
        try
        {
            while (true)
            {
                await _stream.ReadExactlyAsync(header, _cancel.Token);
                var length = BinaryPrimitives.ReadInt32LittleEndian(header);
                if (length is <= 0 or > MessageCodec.MaxFrameSize)
                {
                    Fail(Transport.CloseReason.ProtocolViolation);
                    return;
                }

                var payload = new byte[length];
                await _stream.ReadExactlyAsync(payload, _cancel.Token);
                _received.Enqueue(MessageCodec.Decode(payload));
                Volatile.Write(ref _lastReceived, Stopwatch.GetTimestamp());
            }
        }
        catch (EndOfStreamException)
        {
            Fail(Transport.CloseReason.RemoteClosed);
        }
        catch (ProtocolException)
        {
            Fail(Transport.CloseReason.ProtocolViolation);
        }
        catch (Exception exception) when (exception is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
            Fail(Transport.CloseReason.NetworkError);
        }
    }

    private async Task SendLoopAsync()
    {
        try
        {
            await foreach (var frame in _outgoing.Reader.ReadAllAsync(_cancel.Token))
            {
                await _stream.WriteAsync(frame, _cancel.Token);
            }

            // Graceful close: the peer reads everything, sees the end of the stream and closes its side.
            _client.Client.Shutdown(SocketShutdown.Send);
            await Task.WhenAny(_receiveTask, Task.Delay(LingerTime));
            TearDown();
        }
        catch (Exception exception) when (exception is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
            Fail(Transport.CloseReason.NetworkError);
        }
    }
}
