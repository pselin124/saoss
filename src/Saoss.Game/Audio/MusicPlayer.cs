using Microsoft.Xna.Framework.Audio;

namespace Saoss.Game.Audio;

/// <summary>
/// Streams the background music: a track is decoded a few small buffers ahead of playback from <see cref="Update"/>,
/// so no file is ever decoded whole. When a track ends a random one follows, forever, like the original's stop
/// callback <c>test()</c>. A file that cannot be opened or decoded is skipped.
/// </summary>
public sealed class MusicPlayer : IDisposable
{
    private const int FramesPerBuffer = 4096;
    private const int BuffersAhead = 3;
    private const int MonoChannels = 1;
    private const int StereoChannels = 2;

    private readonly MusicPlaylist _playlist;
    private readonly Random _random;
    private IMusicDecoder? _decoder;
    private DynamicSoundEffectInstance? _instance;
    private float[] _samples = [];
    private byte[] _bytes = [];
    private bool _trackDecoded;
    private float _volume = 1f;

    /// <summary>Creates a stopped player for <paramref name="playlist"/>.</summary>
    public MusicPlayer(MusicPlaylist playlist, Random random)
    {
        _playlist = playlist;
        _random = random;
    }

    /// <summary>Whether a track is playing.</summary>
    public bool IsPlaying => _instance != null;

    /// <summary>Path of the playing track, or null.</summary>
    public string? CurrentTrack { get; private set; }

    /// <summary>Gain 0-1, applied to the playing track at once.</summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (_instance != null)
            {
                _instance.Volume = value;
            }
        }
    }

    /// <summary>Starts the playlist's first track (the start track) at program start.</summary>
    public void PlayStartTrack() => PlayFirstWorking(_playlist.FirstTrack(_random));

    /// <summary>Starts a random track, e.g. when music is turned on in the settings.</summary>
    public void PlayRandomTrack() => PlayFirstWorking(_playlist.NextRandom(_random));

    /// <summary>Stops the music.</summary>
    public void Stop() => CloseTrack();

    /// <summary>Keeps the playback buffers filled and moves on to a random track when the current one has ended.</summary>
    public void Update()
    {
        if (_instance == null)
        {
            return;
        }

        FillBuffers();
        if (_trackDecoded && _instance.PendingBufferCount == 0)
        {
            PlayRandomTrack();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => CloseTrack();

    // Gives up after as many failures as there are tracks, so a folder of broken files does not spin forever.
    private void PlayFirstWorking(string? track)
    {
        CloseTrack();
        for (var attempt = 0; track != null && attempt <= _playlist.Tracks.Count; attempt++)
        {
            if (TryOpen(track))
            {
                return;
            }

            track = _playlist.NextRandom(_random);
        }
    }

    /// <summary>
    /// Steps: open the decoder, create a streaming instance with the file's format, queue the first buffers, play.
    /// Returns false (and leaves the player stopped) when any step fails.
    /// </summary>
    private bool TryOpen(string track)
    {
        try
        {
            _decoder = MusicDecoders.Open(track);
            var channels = _decoder.Channels switch
            {
                MonoChannels => AudioChannels.Mono,
                StereoChannels => AudioChannels.Stereo,
                _ => throw new NotSupportedException($"{_decoder.Channels} channels"),
            };
            _instance = new DynamicSoundEffectInstance(_decoder.SampleRate, channels) { Volume = _volume };
            _samples = new float[FramesPerBuffer * _decoder.Channels];
            _bytes = new byte[_samples.Length * PcmConverter.BytesPerSample];
            _trackDecoded = false;
            CurrentTrack = track;
            FillBuffers();
            _instance.Play();
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Music files may be anything a player dropped in the folder; a bad one must not stop the game.
            Console.Error.WriteLine($"Cannot play music {track}: {ex.Message}");
            CloseTrack();
            return false;
        }
    }

    private void FillBuffers()
    {
        while (!_trackDecoded && _instance!.PendingBufferCount < BuffersAhead)
        {
            SubmitNextBuffer();
        }
    }

    private void SubmitNextBuffer()
    {
        var decoder = _decoder!;
        int read;
        try
        {
            read = decoder.ReadSamples(_samples, 0, _samples.Length);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // A corrupt tail ends the track early.
            Console.Error.WriteLine($"Music decoding failed in {CurrentTrack}: {ex.Message}");
            read = 0;
        }

        read -= read % decoder.Channels;
        if (read <= 0)
        {
            _trackDecoded = true;
            return;
        }

        PcmConverter.ToInt16(_samples.AsSpan(0, read), _bytes);
        _instance!.SubmitBuffer(_bytes, 0, read * PcmConverter.BytesPerSample);
    }

    private void CloseTrack()
    {
        if (_instance != null)
        {
            _instance.Stop();
            _instance.Dispose();
            _instance = null;
        }

        _decoder?.Dispose();
        _decoder = null;
        CurrentTrack = null;
    }
}
