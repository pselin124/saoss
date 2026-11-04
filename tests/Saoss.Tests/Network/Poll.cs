using System.Diagnostics;

namespace Saoss.Tests.Network;

/// <summary>Waits for asynchronous network results without fixed sleeps.</summary>
internal static class Poll
{
    /// <summary>Generous limit; loopback normally answers within milliseconds.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Calls <paramref name="pump"/> until <paramref name="condition"/> holds; fails after the timeout.</summary>
    public static void Until(Func<bool> condition, Action pump, string what, TimeSpan? timeout = null)
    {
        var watch = Stopwatch.StartNew();
        while (true)
        {
            pump();
            if (condition())
            {
                return;
            }

            if (watch.Elapsed > (timeout ?? DefaultTimeout))
            {
                Assert.Fail($"Timed out waiting for {what}.");
            }

            Thread.Sleep(1);
        }
    }
}
