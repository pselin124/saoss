using NLayer;

namespace Saoss.Game.Audio;

/// <summary>Decodes an MP3 file with NLayer.</summary>
public sealed class Mp3MusicDecoder : IMusicDecoder
{
    private readonly MpegFile _file;

    /// <summary>Opens <paramref name="path"/>.</summary>
    public Mp3MusicDecoder(string path) => _file = new MpegFile(path);

    /// <inheritdoc/>
    public int SampleRate => _file.SampleRate;

    /// <inheritdoc/>
    public int Channels => _file.Channels;

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer, int offset, int count) => _file.ReadSamples(buffer, offset, count);

    /// <inheritdoc/>
    public void Dispose() => _file.Dispose();
}
