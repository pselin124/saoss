namespace Saoss.Game.Audio;

/// <summary>Picks the decoder for a music file by its extension.</summary>
public static class MusicDecoders
{
    /// <summary>Opens <paramref name="path"/> (.ogg or .mp3); throws <see cref="NotSupportedException"/> otherwise.</summary>
    public static IMusicDecoder Open(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        MusicPlaylist.OggExtension => new VorbisMusicDecoder(path),
        MusicPlaylist.Mp3Extension => new Mp3MusicDecoder(path),
        _ => throw new NotSupportedException($"Unsupported music file: {path}"),
    };
}
