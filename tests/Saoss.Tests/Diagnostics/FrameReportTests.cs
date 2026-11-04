using Saoss.Game.Diagnostics;

namespace Saoss.Tests.Diagnostics;

/// <summary>Tests the frame time statistics of the --perf and --bench developer options.</summary>
[TestClass]
public sealed class FrameReportTests
{
    [TestMethod]
    public void Percentile_UsesTheNearestRank()
    {
        double[] sorted = [.. Enumerable.Range(1, 100).Select(i => (double)i)];

        Assert.AreEqual(99, FrameReport.Percentile(sorted, 0.99));
        Assert.AreEqual(50, FrameReport.Percentile(sorted, 0.5));
        Assert.AreEqual(100, FrameReport.Percentile(sorted, 1));
        Assert.AreEqual(0, FrameReport.Percentile([], 0.99));
    }

    [TestMethod]
    public void From_AveragesTheSamples()
    {
        FrameSample[] samples =
        [
            new(FrameMs: 10, UpdateMs: 1, DrawMs: 8, DrawCalls: 100, Vertices: 1000, AllocatedBytes: 2048),
            new(FrameMs: 20, UpdateMs: 3, DrawMs: 16, DrawCalls: 300, Vertices: 3000, AllocatedBytes: 0),
        ];

        var report = FrameReport.From(samples, seconds: 0.5, collections: (1, 0, 0));

        Assert.AreEqual(2, report.Frames);
        Assert.AreEqual(4, report.FramesPerSecond);
        Assert.AreEqual(15, report.AverageFrameMs);
        Assert.AreEqual(20, report.P99FrameMs);
        Assert.AreEqual(20, report.MaxFrameMs);
        Assert.AreEqual(2, report.AverageUpdateMs);
        Assert.AreEqual(12, report.AverageDrawMs);
        Assert.AreEqual(200, report.AverageDrawCalls);
        Assert.AreEqual(2000, report.AverageVertices);
        Assert.AreEqual(1024, report.AllocatedBytesPerFrame);
        StringAssert.Contains(report.ToString(), "p99 20.00 ms");
    }

    [TestMethod]
    public void From_NoSamples_GivesZeros()
    {
        var report = FrameReport.From([], seconds: 0, collections: (0, 0, 0));

        Assert.AreEqual(0, report.Frames);
        Assert.AreEqual(0, report.FramesPerSecond);
        Assert.AreEqual(0, report.AverageFrameMs);
    }
}
