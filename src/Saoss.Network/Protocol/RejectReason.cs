namespace Saoss.Network.Protocol;

/// <summary>Why a host turned a client away.</summary>
public enum RejectReason : byte
{
    /// <summary>The client speaks another protocol version.</summary>
    VersionMismatch = 1,

    /// <summary>Not enough open slots for the client's players [pelaajia liikaa!].</summary>
    LobbyFull = 2,

    /// <summary>A match is running; the original stopped accepting clients once the game had started.</summary>
    GameInProgress = 3,
}
