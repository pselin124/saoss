using System.Net;
using Saoss.Network.Protocol;

namespace Saoss.Network.Discovery;

/// <summary>A game found on the LAN.</summary>
/// <param name="Address">Address the host answered from; connect there.</param>
/// <param name="InstanceId">Random id of the host, the same on every address it answered from.</param>
/// <param name="Game">The host's game as of its latest reply.</param>
public sealed record DiscoveredHost(IPAddress Address, int InstanceId, LanGameInfo Game)
{
    /// <summary>TCP port of the game.</summary>
    public int Port => Game.Port;

    /// <summary>Whether the host runs the same game protocol as this game.</summary>
    public bool IsCompatible => Game.ProtocolVersion == ProtocolVersion.Current;

    /// <summary>Whether <paramref name="localPlayers"/> players from this machine could join now.</summary>
    public bool IsJoinable(int localPlayers) => IsCompatible && !Game.IsInGame && Game.OpenSlots >= localPlayers;
}
