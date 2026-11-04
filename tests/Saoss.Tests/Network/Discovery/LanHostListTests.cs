using System.Net;
using Saoss.Core;
using Saoss.Network.Discovery;
using Saoss.Network.Protocol;

namespace Saoss.Tests.Network.Discovery;

[TestClass]
public sealed class LanHostListTests
{
    private static readonly IPAddress Lan = IPAddress.Parse("192.168.1.20");
    private static readonly LanGameInfo Game = new(ProtocolVersion.Current, 9999, "host", 1, 15, 16, GameMode.FlowerDomination, IsInGame: false);

    [TestMethod]
    public void Host_ExpiresAfterSilence_ButStaysWhileItAnswers()
    {
        var clock = new ManualClock();
        var list = new LanHostList(clock);
        list.Record(Lan, new DiscoveryReply(1, 7, Game));

        clock.Advance(TimeSpan.FromSeconds(3));
        list.Record(Lan, new DiscoveryReply(1, 7, Game with { HumanPlayers = 2 }));
        clock.Advance(TimeSpan.FromSeconds(3));
        list.RemoveExpired();
        Assert.HasCount(1, list.Hosts);
        Assert.AreEqual(2, list.Hosts[0].Game.HumanPlayers);

        clock.Advance(LanHostList.Expiry - TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(1));
        list.RemoveExpired();
        Assert.IsEmpty(list.Hosts);
    }

    [TestMethod]
    public void HostOnSeveralAddresses_IsListedOnce_WithItsLanAddress()
    {
        var list = new LanHostList(new ManualClock());
        list.Record(IPAddress.Loopback, new DiscoveryReply(1, 7, Game));
        list.Record(Lan, new DiscoveryReply(1, 7, Game));
        list.Record(IPAddress.Loopback, new DiscoveryReply(1, 7, Game));
        list.Record(IPAddress.Parse("10.0.0.3"), new DiscoveryReply(1, 8, Game));

        Assert.HasCount(2, list.Hosts);
        Assert.AreEqual(Lan, list.Hosts[0].Address);
        Assert.AreEqual(8, list.Hosts[1].InstanceId);
    }

    [TestMethod]
    public void List_IsCapped()
    {
        var list = new LanHostList(new ManualClock());
        for (var id = 0; id < LanHostList.Capacity * 2; id++)
        {
            list.Record(Lan, new DiscoveryReply(1, id, Game));
        }

        Assert.HasCount(LanHostList.Capacity, list.Hosts);
    }

    [TestMethod]
    public void Joinability_FollowsVersionMatchAndSlots()
    {
        var host = new DiscoveredHost(Lan, 1, Game with { OpenSlots = 2 });

        Assert.IsTrue(host.IsJoinable(2));
        Assert.IsFalse(host.IsJoinable(3));
        Assert.IsFalse((host with { Game = host.Game with { IsInGame = true } }).IsJoinable(1));
        var other = host with { Game = host.Game with { ProtocolVersion = ProtocolVersion.Current + 1 } };
        Assert.IsFalse(other.IsCompatible);
        Assert.IsFalse(other.IsJoinable(1));
    }
}
