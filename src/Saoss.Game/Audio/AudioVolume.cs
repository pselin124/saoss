namespace Saoss.Game.Audio;

/// <summary>
/// Volume math shared by effects and music. The original passed volume * 256 to FMOD, i.e. a linear gain where
/// 1.0 is full volume, which is what MonoGame's 0-1 volume is too.
/// </summary>
public static class AudioVolume
{
    private const float FullSetting = 100f;

    /// <summary>
    /// Gain of a ship sound: its relative volume times the sound volume setting (0-100) / 100, clamped to 0-1 like
    /// FMOD clamped the channel volume to 255.
    /// </summary>
    public static float Effect(float relativeVolume, int soundVolume) =>
        Math.Clamp(relativeVolume * soundVolume / FullSetting, 0f, 1f);

    /// <summary>Gain of the music: the music volume setting (0-100) / 100.</summary>
    public static float Music(int musicVolume) => Math.Clamp(musicVolume / FullSetting, 0f, 1f);
}
