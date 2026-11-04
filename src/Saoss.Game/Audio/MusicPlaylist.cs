namespace Saoss.Game.Audio;

/// <summary>
/// The background music: an optional start track, then random picks from the music files found in the music
/// folder, like the original's musics\StartMusic\start.mp3 and its musics\*.mp3/*.ogg scan. Players can drop their
/// own .mp3 and .ogg files in the folder. The original's tracker formats (.xm, .s3m, .mod, .it) are not played.
/// </summary>
public sealed class MusicPlaylist
{
    /// <summary>Extension of MP3 files.</summary>
    public const string Mp3Extension = ".mp3";

    /// <summary>Extension of Ogg Vorbis files.</summary>
    public const string OggExtension = ".ogg";

    private const string StartFolder = "Start";
    private const string StartFile = "start.mp3";

    // The original listed the MP3s before the OGGs.
    private static readonly string[] SupportedExtensions = [Mp3Extension, OggExtension];

    /// <summary>Creates a playlist of <paramref name="tracks"/> that begins with <paramref name="startTrack"/>.</summary>
    public MusicPlaylist(string? startTrack, IReadOnlyList<string> tracks)
    {
        StartTrack = startTrack;
        Tracks = tracks;
    }

    /// <summary>Track played first at startup, or null when there is none.</summary>
    public string? StartTrack { get; }

    /// <summary>Tracks picked at random after the start track.</summary>
    public IReadOnlyList<string> Tracks { get; }

    /// <summary>
    /// Builds the playlist from <paramref name="musicFolder"/>: its top-level music files, and Start/start.mp3 as
    /// the start track. A missing folder gives an empty playlist.
    /// </summary>
    public static MusicPlaylist Scan(string musicFolder)
    {
        if (!Directory.Exists(musicFolder))
        {
            return new MusicPlaylist(null, []);
        }

        var start = Path.Combine(musicFolder, StartFolder, StartFile);
        return new MusicPlaylist(File.Exists(start) ? start : null, SelectTracks(Directory.EnumerateFiles(musicFolder)));
    }

    /// <summary>
    /// Keeps the playable files of <paramref name="files"/> (by extension, any case): MP3s first, then OGGs, each
    /// sorted by file name.
    /// </summary>
    public static IReadOnlyList<string> SelectTracks(IEnumerable<string> files) =>
    [
        .. files
            .Where(file => ExtensionRank(file) >= 0)
            .OrderBy(ExtensionRank)
            .ThenBy(file => Path.GetFileName(file), StringComparer.OrdinalIgnoreCase),
    ];

    /// <summary>The first track to play at startup: the start track, else a random one; null when there are none.</summary>
    public string? FirstTrack(Random random) => StartTrack ?? NextRandom(random);

    /// <summary>A uniformly random track (it may repeat, as in the original), or null when there are none.</summary>
    public string? NextRandom(Random random) => Tracks.Count == 0 ? null : Tracks[random.Next(Tracks.Count)];

    private static int ExtensionRank(string file) =>
        Array.FindIndex(SupportedExtensions, extension => string.Equals(Path.GetExtension(file), extension, StringComparison.OrdinalIgnoreCase));
}
