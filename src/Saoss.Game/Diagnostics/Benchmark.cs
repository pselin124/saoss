using System.Globalization;

namespace Saoss.Game.Diagnostics;

/// <summary>Runs a <see cref="BenchmarkOptions"/> benchmark over the frames the game reports.</summary>
/// <param name="options">What to measure and where to write the result.</param>
public sealed class Benchmark(BenchmarkOptions options)
{
    private readonly FrameRecorder _recorder = new();
    private bool _isMeasuring;
    private bool _isFinished;

    /// <summary>Adds a frame; returns true once the measured time is over and the report has been written.</summary>
    /// <param name="sample">The finished frame.</param>
    /// <param name="elapsed">Time since the game started.</param>
    public bool AddFrame(FrameSample sample, TimeSpan elapsed)
    {
        // The game may draw another frame while it exits; the report is written once.
        if (_isFinished)
        {
            return true;
        }

        if (!_isMeasuring)
        {
            if (elapsed.TotalSeconds >= BenchmarkOptions.WarmUpSeconds)
            {
                _isMeasuring = true;
                _recorder.Start();
            }

            return false;
        }

        _recorder.Add(sample);
        if (_recorder.Seconds < options.Seconds)
        {
            return false;
        }

        Write(_recorder.Report());
        _isFinished = true;
        return true;
    }

    private void Write(FrameReport report)
    {
        var line = string.Create(CultureInfo.InvariantCulture, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {options.Label}: {report}");
        Console.WriteLine(line);
        try
        {
            File.AppendAllText(options.OutputPath, line + Environment.NewLine);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine(e.Message);
        }
    }
}
