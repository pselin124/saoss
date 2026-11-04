namespace Saoss.Game.Audio;

/// <summary>
/// Incremental decoder of one compressed music file into interleaved float samples.
/// </summary>
public interface IMusicDecoder : IDisposable
{
    /// <summary>Samples per second per channel.</summary>
    int SampleRate { get; }

    /// <summary>Number of interleaved channels.</summary>
    int Channels { get; }

    /// <summary>
    /// Decodes up to <paramref name="count"/> interleaved samples (-1 to 1) into <paramref name="buffer"/> from
    /// <paramref name="offset"/>; returns how many were written, 0 at the end of the file.
    /// </summary>
    int ReadSamples(float[] buffer, int offset, int count);
}
