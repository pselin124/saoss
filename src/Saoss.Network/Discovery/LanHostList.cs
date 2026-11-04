using System.Net;

namespace Saoss.Network.Discovery;

/// <summary>
/// The games a <see cref="LanBrowser"/> has heard of, in the order they were found. A host that answers on several
/// addresses (a host on this machine answers on loopback and on the LAN) is listed once, preferring a LAN address;
/// a host silent for <see cref="Expiry"/> is dropped.
/// </summary>
public sealed class LanHostList
{
    /// <summary>Most hosts listed, so a flood of fake replies cannot grow the list without bound.</summary>
    public const int Capacity = 32;

    /// <summary>How long a host stays listed without answering; requests go out every second.</summary>
    public static readonly TimeSpan Expiry = TimeSpan.FromSeconds(3.5);

    private readonly TimeProvider _time;
    private readonly List<DiscoveredHost> _hosts = [];
    private readonly List<long> _lastSeen = [];

    /// <summary>Creates an empty list; <paramref name="time"/> defaults to the system clock.</summary>
    public LanHostList(TimeProvider? time = null) => _time = time ?? TimeProvider.System;

    /// <summary>The hosts heard of recently.</summary>
    public IReadOnlyList<DiscoveredHost> Hosts => _hosts;

    /// <summary>Adds or refreshes the host that answered from <paramref name="address"/>.</summary>
    public void Record(IPAddress address, DiscoveryReply reply)
    {
        var now = _time.GetTimestamp();
        var index = _hosts.FindIndex(h => h.InstanceId == reply.InstanceId);
        if (index < 0)
        {
            if (_hosts.Count < Capacity)
            {
                _hosts.Add(new DiscoveredHost(address, reply.InstanceId, reply.Game));
                _lastSeen.Add(now);
            }

            return;
        }

        var known = _hosts[index].Address;
        var keepKnown = IPAddress.IsLoopback(address) && !IPAddress.IsLoopback(known);
        _hosts[index] = new DiscoveredHost(keepKnown ? known : address, reply.InstanceId, reply.Game);
        _lastSeen[index] = now;
    }

    /// <summary>Drops the hosts that have been silent too long.</summary>
    public void RemoveExpired()
    {
        var now = _time.GetTimestamp();
        for (var i = _hosts.Count - 1; i >= 0; i--)
        {
            if (_time.GetElapsedTime(_lastSeen[i], now) > Expiry)
            {
                _hosts.RemoveAt(i);
                _lastSeen.RemoveAt(i);
            }
        }
    }
}
