using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens;

/// <summary>
/// The settings menu (Menu.cpp asetusmenu and drawAsetusmenu): red labels on black with the values beside them and
/// one rotating arrow. Up and Down move with wrap-around; Left and Right change the value (Enter acts like Right);
/// on "Player Setup" any of them opens the player setup. Holding a fire key (Shift) changes volumes in steps of 1.
/// Changes stay in the menu until "Apply and Save Settings" or "Apply, Save and Exit Menu"; Escape also applies,
/// saves and leaves, as in the original.
/// </summary>
public sealed class SettingsMenuScreen : IScreen, IDisposable
{
    private const float TitleX = -8f;
    private const float TitleY = 8f;
    private const float TitleZ = -8f;
    private const float LabelX = -20f;
    private const float ValueX = -4f;
    private const float ResolutionCrossX = 2f;
    private const float ResolutionHeightX = 5f;
    private const float RowTopY = 14f;
    private const float RowSpacing = 2f;
    private const float ArrowX = -6.7f;
    private const float ArrowTopY = 3.8f;
    private const float ArrowRowStep = 0.54667f;

    private static readonly SettingsMenuItem LastItem = Enum.GetValues<SettingsMenuItem>().Max();
    private static readonly Vector3 ArrowScale = new(0.3f);

    private static readonly string[] Labels =
    [
        "Resolution:",
        "Fullscreen:",
        "Players:",
        "Musics:",
        "Sound Effects:",
        "Credits:",
        "Fps:",
        "Music Volume:",
        "Sound Volume:",
        "Player Setup",
        "Camera Mode:",
        "Detail:",
        "Apply and Save Settings",
        "Apply, Save and Exit Menu",
    ];

    private readonly MenuScene _scene;
    private readonly IReadOnlyList<Resolution> _resolutions;
    private readonly Action<GameSettings> _apply;
    private readonly Action<GameSettings> _applyAndExit;
    private GameSettings _settings;
    private int _resolutionIndex;
    private PlayerSetupMenuScreen? _playerSetup;
    private float _arrowSpinDegrees;

    /// <summary>
    /// Opens the menu on <paramref name="settings"/>. <paramref name="apply"/> runs for "Apply and Save Settings";
    /// <paramref name="applyAndExit"/> for "Apply, Save and Exit Menu" and Escape.
    /// </summary>
    public SettingsMenuScreen(
        GraphicsDevice graphicsDevice,
        GameAssets assets,
        GameSettings settings,
        IReadOnlyList<Resolution> resolutions,
        Action<GameSettings> apply,
        Action<GameSettings> applyAndExit)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _settings = settings;
        _resolutions = resolutions;
        _resolutionIndex = ResolutionOptions.IndexOf(resolutions, new Resolution(settings.Width, settings.Height));
        _apply = apply;
        _applyAndExit = applyAndExit;
    }

    /// <summary>The highlighted entry.</summary>
    public SettingsMenuItem Selection { get; private set; }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (_playerSetup != null)
        {
            _playerSetup.Update(keyboard);
            return;
        }

        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        if (keyboard.WasPressed(Keys.Enter) || keyboard.WasPressed(Keys.Right))
        {
            Choose(increase: true, IsFineStepHeld(keyboard));
        }
        else if (keyboard.WasPressed(Keys.Left))
        {
            Choose(increase: false, IsFineStepHeld(keyboard));
        }
        else if (keyboard.WasPressed(Keys.Up))
        {
            Selection = Selection > 0 ? Selection - 1 : LastItem;
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            Selection = Selection < LastItem ? Selection + 1 : 0;
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            Selection = SettingsMenuItem.ApplySaveAndExit;
            Choose(increase: true, fine: false);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (_playerSetup != null)
        {
            _playerSetup.Draw();
            return;
        }

        _scene.Begin(withBackground: false);
        _scene.DrawText(new Vector3(TitleX, TitleY, TitleZ), "Settings", Color.Red);
        for (var i = 0; i < Labels.Length; i++)
        {
            DrawAt(LabelX, RowY((SettingsMenuItem)i), Labels[i]);
        }

        DrawValues();
        var arrowY = ArrowTopY - ((int)Selection * ArrowRowStep);
        _scene.DrawArrow(ArrowX, arrowY, ArrowScale, _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();

    private static float RowY(SettingsMenuItem item) => RowTopY - ((int)item * RowSpacing);

    private static string OnOff(bool value) => value ? "on" : "off";

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // Player 1's and player 2's fire keys, like the original.
    private static bool IsFineStepHeld(KeyboardTracker keyboard) =>
        keyboard.IsDown(PlayerKeyBindings.Defaults[0].Fire) || keyboard.IsDown(PlayerKeyBindings.Defaults[1].Fire);

    private void Choose(bool increase, bool fine)
    {
        switch (Selection)
        {
            case SettingsMenuItem.PlayerSetup:
                _playerSetup = new PlayerSetupMenuScreen(_scene, _settings, ClosePlayerSetup);
                break;
            case SettingsMenuItem.ApplyAndSave:
                _apply(_settings);
                break;
            case SettingsMenuItem.ApplySaveAndExit:
                _applyAndExit(_settings);
                break;
            case SettingsMenuItem.Resolution:
                _resolutionIndex = ResolutionOptions.Step(_resolutionIndex, _resolutions.Count, larger: increase);
                var resolution = _resolutions[_resolutionIndex];
                _settings = _settings with { Width = resolution.Width, Height = resolution.Height };
                break;
            default:
                _settings = ChangeValue(Selection, increase, fine);
                break;
        }
    }

    private GameSettings ChangeValue(SettingsMenuItem item, bool increase, bool fine)
    {
        var s = _settings;
        return item switch
        {
            SettingsMenuItem.Fullscreen => s with { Fullscreen = !s.Fullscreen },
            SettingsMenuItem.Players => s with { PlayerCount = SettingsSteps.PlayerCount(s.PlayerCount, increase) },
            SettingsMenuItem.Musics => s with { MusicEnabled = !s.MusicEnabled },
            SettingsMenuItem.SoundEffects => s with { SoundEffectsEnabled = !s.SoundEffectsEnabled },
            SettingsMenuItem.Credits => s with { Credits = SettingsSteps.Credits(s.Credits, increase) },
            SettingsMenuItem.Fps => s with { TicksPerSecond = SettingsSteps.TicksPerSecond(s.TicksPerSecond, increase) },
            SettingsMenuItem.MusicVolume => s with { MusicVolume = SettingsSteps.Volume(s.MusicVolume, increase, fine) },
            SettingsMenuItem.SoundVolume => s with { SoundVolume = SettingsSteps.Volume(s.SoundVolume, increase, fine) },
            SettingsMenuItem.CameraMode => s with { CameraMode = s.CameraMode == CameraMode.Tilted ? CameraMode.TopDown : CameraMode.Tilted },
            SettingsMenuItem.Detail => s with { Detail = SettingsSteps.Detail(s.Detail, increase) },
            _ => s,
        };
    }

    private void ClosePlayerSetup(GameSettings settings)
    {
        _settings = settings;
        _playerSetup = null;
    }

    private void DrawValues()
    {
        var s = _settings;
        DrawAt(ValueX, RowY(SettingsMenuItem.Resolution), Number(s.Width));
        DrawAt(ResolutionCrossX, RowY(SettingsMenuItem.Resolution), "x");
        DrawAt(ResolutionHeightX, RowY(SettingsMenuItem.Resolution), Number(s.Height));
        DrawValue(SettingsMenuItem.Fullscreen, OnOff(s.Fullscreen));
        DrawValue(SettingsMenuItem.Players, Number(s.PlayerCount));
        DrawValue(SettingsMenuItem.Musics, OnOff(s.MusicEnabled));
        DrawValue(SettingsMenuItem.SoundEffects, OnOff(s.SoundEffectsEnabled));
        DrawValue(SettingsMenuItem.Credits, Number(s.Credits));
        DrawValue(SettingsMenuItem.Fps, Number(s.TicksPerSecond));
        DrawValue(SettingsMenuItem.MusicVolume, Number(s.MusicVolume));
        DrawValue(SettingsMenuItem.SoundVolume, Number(s.SoundVolume));
        DrawValue(SettingsMenuItem.CameraMode, s.CameraMode == CameraMode.Tilted ? "3d" : "2d");
        DrawValue(SettingsMenuItem.Detail, Number(s.Detail));
    }

    private void DrawValue(SettingsMenuItem item, string text) => DrawAt(ValueX, RowY(item), text);

    private void DrawAt(float x, float y, string text) => _scene.DrawText(new Vector3(x, y, MenuScene.TextZ), text, Color.Red);
}
