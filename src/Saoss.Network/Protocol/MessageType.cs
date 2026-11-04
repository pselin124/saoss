namespace Saoss.Network.Protocol;

/// <summary>Kind of a message; the first byte of every payload.</summary>
public enum MessageType : byte
{
    /// <summary>Client to host: first message after connecting.</summary>
    Hello = 1,

    /// <summary>Host to client: the join was accepted.</summary>
    Welcome = 2,

    /// <summary>Host to client: the join was refused; the host closes the connection.</summary>
    Reject = 3,

    /// <summary>Host to clients: the whole lobby.</summary>
    LobbyUpdate = 4,

    /// <summary>Client to host: put one of the client's slots on another team.</summary>
    TeamChangeRequest = 5,

    /// <summary>Both ways: a chat line.</summary>
    Chat = 6,

    /// <summary>Host to clients: a match begins.</summary>
    MatchStart = 7,

    /// <summary>Client to host: buttons of the client's ships.</summary>
    PlayerInput = 8,

    /// <summary>Client to host: buy a ship for a pending choice.</summary>
    ShipChoiceRequest = 9,

    /// <summary>Host to clients: snapshot of the match after a tick.</summary>
    WorldUpdate = 10,

    /// <summary>Host to clients: the match is over and everybody is back in the lobby.</summary>
    ReturnToLobby = 11,

    /// <summary>Both ways: the sender is leaving.</summary>
    Goodbye = 12,

    /// <summary>Both ways: keeps an idle connection alive.</summary>
    Heartbeat = 13,
}
