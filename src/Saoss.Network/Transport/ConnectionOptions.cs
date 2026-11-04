namespace Saoss.Network.Transport;

/// <summary>Timing and buffering of a <see cref="TcpConnection"/>.</summary>
public sealed record ConnectionOptions
{
    /// <summary>Defaults: heartbeat every second, drop after five silent seconds.</summary>
    public static ConnectionOptions Default { get; } = new();

    /// <summary>A heartbeat is sent when nothing else was sent for this long.</summary>
    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The connection is dropped when nothing arrived for this long (the original allowed 40 checks of 250 ms).
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Frames waiting to be sent; a peer that falls this far behind is dropped.</summary>
    public int SendQueueCapacity { get; init; } = 1024;
}
