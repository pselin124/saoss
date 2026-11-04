using System.Diagnostics;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Diagnostics;

/// <summary>
/// Developer timing of the game loop: the game reports where updates and frames start and end, and each finished
/// frame becomes a <see cref="FrameSample"/>.
/// </summary>
public sealed class PerformanceMonitor
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private long _updateStarted;
    private long _updateTicks;
    private long _drawStarted;
    private long _lastFrameEnded;
    private long _allocatedAtLastFrame = GC.GetAllocatedBytesForCurrentThread();

    /// <summary>Time since the monitor was created.</summary>
    public TimeSpan Elapsed => _clock.Elapsed;

    /// <summary>An update starts.</summary>
    public void BeginUpdate() => _updateStarted = _clock.ElapsedTicks;

    /// <summary>The update started last ends.</summary>
    public void EndUpdate() => _updateTicks += _clock.ElapsedTicks - _updateStarted;

    /// <summary>Drawing of a frame starts.</summary>
    public void BeginDraw() => _drawStarted = _clock.ElapsedTicks;

    /// <summary>The frame has been presented: returns its sample and starts counting the next frame.</summary>
    public FrameSample EndFrame()
    {
        var now = _clock.ElapsedTicks;
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        var sample = new FrameSample(
            ToMilliseconds(now - _lastFrameEnded),
            ToMilliseconds(_updateTicks),
            ToMilliseconds(now - _drawStarted),
            GlFrameCounters.DrawCalls,
            GlFrameCounters.Vertices,
            allocated - _allocatedAtLastFrame);
        _lastFrameEnded = now;
        _updateTicks = 0;
        _allocatedAtLastFrame = allocated;
        GlFrameCounters.Reset();
        return sample;
    }

    private static double ToMilliseconds(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;
}
