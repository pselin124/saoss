using Saoss.Core;
using Saoss.Game.Screens.Network;
using Saoss.Network.Lobby;

namespace Saoss.Tests.Screens.Network;

[TestClass]
public sealed class LobbyTextsTests
{
    [TestMethod]
    public void SlotNames_NumberTheCpusInSlotOrder_AndDashEmptySlots()
    {
        var lobby = new LobbyState(["tgo"]);
        lobby.TrySetMaxPlayers(5);
        lobby.CycleStatus(1);
        lobby.CycleStatus(1);
        lobby.CycleStatus(2);
        lobby.CycleStatus(3);
        lobby.CycleStatus(3);

        var names = LobbyTexts.SlotNames(lobby);

        CollectionAssert.AreEqual(new[] { "tgo", "cpu 1", "-", "cpu 2", "-" }, names.Take(5).ToArray());
    }

    [TestMethod]
    public void ShipNames_FollowTheShipSlots()
    {
        var lobby = new LobbyState(["tgo", "joku"]);
        lobby.TrySetMaxPlayers(4);
        lobby.CycleStatus(3);
        lobby.CycleStatus(3);

        var names = LobbyTexts.ShipNames(lobby, lobby.PlayerSlots());

        CollectionAssert.AreEqual(new[] { "tgo", "joku", "cpu 1" }, names.ToArray());
    }

    [TestMethod]
    public void TeamAndStatusTexts_MatchTheOriginal()
    {
        var lobby = new LobbyState(["tgo"]);

        Assert.AreEqual("team 1", LobbyTexts.Team(lobby.Slots[0]));
        Assert.AreEqual("team 0", LobbyTexts.Team(lobby.Slots[1]));
        Assert.AreEqual("human", LobbyTexts.Status(SlotStatus.Human));
        Assert.AreEqual("open", LobbyTexts.Status(SlotStatus.Open));
        Assert.AreEqual("death match with random ships", LobbyTexts.Mode(GameMode.RandomShipsDeathmatch));
    }
}
