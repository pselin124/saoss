using System.Diagnostics;

namespace Saoss.Game.Diagnostics;

/// <summary>Collects frame samples from <see cref="Start"/> on and summarizes them, with the garbage collections meanwhile.</summary>
public sealed class FrameRecorder
{
    private readonly List<FrameSample> _samples = [];
    private readonly Stopwatch _clock = new();
    private (int Gen0, int Gen1, int Gen2) _collectionsAtStart;

    /// <summary>Seconds since <see cref="Start"/>.</summary>
    public double Seconds => _clock.Elapsed.TotalSeconds;

    /// <summary>Forgets earlier samples and starts a new run.</summary>
    public void Start()
    {
        _samples.Clear();
        _collectionsAtStart = Collections();
        _clock.Restart();
    }

    /// <summary>Adds a finished frame.</summary>
    public void Add(FrameSample sample) => _samples.Add(sample);

    /// <summary>Summarizes the frames since <see cref="Start"/>.</summary>
    public FrameReport Report()
    {
        var now = Collections();
        return FrameReport.From(_samples, Seconds, (now.Gen0 - _collectionsAtStart.Gen0, now.Gen1 - _collectionsAtStart.Gen1, now.Gen2 - _collectionsAtStart.Gen2));
    }

    private static (int Gen0, int Gen1, int Gen2) Collections() => (GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
}
