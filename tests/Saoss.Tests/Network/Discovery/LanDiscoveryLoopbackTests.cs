using System.Net;
using Saoss.Core;
using Saoss.Network.Discovery;
using Saoss.Network.Protocol;
using Saoss.Network.Sessions;

namespace Saoss.Tests.Network.Discovery;

[TestClass]
public sealed class LanDiscoveryLoopbackTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    [TestMethod]
    public void Browser_FindsTheHost_SeesItsMatch_AndForgetsItWhenItStops()
    {
        using var host = new HostSession(["hostess", "guest"], port: 0);
        host.Lobby.SetMode(GameMode.EatTheMushroom);
        using var announcer = LanAnnouncer.TryStart(port: 0);
        Assert.IsNotNull(announcer);
        var clock = new ManualClock();

        // A dead target first: its ICMP "port unreachable" must not stop the browser.
        IPEndPoint[] targets = [new(IPAddress.Loopback, 1), new(IPAddress.Loopback, announcer.Port)];
        using var browser = new LanBrowser(targets, clock);
        void Pump()
        {
            host.Update();
            announcer.Update(() => LanGameInfo.FromSession(host));
            browser.Update();
        }

        Poll.Until(() => browser.Hosts.Count == 1, Pump, "the host to be found", Timeout);
        var found = browser.Hosts[0];
        Assert.AreEqual(IPAddress.Loopback, found.Address);
        Assert.AreEqual(new LanGameInfo(ProtocolVersion.Current, host.Port, "hostess", 2, 14, 16, GameMode.EatTheMushroom, IsInGame: false), found.Game);
        Assert.IsTrue(found.IsJoinable(1));

        host.StartMatch(seed: 3);
        clock.Advance(LanBrowser.RequestInterval);
        Poll.Until(() => browser.Hosts[0].Game.IsInGame, Pump, "the host to report its match", Timeout);
        Assert.IsFalse(browser.Hosts[0].IsJoinable(1));

        announcer.Dispose();
        clock.Advance(LanHostList.Expiry + LanBrowser.RequestInterval);
        browser.Update();
        Assert.IsEmpty(browser.Hosts);
    }

    [TestMethod]
    public void SecondAnnouncerOnTheSamePort_DoesNotThrow()
    {
        using var first = LanAnnouncer.TryStart(port: 0);
        Assert.IsNotNull(first);

        using var second = LanAnnouncer.TryStart(first.Port);
        second?.Update(() => throw new AssertFailedException("No request was sent."));
    }
}
