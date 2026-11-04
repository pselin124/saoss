using System.Net;
using System.Net.Sockets;

namespace Saoss.Network.Discovery;

/// <summary>
/// The joiner side of LAN discovery: while it exists it sends a request to every broadcast target once a second
/// and collects the replies into <see cref="Hosts"/>. Replies only fill the list; connecting is up to the player.
/// Call <see cref="Update"/> every frame from the game thread.
/// </summary>
public sealed class LanBrowser : IDisposable
{
    /// <summary>Time between requests.</summary>
    public static readonly TimeSpan RequestInterval = TimeSpan.FromSeconds(1);

    private readonly DiscoverySocket? _socket;
    private readonly IReadOnlyList<IPEndPoint> _targets;
    private readonly TimeProvider _time;
    private readonly LanHostList _hosts;
    private readonly byte[] _request;
    private readonly int _token = Random.Shared.Next();
    private long? _lastRequest;

    /// <summary>
    /// Creates the browser; requests go to <paramref name="targets"/>, by default the
    /// <see cref="BroadcastTargets"/> of the discovery port. Without a usable socket the list just stays empty.
    /// </summary>
    public LanBrowser(IReadOnlyList<IPEndPoint>? targets = null, TimeProvider? time = null)
    {
        _time = time ?? TimeProvider.System;
        _hosts = new LanHostList(_time);
        _targets = targets ?? BroadcastTargets.ForPort(DiscoveryProtocol.Port);
        _request = DiscoveryCodec.EncodeRequest(_token);
        try
        {
            _socket = new DiscoverySocket(0, shareAddress: false);
        }
        catch (SocketException)
        {
            _socket = null;
        }
    }

    /// <summary>The games heard of recently, in the order they were found.</summary>
    public IReadOnlyList<DiscoveredHost> Hosts => _hosts.Hosts;

    /// <summary>Sends the next request when due, takes in replies and forgets hosts that went silent.</summary>
    public void Update()
    {
        if (_socket == null)
        {
            return;
        }

        var now = _time.GetTimestamp();
        if (_lastRequest is not { } last || _time.GetElapsedTime(last, now) >= RequestInterval)
        {
            _lastRequest = now;
            foreach (var target in _targets)
            {
                _socket.Send(_request, target);
            }
        }

        _socket.Poll((from, packet) =>
        {
            if (DiscoveryCodec.TryDecodeReply(packet, out var reply) && reply.Token == _token)
            {
                _hosts.Record(from.Address, reply);
            }
        });
        _hosts.RemoveExpired();
    }

    /// <inheritdoc/>
    public void Dispose() => _socket?.Dispose();
}
