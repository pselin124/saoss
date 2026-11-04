using System.Globalization;
using Saoss.Core;
using Saoss.Network.Lobby;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// Texts of the lobby table and of the network match (net.cpp paivitaPelaajat and drawHostMPMenu): slot names,
/// statuses, teams and game modes.
/// </summary>
public static class LobbyTexts
{
    /// <summary>
    /// Name of every slot: the nickname of a human, "CPU N" for the N-th AI slot in use and "-" for open or closed
    /// slots.
    /// </summary>
    public static IReadOnlyList<string> SlotNames(LobbyState lobby)
    {
        var cpu = 0;
        return
        [
            .. lobby.Slots.Select((slot, i) => slot.Status switch
            {
                SlotStatus.Human => slot.Nickname,
                SlotStatus.Ai when i < lobby.MaxPlayers => "CPU " + (++cpu).ToString(CultureInfo.InvariantCulture),
                _ => "-",
            }),
        ];
    }

    /// <summary>Nickname of every ship of a match started from <paramref name="lobby"/> with the given ship slots.</summary>
    public static IReadOnlyList<string> ShipNames(LobbyState lobby, IReadOnlyList<int> shipSlots)
    {
        var names = SlotNames(lobby);
        return [.. shipSlots.Select(slot => names[slot])];
    }

    /// <summary>The status column: "open", "closed", "CPU" or "human".</summary>
    public static string Status(SlotStatus status) => status switch
    {
        SlotStatus.Open => "open",
        SlotStatus.Closed => "closed",
        SlotStatus.Ai => "CPU",
        _ => "human",
    };

    /// <summary>The team column: "team N" counting from 1 for players, "team 0" for empty slots.</summary>
    public static string Team(LobbySlot slot) =>
        "team " + (slot.IsPlayer ? slot.Team + 1 : 0).ToString(CultureInfo.InvariantCulture);

    /// <summary>Name of a game mode as the lobby shows it.</summary>
    public static string Mode(GameMode mode) => mode switch
    {
        GameMode.RandomShipsDeathmatch => "deathmatch with random ships",
        GameMode.FlowerDomination => "flower domination",
        GameMode.CaptureTheFlower => "capture the flower",
        GameMode.EatTheMushroom => "eat the mushroom",
        _ => "deathmatch",
    };
}
