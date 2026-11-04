namespace Saoss.Network.Lobby;

/// <summary>What occupies a lobby slot; values are the original's mpstatus codes.</summary>
public enum SlotStatus : byte
{
    /// <summary>Free for a joining player ["open", "-"].</summary>
    Open = 0,

    /// <summary>Not used in the match ["closed"].</summary>
    Closed = 1,

    /// <summary>An AI pilot ["cpu"].</summary>
    Ai = 2,

    /// <summary>A person on the host or on a client ["human"].</summary>
    Human = 3,
}
