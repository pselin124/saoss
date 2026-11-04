using System.Net;

namespace Saoss.Network.Discovery;

/// <summary>
/// Limits the replies of a <see cref="LanAnnouncer"/> within each one-second window, per source address and in
/// total, so flooding it with (possibly spoofed) requests cannot turn the host into a traffic source.
/// </summary>
public sealed class ReplyRateLimiter
{
    /// <summary>Replies per source address and second; a joiner sends up to a few copies of each request.</summary>
    public const int DefaultPerSource = 8;

    /// <summary>Replies per second in total.</summary>
    public const int DefaultTotal = 64;

    private static readonly TimeSpan Window = TimeSpan.FromSeconds(1);

    private readonly int _perSource;
    private readonly int _total;
    private readonly TimeProvider _time;
    private readonly Dictionary<IPAddress, int> _counts = [];
    private long _windowStart;
    private int _used;

    /// <summary>Creates a limiter; <paramref name="time"/> defaults to the system clock.</summary>
    public ReplyRateLimiter(int perSource = DefaultPerSource, int total = DefaultTotal, TimeProvider? time = null)
    {
        _perSource = perSource;
        _total = total;
        _time = time ?? TimeProvider.System;
        _windowStart = _time.GetTimestamp();
    }

    /// <summary>Whether a reply to <paramref name="source"/> may be sent now; counts it when it may.</summary>
    public bool TryAcquire(IPAddress source)
    {
        var now = _time.GetTimestamp();
        if (_time.GetElapsedTime(_windowStart, now) >= Window)
        {
            _windowStart = now;
            _counts.Clear();
            _used = 0;
        }

        var count = _counts.GetValueOrDefault(source);
        if (_used >= _total || count >= _perSource)
        {
            return false;
        }

        _counts[source] = count + 1;
        _used++;
        return true;
    }
}
