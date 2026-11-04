namespace Saoss.Game.Settings;

/// <summary>
/// How Left and Right change each value in the settings menus (Menu.cpp asetusmenu and playerMenu).
/// Every value wraps around at the ends of its range. <c>increase</c> is true for Right and Enter.
/// </summary>
public static class SettingsSteps
{
    private const int CreditsStep = 25;
    private const int FastTicksThreshold = 100;
    private const int SlowTicksStep = 5;
    private const int FastTicksStep = 10;
    private const int VolumeStep = 5;
    private const int FineVolumeStep = 1;

    /// <summary>Next player count, 2 to 16.</summary>
    public static int PlayerCount(int count, bool increase) => increase
        ? (count >= GameSettings.MaxPlayers ? GameSettings.MinPlayers : count + 1)
        : (count <= GameSettings.MinPlayers ? GameSettings.MaxPlayers : count - 1);

    /// <summary>Next starting credits in steps of 25, wrapping between 25 and 1000.</summary>
    public static int Credits(int credits, bool increase)
    {
        if (increase)
        {
            credits += CreditsStep;
            return credits > GameSettings.MaxCredits ? CreditsStep : credits;
        }

        credits -= CreditsStep;
        return credits < CreditsStep ? GameSettings.MaxCredits : credits;
    }

    /// <summary>Next game speed: steps of 5 below 100 ticks per second and of 10 from there up to 200.</summary>
    public static int TicksPerSecond(int ticks, bool increase)
    {
        if (increase)
        {
            ticks += ticks >= FastTicksThreshold ? FastTicksStep : SlowTicksStep;
            return ticks > GameSettings.MaxTicksPerSecond ? GameSettings.MinTicksPerSecond : ticks;
        }

        ticks -= ticks >= FastTicksThreshold + FastTicksStep ? FastTicksStep : SlowTicksStep;
        return ticks < GameSettings.MinTicksPerSecond ? GameSettings.MaxTicksPerSecond : ticks;
    }

    /// <summary>Next volume in steps of 5, or of 1 while a fire key is held (<paramref name="fine"/>).</summary>
    public static int Volume(int volume, bool increase, bool fine)
    {
        var step = fine ? FineVolumeStep : VolumeStep;
        if (increase)
        {
            volume += step;
            return volume > GameSettings.MaxVolume ? 0 : volume;
        }

        volume -= step;
        return volume < 0 ? GameSettings.MaxVolume : volume;
    }

    /// <summary>Next detail level, 0 to <see cref="GameSettings.MaxDetail"/>.</summary>
    public static int Detail(int detail, bool increase) => increase
        ? (detail >= GameSettings.MaxDetail ? 0 : detail + 1)
        : (detail <= 0 ? GameSettings.MaxDetail : detail - 1);

    /// <summary>Next team; the player setup only counts up.</summary>
    public static int Team(int team) => team >= GameSettings.MaxTeam ? 0 : team + 1;
}
