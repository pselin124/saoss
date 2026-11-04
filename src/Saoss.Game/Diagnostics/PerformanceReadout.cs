namespace Saoss.Game.Diagnostics;

/// <summary>
/// The <c>--perf</c> readout: once a second the last second's frame statistics go to the window title and to
/// standard output.
/// </summary>
/// <param name="setTitle">Sets the window title.</param>
public sealed class PerformanceReadout(Action<string> setTitle)
{
    private const double IntervalSeconds = 1;

    private readonly FrameRecorder _recorder = CreateStarted();

    /// <summary>Adds a finished frame and shows the statistics when a second is full.</summary>
    public void AddFrame(FrameSample sample)
    {
        _recorder.Add(sample);
        if (_recorder.Seconds < IntervalSeconds)
        {
            return;
        }

        var report = _recorder.Report().ToString();
        setTitle($"Saoss - {report}");
        Console.WriteLine(report);
        _recorder.Start();
    }

    private static FrameRecorder CreateStarted()
    {
        var recorder = new FrameRecorder();
        recorder.Start();
        return recorder;
    }
}
