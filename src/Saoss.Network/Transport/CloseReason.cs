namespace Saoss.Network.Transport;

/// <summary>Why a <see cref="TcpConnection"/> closed.</summary>
public enum CloseReason
{
    /// <summary>Closed by this side.</summary>
    Local = 1,

    /// <summary>The peer closed the connection.</summary>
    RemoteClosed = 2,

    /// <summary>Nothing arrived within <see cref="ConnectionOptions.Timeout"/>.</summary>
    TimedOut = 3,

    /// <summary>The peer sent an oversized frame or a malformed message.</summary>
    ProtocolViolation = 4,

    /// <summary>A socket error.</summary>
    NetworkError = 5,

    /// <summary>The peer did not keep up with the data sent to it.</summary>
    SendQueueFull = 6,
}
