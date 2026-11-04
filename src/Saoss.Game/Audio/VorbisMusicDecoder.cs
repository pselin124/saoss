using NVorbis;

namespace Saoss.Game.Audio;

/// <summary>Decodes an Ogg Vorbis file with NVorbis.</summary>
public sealed class VorbisMusicDecoder : IMusicDecoder
{
    private readonly VorbisReader _reader;

    /// <summary>Opens <paramref name="path"/> and reads its headers.</summary>
    public VorbisMusicDecoder(string path) => _reader = new VorbisReader(path);

    /// <inheritdoc/>
    public int SampleRate => _reader.SampleRate;

    /// <inheritdoc/>
    public int Channels => _reader.Channels;

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer, int offset, int count) => _reader.ReadSamples(buffer, offset, count);

    /// <inheritdoc/>
    public void Dispose() => _reader.Dispose();
}
