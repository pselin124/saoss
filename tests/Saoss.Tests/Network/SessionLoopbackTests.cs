using Saoss.Core.Simulation;
using Saoss.Network.Lobby;
using Saoss.Network.Protocol;
using Saoss.Network.Sessions;
using Saoss.Network.Transport;
using Saoss.Tests.Simulation.Snapshots;

namespace Saoss.Tests.Network;

[TestClass]
public sealed class SessionLoopbackTests
{
    private const int Seed = 11;
    private const int MatchTicks = 300;
    private const string Loopback = "127.0.0.1";

    private static readonly ShipControls RemoteControls = new() { Thrust = true, Fire = true, Left = true };

    [TestMethod]
    public void TwoClients_JoinChatAndMirrorTheHostsMatch()
    {
        using var host = new HostSession(["hostess"], port: 0);
        using var alice = ClientSession.Connect(Loopback, host.Port, ["alice"]);
        Poll.Until(() => InLobby(alice, 2), () => PumpAll(host, alice), "alice in the lobby");
        using var bob = ClientSession.Connect("localhost", host.Port, ["bob", "carl"]);
        void Pump() => PumpAll(host, alice, bob);
        Poll.Until(() => InLobby(alice, 4) && InLobby(bob, 4), Pump, "both clients in the lobby");

        CollectionAssert.AreEqual(new[] { "hostess", "alice", "bob", "carl" }, alice.Lobby!.Slots.Take(4).Select(s => s.Nickname).ToArray());
        CollectionAssert.AreEqual(new[] { 1 }, host.Lobby.SlotsOf(alice.ClientId).ToArray());

        ArrangeLobbyAndChat(host, alice, bob, Pump);

        var world = host.StartMatch(Seed);
        Poll.Until(() => alice.State == ClientState.InMatch && bob.State == ClientState.InMatch, Pump, "the match to start");
        CollectionAssert.AreEqual(new[] { 1 }, alice.LocalShips.ToArray());
        CollectionAssert.AreEqual(new[] { 2, 3 }, bob.LocalShips.ToArray());
        Assert.IsTrue(host.IsLocalShip(0));

        var (hostEvents, aliceEvents) = PlayMatch(host, world, alice, bob);
        Poll.Until(() => alice.World!.Tick == world.Tick && bob.World!.Tick == world.Tick, () => CollectEvents(host, alice, bob, aliceEvents), "the last update");

        DeepComparer.AssertEqual(world, alice.World, "alice's world");
        DeepComparer.AssertEqual(world, bob.World, "bob's world");
        DeepComparer.AssertEqual(hostEvents, aliceEvents, "events played by alice");
        Assert.IsNotEmpty(hostEvents);
        host.ApplyClientInputs(world);
        Assert.AreEqual(RemoteControls, world.Ships[1].Controls);

        host.ReturnToLobby();
        Poll.Until(() => alice.State == ClientState.Lobby && bob.State == ClientState.Lobby, Pump, "the return to the lobby");
    }

    [TestMethod]
    public void ClientLeavingMidMatch_ShipsTurnAi_AndLobbySlotsTooAfterward()
    {
        using var host = new HostSession(["host"], port: 0);
        using var client = ClientSession.Connect(Loopback, host.Port, ["quitter"]);
        Poll.Until(() => InLobby(client, 2), () => PumpAll(host, client), "the client in the lobby");
        var world = host.StartMatch(Seed);
        Poll.Until(() => client.State == ClientState.InMatch, () => PumpAll(host, client), "the match to start");
        Assert.IsTrue(world.Ships[1].IsHuman);

        client.Leave();
        Poll.Until(() => !world.Ships[1].IsHuman, host.Update, "the ship to turn AI");

        Assert.AreEqual(ClientState.Disconnected, client.State);
        Assert.AreEqual(SlotStatus.Ai, host.Lobby.Slots[1].Status);
        Assert.AreEqual(0, host.JoinedClientCount);
        host.ReturnToLobby();
        Assert.AreEqual(SlotStatus.Ai, host.Lobby.Slots[1].Status);
    }

    [TestMethod]
    public void ConnectionDroppedMidMatch_ShipTurnsAi()
    {
        using var host = new HostSession(["host"], port: 0);
        using var raw = NetClient.ConnectAsync(Loopback, host.Port, Poll.DefaultTimeout).GetAwaiter().GetResult();
        raw.Send(new HelloMessage(ProtocolVersion.Current, ["raw"]));
        Poll.Until(() => host.JoinedClientCount == 1, host.Update, "the raw client to join");
        var world = host.StartMatch(Seed);

        raw.Close();
        Poll.Until(() => !world.Ships[1].IsHuman, host.Update, "the ship to turn AI");
    }

    [TestMethod]
    public void ClientLeavingLobby_FreesItsSlots()
    {
        using var host = new HostSession(["host"], port: 0);
        using var first = ClientSession.Connect(Loopback, host.Port, ["first"]);
        using var second = ClientSession.Connect(Loopback, host.Port, ["second"]);
        Poll.Until(() => InLobby(first, 3) && InLobby(second, 3), () => PumpAll(host, first, second), "both clients in the lobby");
        var leaver = first.ClientId < second.ClientId ? first : second;
        var stayer = leaver == first ? second : first;

        leaver.Leave();
        Poll.Until(() => InLobby(stayer, 2), () => PumpAll(host, stayer), "the lobby update");

        Assert.AreEqual(stayer.ClientId, stayer.Lobby!.Slots[1].OwnerId);
        Assert.AreEqual(SlotStatus.Open, stayer.Lobby.Slots[2].Status);
    }

    [TestMethod]
    public void VersionMismatch_IsRejected()
    {
        using var host = new HostSession(["host"], port: 0);
        using var raw = NetClient.ConnectAsync(Loopback, host.Port, Poll.DefaultTimeout).GetAwaiter().GetResult();
        raw.Send(new HelloMessage(ProtocolVersion.Current + 1, ["old"]));

        Message? reply = null;
        Poll.Until(() => reply != null, () => { host.Update(); raw.TryReceive(out reply); }, "the rejection");

        Assert.AreEqual(new RejectMessage(RejectReason.VersionMismatch), reply);
        Assert.AreEqual(0, host.JoinedClientCount);
    }

    [TestMethod]
    public void TooManyPlayers_AndRunningMatch_AreRejected()
    {
        using var host = new HostSession(["host"], port: 0);
        host.Lobby.TrySetMaxPlayers(2);
        using var crowd = ClientSession.Connect(Loopback, host.Port, ["a", "b"]);
        Poll.Until(() => crowd.State == ClientState.Disconnected, () => PumpAll(host, crowd), "the rejection");
        Assert.AreEqual((DisconnectReason.Rejected, RejectReason.LobbyFull), (crowd.DisconnectReason, crowd.RejectReason));

        host.StartMatch(Seed);
        using var late = ClientSession.Connect(Loopback, host.Port, ["late"]);
        Poll.Until(() => late.State == ClientState.Disconnected, () => PumpAll(host, late), "the rejection");
        Assert.AreEqual(RejectReason.GameInProgress, late.RejectReason);
    }

    [TestMethod]
    public void HostShutdown_ClientsSeeHostLeft()
    {
        var host = new HostSession(["host"], port: 0);
        using var client = ClientSession.Connect(Loopback, host.Port, ["c"]);
        Poll.Until(() => client.State == ClientState.Lobby, () => PumpAll(host, client), "the client in the lobby");

        host.Shutdown();

        Poll.Until(() => client.State == ClientState.Disconnected, client.Update, "the goodbye");
        Assert.AreEqual(DisconnectReason.HostLeft, client.DisconnectReason);
    }

    private static void ArrangeLobbyAndChat(HostSession host, ClientSession alice, ClientSession bob, Action pump)
    {
        host.Lobby.CycleStatus(4);
        host.Lobby.CycleStatus(5);
        host.Lobby.CycleStatus(5);
        bob.RequestTeam(2, 3);
        alice.RequestTeam(2, 9);
        alice.SendChat("hello\u0007 all");
        host.SendChat("welcome");
        Poll.Until(() => bob.ChatLog.Count == 2 && alice.Lobby!.Slots[2].Team == 3 && alice.Lobby.Slots[5].Status == SlotStatus.Ai, pump, "chat and lobby changes");

        Assert.AreEqual(SlotStatus.Closed, host.Lobby.Slots[4].Status);
        CollectionAssert.AreEquivalent(new[] { new ChatLine("alice", "hello all"), new ChatLine("hostess", "welcome") }, bob.ChatLog.ToArray());
        Assert.HasCount(2, host.ChatLog);
    }

    // Steps the host world while the clients send fixed buttons; returns the events the host produced and those
    // alice was given to play, in order.
    private static (List<GameEvent> Host, List<GameEvent> Alice) PlayMatch(HostSession host, World world, ClientSession alice, ClientSession bob)
    {
        var hostEvents = new List<GameEvent>();
        var aliceEvents = new List<GameEvent>();
        for (var tick = 0; tick < MatchTicks; tick++)
        {
            alice.SendInputs([.. alice.LocalShips.Select(i => new ShipInput(i, RemoteControls))]);
            bob.SendInputs([.. bob.LocalShips.Select(i => new ShipInput(i, RemoteControls))]);
            CollectEvents(host, alice, bob, aliceEvents);
            world.Ships[0].Controls = ScriptedMatch.Controls(Seed, world.Tick, 0);
            host.ApplyClientInputs(world);
            world.Step();
            hostEvents.AddRange(world.Events);
            host.BroadcastWorld(world);
        }

        return (hostEvents, aliceEvents);
    }

    private static void CollectEvents(HostSession host, ClientSession alice, ClientSession bob, List<GameEvent> aliceEvents)
    {
        PumpAll(host, alice, bob);
        aliceEvents.AddRange(alice.ReceivedTicks.SelectMany(t => t.Events));
    }

    private static bool InLobby(ClientSession client, int humans) =>
        client.State == ClientState.Lobby && client.Lobby?.Slots.Count(s => s.Status == SlotStatus.Human) == humans;

    private static void PumpAll(HostSession host, params ClientSession[] clients)
    {
        host.Update();
        foreach (var client in clients)
        {
            client.Update();
        }
    }
}
