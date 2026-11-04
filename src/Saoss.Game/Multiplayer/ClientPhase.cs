namespace Saoss.Game.Multiplayer;

/// <summary>Where a <see cref="ClientFlow"/> is.</summary>
public enum ClientPhase
{
    /// <summary>Waiting for the connection and the host's welcome.</summary>
    Connecting,

    /// <summary>In the lobby view or one of its submenus.</summary>
    Lobby,

    /// <summary>Playing a match.</summary>
    Match,

    /// <summary>Looking at the end-of-match screen.</summary>
    Victory,

    /// <summary>The session is over; only the closing message may be up.</summary>
    Ended,
}
