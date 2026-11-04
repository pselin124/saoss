namespace Saoss.Game.Screens;

/// <summary>
/// Entries of the settings menu in menu order (Menu.cpp asetusmenu); the value is the line number.
/// </summary>
public enum SettingsMenuItem
{
    /// <summary>Window or fullscreen size.</summary>
    Resolution,

    /// <summary>Borderless fullscreen on or off.</summary>
    Fullscreen,

    /// <summary>Number of players in a local match.</summary>
    Players,

    /// <summary>Background music on or off.</summary>
    Music,

    /// <summary>Sound effects on or off.</summary>
    SoundEffects,

    /// <summary>Starting credits.</summary>
    Credits,

    /// <summary>Game speed in simulation ticks per second.</summary>
    Fps,

    /// <summary>Music volume.</summary>
    MusicVolume,

    /// <summary>Sound effect volume.</summary>
    SoundVolume,

    /// <summary>Opens the player setup (human or CPU, and team, of each player).</summary>
    PlayerSetup,

    /// <summary>Skill of the computer pilots; not in the original.</summary>
    AiLevel,

    /// <summary>Tilted ("3d") or top-down ("2d") match camera.</summary>
    CameraMode,

    /// <summary>Detail level.</summary>
    Detail,

    /// <summary>Applies and saves the settings and stays in the menu.</summary>
    ApplyAndSave,

    /// <summary>Applies and saves the settings and leaves the menu; Escape also does this.</summary>
    ApplySaveAndExit,
}
