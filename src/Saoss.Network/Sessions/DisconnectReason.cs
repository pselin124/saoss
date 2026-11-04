namespace Saoss.Network.Sessions;

/// <summary>Why a <see cref="ClientSession"/> ended.</summary>
public enum DisconnectReason
{
    /// <summary>The host could not be reached.</summary>
    ConnectFailed,

    /// <summary>The host refused the players; see <see cref="ClientSession.RejectReason"/>.</summary>
    Rejected,

    /// <summary>The host quit.</summary>
    HostLeft,

    /// <summary>The connection broke or timed out.</summary>
    ConnectionLost,

    /// <summary>The host sent data that breaks the protocol.</summary>
    ProtocolError,

    /// <summary>This side left.</summary>
    Left,
}
