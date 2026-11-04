using Saoss.Core.Simulation;

namespace Saoss.Network.Protocol;

/// <summary>
/// A match begins [game started]. Clients create the same <see cref="World"/> from the settings and seed and then
/// only mirror the host's snapshots.
/// </summary>
/// <param name="Settings">Match settings; the player list index is the ship index.</param>
/// <param name="Seed">Random seed of the world.</param>
/// <param name="ShipSlots">Lobby slot of every ship, by ship index.</param>
public sealed record MatchStartMessage(MatchSettings Settings, int Seed, IReadOnlyList<int> ShipSlots) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.MatchStart;
}
