using System.Net.Sockets;

namespace Saoss.Network.Discovery;

/// <summary>
/// The host side of LAN discovery: answers discovery requests on UDP <see cref="DiscoveryProtocol.Port"/> with a
/// description of the game, during matches too (marked as in game, so joiners see it but cannot pick it). Replies
/// go only to the asker and are rate-limited. Call <see cref="Update"/> every frame from the game thread.
/// </summary>
public sealed class LanAnnouncer : IDisposable
{
    private readonly DiscoverySocket _socket;
    private readonly ReplyRateLimiter _limiter;
    private readonly int _instanceId = Random.Shared.Next();

    private LanAnnouncer(DiscoverySocket socket, TimeProvider? time)
    {
        _socket = socket;
        _limiter = new ReplyRateLimiter(time: time);
    }

    /// <summary>The UDP port listened on.</summary>
    public int Port => _socket.Port;

    /// <summary>
    /// Starts listening; null when the port cannot be used, and the game is then just not discoverable. The port is
    /// shared, so a second game on the same machine does not fail.
    /// </summary>
    public static LanAnnouncer? TryStart(int port = DiscoveryProtocol.Port, TimeProvider? time = null)
    {
        try
        {
            return new LanAnnouncer(new DiscoverySocket(port, shareAddress: true), time);
        }
        catch (SocketException)
        {
            return null;
        }
    }

    /// <summary>Answers waiting requests; <paramref name="describe"/> is called only when there is one to answer.</summary>
    public void Update(Func<LanGameInfo> describe)
    {
        LanGameInfo? game = null;
        _socket.Poll((from, packet) =>
        {
            if (DiscoveryCodec.TryDecodeRequest(packet, out var token) && _limiter.TryAcquire(from.Address))
            {
                game ??= describe();
                _socket.Send(DiscoveryCodec.EncodeReply(token, _instanceId, game), from);
            }
        });
    }

    /// <inheritdoc/>
    public void Dispose() => _socket.Dispose();
}
