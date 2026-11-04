using Saoss.Core;
using Saoss.Network.Lobby;
using Saoss.Network.Sessions;

namespace Saoss.Network.Discovery;

/// <summary>What a host tells joiners about its game in a discovery reply.</summary>
/// <param name="ProtocolVersion">The host's game protocol version (<see cref="Protocol.ProtocolVersion.Current"/>).</param>
/// <param name="Port">TCP port of the game.</param>
/// <param name="HostNickname">Nickname of the host's first player.</param>
/// <param name="HumanPlayers">Humans in the lobby.</param>
/// <param name="OpenSlots">Slots a joiner's players can take.</param>
/// <param name="MaxPlayers">Slots in use.</param>
/// <param name="Mode">Game mode, or null when this version does not know it.</param>
/// <param name="IsInGame">Whether a match is running; nobody can join then.</param>
public sealed record LanGameInfo(
    int ProtocolVersion,
    int Port,
    string HostNickname,
    int HumanPlayers,
    int OpenSlots,
    int MaxPlayers,
    GameMode? Mode,
    bool IsInGame)
{
    /// <summary>Describes the game of a running host session.</summary>
    public static LanGameInfo FromSession(HostSession session)
    {
        var lobby = session.Lobby;
        return new LanGameInfo(
            Protocol.ProtocolVersion.Current,
            session.Port,
            session.HostNickname,
            lobby.Slots.Take(lobby.MaxPlayers).Count(s => s.Status == SlotStatus.Human),
            lobby.OpenSlotCount,
            lobby.MaxPlayers,
            lobby.Mode,
            session.World != null);
    }
}
