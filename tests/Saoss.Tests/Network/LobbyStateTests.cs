using Saoss.Core;
using Saoss.Core.Simulation.Ai;
using Saoss.Network.Lobby;
using Saoss.Network.Protocol;

namespace Saoss.Tests.Network;

[TestClass]
public sealed class LobbyStateTests
{
    [TestMethod]
    public void NewLobby_HostPlayersFirstOnTeamOne_RestOpenOnTeamTwo()
    {
        var lobby = new LobbyState(["anna", "bert"]);

        Assert.AreEqual(new LobbySlot(SlotStatus.Human, 0, "anna", LobbyState.HostId), lobby.Slots[0]);
        Assert.AreEqual(new LobbySlot(SlotStatus.Human, 0, "bert", LobbyState.HostId), lobby.Slots[1]);
        Assert.IsTrue(lobby.Slots.Skip(2).All(s => s == new LobbySlot(SlotStatus.Open, 1, string.Empty, LobbyState.NoOwner)));
        Assert.AreEqual((16, GameMode.RandomShipsDeathmatch, 250), (lobby.MaxPlayers, lobby.Mode, lobby.Credits));
    }

    [TestMethod]
    public void TryJoin_FillsOpenSlotsInOrder_SkippingClosedAndAi()
    {
        var lobby = new LobbyState(["host"]);
        lobby.CycleStatus(1);
        lobby.CycleStatus(2);
        lobby.CycleStatus(2);

        Assert.IsTrue(lobby.TryJoin(1, ["a", "b"]));

        Assert.AreEqual(SlotStatus.Closed, lobby.Slots[1].Status);
        Assert.AreEqual(SlotStatus.Ai, lobby.Slots[2].Status);
        CollectionAssert.AreEqual(new[] { 3, 4 }, lobby.SlotsOf(1).ToArray());
        Assert.AreEqual("b", lobby.Slots[4].Nickname);
    }

    [TestMethod]
    public void TryJoin_NotEnoughOpenSlots_ChangesNothing()
    {
        var lobby = new LobbyState(["host"]);
        Assert.IsTrue(lobby.TrySetMaxPlayers(3));
        var version = lobby.Version;

        Assert.IsFalse(lobby.TryJoin(1, ["a", "b", "c"]));

        Assert.AreEqual(version, lobby.Version);
        Assert.AreEqual(2, lobby.OpenSlotCount);
    }

    [TestMethod]
    public void TryJoin_SanitizesNicknames()
    {
        var lobby = new LobbyState(["host"]);

        lobby.TryJoin(1, ["<<>>", "Pekka Pouta X"]);

        Assert.AreEqual(Nickname.Fallback, lobby.Slots[1].Nickname);
        Assert.AreEqual("Pekka Pout", lobby.Slots[2].Nickname);
    }

    [TestMethod]
    public void RemoveClient_InLobby_LaterPlayersMoveUpAndTeamsStayWithSlots()
    {
        var lobby = new LobbyState(["host"]);
        lobby.TryJoin(1, ["a"]);
        lobby.TryJoin(2, ["b"]);
        lobby.SetTeam(2, 5);

        lobby.RemoveClient(1);

        Assert.AreEqual(new LobbySlot(SlotStatus.Human, 1, "b", 2), lobby.Slots[1]);
        Assert.AreEqual(new LobbySlot(SlotStatus.Open, 5, string.Empty, LobbyState.NoOwner), lobby.Slots[2]);
    }

    [TestMethod]
    public void ConvertClientToAi_SlotsBecomeAiAndStayPut()
    {
        var lobby = new LobbyState(["host"]);
        lobby.TryJoin(1, ["a"]);
        lobby.TryJoin(2, ["b"]);

        CollectionAssert.AreEqual(new[] { 1 }, lobby.ConvertClientToAi(1).ToArray());

        Assert.AreEqual(new LobbySlot(SlotStatus.Ai, 1, string.Empty, LobbyState.NoOwner), lobby.Slots[1]);
        Assert.AreEqual(2, lobby.Slots[2].OwnerId);
        Assert.IsFalse(lobby.TryJoin(1, []));
    }

    [TestMethod]
    public void CycleStatus_OpenClosedAiOpen_NeverTouchesHumans()
    {
        var lobby = new LobbyState(["host"]);
        lobby.TryJoin(1, ["a"]);

        lobby.CycleStatus(0);
        lobby.CycleStatus(1);
        Assert.AreEqual(SlotStatus.Human, lobby.Slots[0].Status);
        Assert.AreEqual(SlotStatus.Human, lobby.Slots[1].Status);

        var seen = new List<SlotStatus>();
        for (var i = 0; i < 3; i++)
        {
            lobby.CycleStatus(2);
            seen.Add(lobby.Slots[2].Status);
        }

        CollectionAssert.AreEqual(new[] { SlotStatus.Closed, SlotStatus.Ai, SlotStatus.Open }, seen);
    }

    [TestMethod]
    public void CycleStatus_ClosingAnEarlierSlot_PlayersMoveUpWhenItOpens()
    {
        var lobby = new LobbyState(["host"]);
        lobby.CycleStatus(1);
        lobby.TryJoin(1, ["a"]);
        Assert.AreEqual(2, lobby.SlotsOf(1).Single());

        lobby.CycleStatus(1);
        lobby.CycleStatus(1);

        Assert.AreEqual(1, lobby.SlotsOf(1).Single());
    }

    [TestMethod]
    public void TrySetTeam_ClientMayChangeOnlyItsOwnSlots()
    {
        var lobby = new LobbyState(["host"]);
        lobby.TryJoin(1, ["a"]);
        lobby.TryJoin(2, ["b"]);

        Assert.IsTrue(lobby.TrySetTeam(1, 1, 9));
        Assert.IsFalse(lobby.TrySetTeam(1, 2, 9));
        Assert.IsFalse(lobby.TrySetTeam(1, 0, 9));
        Assert.IsFalse(lobby.TrySetTeam(1, 1, 16));

        Assert.AreEqual(9, lobby.Slots[1].Team);
        Assert.AreEqual(1, lobby.Slots[2].Team);
    }

    [TestMethod]
    public void TrySetMaxPlayers_RefusedWhenHumansWouldNotFit()
    {
        var lobby = new LobbyState(["host"]);
        lobby.TryJoin(1, ["a", "b"]);

        Assert.IsFalse(lobby.TrySetMaxPlayers(2));
        Assert.AreEqual(3, lobby.Slots.Count(s => s.Status == SlotStatus.Human));
        Assert.IsTrue(lobby.TrySetMaxPlayers(3));
        Assert.IsFalse(lobby.TrySetMaxPlayers(1));
        Assert.IsFalse(lobby.TrySetMaxPlayers(17));
    }

    [TestMethod]
    public void StepCredits_FollowsOriginalSteps()
    {
        var lobby = new LobbyState(["host"]);
        lobby.SetCredits(975);
        lobby.StepCredits(up: true);
        Assert.AreEqual(1000, lobby.Credits);
        lobby.StepCredits(up: true);
        Assert.AreEqual(1100, lobby.Credits);
        lobby.SetCredits(9000);
        lobby.StepCredits(up: true);
        Assert.AreEqual(25, lobby.Credits);
        lobby.StepCredits(up: false);
        Assert.AreEqual(9000, lobby.Credits);
        lobby.SetCredits(1000);
        lobby.StepCredits(up: false);
        Assert.AreEqual(975, lobby.Credits);
    }

    [TestMethod]
    [DataRow(GameMode.RandomShipsDeathmatch, true, false)]
    [DataRow(GameMode.Deathmatch, true, true)]
    [DataRow(GameMode.FlowerDomination, false, false)]
    [DataRow(GameMode.CaptureTheFlower, false, false)]
    [DataRow(GameMode.EatTheMushroom, false, false)]
    public void ToMatchSettings_MapsModesLikeTheOriginalNetworkGame(GameMode mode, bool usesCredits, bool choose)
    {
        var lobby = new LobbyState(["host"]);
        lobby.TryJoin(1, ["a"]);
        lobby.CycleStatus(2);
        lobby.CycleStatus(3);
        lobby.CycleStatus(3);
        lobby.SetTeam(3, 4);
        lobby.SetMode(mode);
        lobby.SetCredits(700);

        var settings = lobby.ToMatchSettings();

        Assert.AreEqual((mode, usesCredits, choose, 700), (settings.Mode, settings.UsesCredits, settings.PlayersChooseShips, settings.Credits));
        CollectionAssert.AreEqual(new[] { 0, 1, 3 }, lobby.PlayerSlots().ToArray());
        CollectionAssert.AreEqual(new[] { (0, true), (1, true), (4, false) }, settings.Players.Select(p => (p.Team, p.IsHuman)).ToArray());
    }

    [TestMethod]
    public void AiDifficulty_DefaultsToTheOriginalAndReachesTheMatch()
    {
        var lobby = new LobbyState(["host"]);
        Assert.AreEqual(AiDifficulty.GalacticTerror, lobby.AiDifficulty);
        var version = lobby.Version;

        lobby.SetAiDifficulty(AiDifficulty.SpaceCadet);

        Assert.AreEqual(version + 1, lobby.Version);
        Assert.AreEqual(AiDifficulty.SpaceCadet, lobby.ToMatchSettings().AiDifficulty);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => lobby.SetAiDifficulty((AiDifficulty)7));
    }

    [TestMethod]
    public void Nickname_AllowsOriginalCharacters()
    {
        Assert.IsTrue(Nickname.IsValid("Åke-Öö.9 ä"));
        Assert.IsFalse(Nickname.IsValid(string.Empty));
        Assert.IsFalse(Nickname.IsValid("elevenchars"));
        Assert.IsFalse(Nickname.IsValid("tab\there"));
        Assert.AreEqual("abc", Nickname.Sanitize("a_b!c"));
    }

    [TestMethod]
    public void ChatText_StripsControlCharactersAndCuts()
    {
        Assert.AreEqual("hello world", ChatText.Sanitize("hel\u0007lo\r\n world\t"));
        Assert.AreEqual(ChatText.MaxLength, ChatText.Sanitize(new string('z', 900)).Length);
        Assert.AreEqual(string.Empty, ChatText.Sanitize("\u0001\u0002"));
    }
}
