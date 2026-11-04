using System.Net;
using Saoss.Network.Discovery;

namespace Saoss.Tests.Network.Discovery;

[TestClass]
public sealed class ReplyRateLimiterTests
{
    private static readonly IPAddress First = IPAddress.Parse("192.168.1.2");
    private static readonly IPAddress Second = IPAddress.Parse("192.168.1.3");

    [TestMethod]
    public void EachSource_GetsItsShareEverySecond()
    {
        var clock = new ManualClock();
        var limiter = new ReplyRateLimiter(perSource: 3, total: 100, clock);

        Assert.AreEqual(3, Allowed(limiter, First, 10));
        Assert.AreEqual(3, Allowed(limiter, Second, 10));

        clock.Advance(TimeSpan.FromMilliseconds(999));
        Assert.AreEqual(0, Allowed(limiter, First, 10));
        clock.Advance(TimeSpan.FromMilliseconds(1));
        Assert.AreEqual(3, Allowed(limiter, First, 10));
    }

    [TestMethod]
    public void AllSources_ShareATotal()
    {
        var limiter = new ReplyRateLimiter(perSource: 3, total: 5, new ManualClock());

        Assert.AreEqual(3, Allowed(limiter, First, 10));
        Assert.AreEqual(2, Allowed(limiter, Second, 10));
        Assert.AreEqual(0, Allowed(limiter, IPAddress.Loopback, 10));
    }

    private static int Allowed(ReplyRateLimiter limiter, IPAddress source, int attempts) =>
        Enumerable.Range(0, attempts).Count(_ => limiter.TryAcquire(source));
}
