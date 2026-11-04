namespace Saoss.Network.Sessions;

/// <summary>Where a <see cref="ClientSession"/> is.</summary>
public enum ClientState
{
    /// <summary>Opening the TCP connection.</summary>
    Connecting,

    /// <summary>Connected and waiting for the host to accept the players.</summary>
    Joining,

    /// <summary>In the host's lobby.</summary>
    Lobby,

    /// <summary>Mirroring the host's match.</summary>
    InMatch,

    /// <summary>The session is over; see <see cref="ClientSession.DisconnectReason"/>.</summary>
    Disconnected,
}
