using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Match;
using Saoss.Network.Lobby;
using Saoss.Network.Sessions;
using Saoss.Tests.Network;
using Saoss.Tests.Simulation.Modes;

namespace Saoss.Tests.Match;

/// <summary>Tests the match drivers behind the game screen, the network ones over loopback.</summary>
[TestClass]
public sealed class MatchDriverTests
{
    private const string Loopback = "127.0.0.1";
    private const int Seed = 5;

    private static readonly ShipControls Thrust = new() { Thrust = true };

    [TestMethod]
    public void Local_PausesForAChoice_ThenStepsOncePerAdvance()
    {
        var driver = new LocalMatchDriver(ModeTestSettings.Credits(choose: true, humans: [true, false]), Seed);

        CollectionAssert.AreEqual(new[] { 0 }, driver.LocalShips.ToArray());
        Assert.AreEqual(0, driver.LocalChoice);
        Assert.IsEmpty(driver.Advance(new Dictionary<int, ShipControls>()));

        Assert.IsTrue(driver.ChooseShip(0, ShipType.Trek));
        var ticks = driver.Advance(new Dictionary<int, ShipControls> { [0] = Thrust });

        Assert.HasCount(1, ticks);
        Assert.AreEqual(1, driver.World.Tick);
        Assert.IsNull(driver.LocalChoice);
        Assert.IsTrue(driver.World.Ships[0].Controls.Thrust);
    }

    [TestMethod]
    public void HostAndClient_PlayOnWhileTheClientBuysItsShip()
    {
        using var host = new HostSession(["host"], port: 0);
        using var client = ClientSession.Connect(Loopback, host.Port, ["guest"]);
        void Pump()
        {
            host.Update();
            client.Update();
        }

        Poll.Until(() => client.State == ClientState.Lobby && client.Lobby?.Slots[1].Status == SlotStatus.Human, Pump, "the client in the lobby");
        host.Lobby.SetMode(GameMode.Deathmatch);
        host.StartMatch(Seed);
        var hostDriver = new HostMatchDriver(host);
        Poll.Until(() => client.State == ClientState.InMatch, Pump, "the match to start");
        var clientDriver = new ClientMatchDriver(client);

        Assert.AreEqual(0, hostDriver.LocalChoice);
        CollectionAssert.AreEqual(new[] { 1 }, clientDriver.LocalShips.ToArray());
        Poll.Until(() => hostDriver.World.Tick > 5 && client.World!.Tick > 0, () => AdvanceBoth(host, hostDriver, client, clientDriver), "the match to run");
        Assert.AreEqual(1, clientDriver.LocalChoice);
        Assert.IsTrue(hostDriver.World.IsChoosingShip(0), "Nobody waits for the buyers.");

        Assert.IsTrue(clientDriver.ChooseShip(1, ShipType.Ufo));
        Assert.IsNull(clientDriver.LocalChoice, "The menu closes while the host takes the purchase.");
        Poll.Until(() => hostDriver.World.Ships[1].Type == ShipType.Ufo && !hostDriver.World.IsChoosingShip(1), () => AdvanceBoth(host, hostDriver, client, clientDriver), "the host to sell the ship");

        Poll.Until(
            () => hostDriver.World.Ships[1].Controls.Thrust,
            () => AdvanceBoth(host, hostDriver, client, clientDriver, clientControls: Thrust),
            "the client's thrust to reach the host");
    }

    [TestMethod]
    public void Client_RefusesAnUnaffordableShipWithoutAskingTheHost()
    {
        using var host = new HostSession(["host"], port: 0);
        using var client = ClientSession.Connect(Loopback, host.Port, ["guest"]);
        void Pump()
        {
            host.Update();
            client.Update();
        }

        Poll.Until(() => client.State == ClientState.Lobby && client.Lobby?.Slots[1].Status == SlotStatus.Human, Pump, "the client in the lobby");
        host.Lobby.SetMode(GameMode.Deathmatch);
        host.Lobby.SetCredits(LobbyState.MinCredits);
        host.StartMatch(Seed);
        Poll.Until(() => client.State == ClientState.InMatch, Pump, "the match to start");
        var clientDriver = new ClientMatchDriver(client);

        Assert.IsFalse(clientDriver.ChooseShip(1, ShipType.Elephant));
        Assert.AreEqual(1, clientDriver.LocalChoice);
    }

    private static void AdvanceBoth(HostSession host, HostMatchDriver hostDriver, ClientSession client, ClientMatchDriver clientDriver, ShipControls clientControls = default)
    {
        host.Update();
        hostDriver.Advance(new Dictionary<int, ShipControls>());
        client.Update();
        clientDriver.Advance(new Dictionary<int, ShipControls> { [1] = clientControls });
    }
}
