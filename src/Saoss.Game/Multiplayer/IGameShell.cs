using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Audio;
using Saoss.Game.Screens;
using Saoss.Game.Settings;

namespace Saoss.Game.Multiplayer;

/// <summary>What the network game screens need from the running game: graphics, settings, audio and screen switching.</summary>
public interface IGameShell
{
    /// <summary>The graphics device.</summary>
    GraphicsDevice GraphicsDevice { get; }

    /// <summary>Shared textures, font and meshes.</summary>
    GameAssets Assets { get; }

    /// <summary>The current settings.</summary>
    GameSettings Settings { get; }

    /// <summary>The game's audio, or null when silent.</summary>
    GameAudio? Audio { get; }

    /// <summary>Window sizes offered in the settings menus.</summary>
    IReadOnlyList<Resolution> Resolutions { get; }

    /// <summary>
    /// Makes <paramref name="screen"/> the active screen; the previous one is disposed unless
    /// <paramref name="disposePrevious"/> is false (its owner then disposes it).
    /// </summary>
    void Show(IScreen screen, bool disposePrevious = true);

    /// <summary>Applies and saves settings.</summary>
    void ApplySettings(GameSettings settings);

    /// <summary>Shows the main menu.</summary>
    void ShowMainMenu();
}
