using Microsoft.Xna.Framework.Audio;
using Saoss.Core.Simulation;
using Saoss.Game.Settings;

namespace Saoss.Game.Audio;

/// <summary>
/// All of the game's audio: ship sound effects and background music, configured from <see cref="GameSettings"/>.
/// Created through <see cref="TryCreate"/>, which returns null when audio is unavailable so the game runs silently.
/// </summary>
public sealed class GameAudio : IDisposable
{
    private const string SoundsFolder = "Sounds";
    private const string MusicFolder = "Music";

    private readonly SoundBank _bank;
    private readonly ShipSoundPlayer _ships;
    private readonly MusicPlayer _music;

    private GameAudio(SoundBank bank, MusicPlayer music)
    {
        _bank = bank;
        _ships = new ShipSoundPlayer(bank);
        _music = music;
    }

    /// <summary>
    /// Loads the sounds and the playlist from the Content folder next to the executable, applies
    /// <paramref name="settings"/> and starts the start track when music is on. Returns null, after logging why,
    /// when there is no audio device or the sounds cannot be loaded.
    /// </summary>
    public static GameAudio? TryCreate(GameSettings settings)
    {
        var content = Path.Combine(AppContext.BaseDirectory, "Content");
        SoundBank? bank = null;
        try
        {
            bank = SoundBank.Load(Path.Combine(content, SoundsFolder));
            var audio = new GameAudio(bank, new MusicPlayer(MusicPlaylist.Scan(Path.Combine(content, MusicFolder)), new Random()));
            bank = null;
            audio.ApplyEffects(settings);
            audio._music.Volume = AudioVolume.Music(settings.MusicVolume);
            if (settings.MusicEnabled)
            {
                audio._music.PlayStartTrack();
            }

            return audio;
        }
        catch (Exception ex) when (ex is NoAudioHardwareException or DllNotFoundException or TypeInitializationException or IOException or InvalidOperationException)
        {
            bank?.Dispose();
            Console.Error.WriteLine($"Audio disabled: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Applies changed settings (the settings menu's Apply): effects on/off and volume, music volume, and music on
    /// (a random track starts) or off (the music stops).
    /// </summary>
    public void Apply(GameSettings settings)
    {
        ApplyEffects(settings);
        _music.Volume = AudioVolume.Music(settings.MusicVolume);
        if (settings.MusicEnabled && !_music.IsPlaying)
        {
            _music.PlayRandomTrack();
        }
        else if (!settings.MusicEnabled && _music.IsPlaying)
        {
            _music.Stop();
        }
    }

    /// <summary>Sets the music volume (0-100) at once, while it is being changed in the settings menu.</summary>
    public void PreviewMusicVolume(int musicVolume) => _music.Volume = AudioVolume.Music(musicVolume);

    /// <summary>Plays the sounds of the events of <paramref name="world"/>'s last step.</summary>
    public void PlayEvents(World world) => _ships.Play(world.Events);

    /// <summary>Stops the ships' looping sounds, when the match is left or paused.</summary>
    public void StopShipLoops() => _ships.StopAll();

    /// <summary>Streams the music and frees finished sounds; call once per update.</summary>
    public void Update()
    {
        _music.Update();
        _ships.Update();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _ships.Dispose();
        _music.Dispose();
        _bank.Dispose();
    }

    private void ApplyEffects(GameSettings settings)
    {
        _ships.Enabled = settings.SoundEffectsEnabled;
        _ships.Volume = settings.SoundVolume;
    }
}
