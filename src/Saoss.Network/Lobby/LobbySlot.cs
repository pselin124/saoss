namespace Saoss.Network.Lobby;

/// <summary>One lobby slot [mpstatus, mptiimi, netnick].</summary>
/// <param name="Status">What occupies the slot.</param>
/// <param name="Team">Team, 0-15; shown as "team N+1".</param>
/// <param name="Nickname">Nickname of a human player; empty otherwise.</param>
/// <param name="OwnerId">
/// Machine of a human player: <see cref="LobbyState.HostId"/> or a client id; <see cref="LobbyState.NoOwner"/> otherwise.
/// </param>
public sealed record LobbySlot(SlotStatus Status, int Team, string Nickname, int OwnerId)
{
    /// <summary>The slot takes part in a match (a human or an AI).</summary>
    public bool IsPlayer => Status is SlotStatus.Human or SlotStatus.Ai;
}
