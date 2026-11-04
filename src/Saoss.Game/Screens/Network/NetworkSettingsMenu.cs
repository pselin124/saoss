using System.Globalization;
using Microsoft.Xna.Framework;
using Saoss.Game.Multiplayer;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The "game settings menu" of network play (netgamesets.cpp netGameSets and netGameSetsDraw), opened from the
/// join menu, the host settings and, with Escape, during a network match, which keeps running behind it. Changes
/// stay in the menu until "apply & save settings"; "resume" closes it and, during a match, "exit multiplayer" leaves
/// the game. Escape chooses the last entry, as in the original. Camera and detail apply to the next match.
/// </summary>
public static class NetworkSettingsMenu
{
    private const float LabelX = -22f;
    private const float TopY = 4f;
    private const float LineSpacing = 2f;
    private const float TextZ = -40f;
    private const float ValueX = 3f;
    private const float CrossX = 11f;
    private const float HeightX = 13f;
    private const float ArrowX = -7.3f;
    private const float ArrowTopY = 1f;
    private const float ArrowStep = 0.55f;
    private const float TitleSize = 1.4f;

    private static readonly Vector3 TitlePosition = new(-25f, 8f, TextZ);
    private static readonly MenuPanel Panel = new(-2.35f, -1.75f, 1.7f, 0.55f);

    /// <summary>Builds the menu on the current settings.</summary>
    public static OptionMenuScreen Create(IGameShell shell, NetworkSettingsMenuOptions options)
    {
        var settings = shell.Settings;
        var resolutions = shell.Resolutions;
        var resolutionIndex = ResolutionOptions.IndexOf(resolutions, new Resolution(settings.Width, settings.Height));
        void Change(Func<GameSettings, GameSettings> change) => settings = change(settings);

        var entries = new List<OptionMenuEntry>
        {
            new("resolution:", d =>
            {
                resolutionIndex = ResolutionOptions.Step(resolutionIndex, resolutions.Count, larger: d != MenuDirection.Left);
                Change(s => s with { Width = resolutions[resolutionIndex].Width, Height = resolutions[resolutionIndex].Height });
            })
            {
                Value = () => Number(settings.Width),
                ValueX = ValueX,
            },
            Toggle("fullscreen:", () => settings.Fullscreen, () => Change(s => s with { Fullscreen = !s.Fullscreen })),
            new("camera mode:", _ => Change(s => s with { CameraMode = s.CameraMode == CameraMode.Tilted ? CameraMode.TopDown : CameraMode.Tilted }))
            {
                Value = () => settings.CameraMode == CameraMode.Tilted ? "3d" : "2d",
                ValueX = ValueX,
            },
            Toggle("musics:", () => settings.MusicEnabled, () => Change(s => s with { MusicEnabled = !s.MusicEnabled })),
            Toggle("sound effects:", () => settings.SoundEffectsEnabled, () => Change(s => s with { SoundEffectsEnabled = !s.SoundEffectsEnabled })),
            new("music volume:", d =>
            {
                Change(s => s with { MusicVolume = SettingsSteps.Volume(s.MusicVolume, d != MenuDirection.Left, fine: false) });
                shell.Audio?.PreviewMusicVolume(settings.MusicVolume);
            })
            {
                Value = () => Number(settings.MusicVolume),
                ValueX = ValueX,
            },
            new("sound volume:", d => Change(s => s with { SoundVolume = SettingsSteps.Volume(s.SoundVolume, d != MenuDirection.Left, fine: false) }))
            {
                Value = () => Number(settings.SoundVolume),
                ValueX = ValueX,
            },
            new("fps:", d =>
            {
                if (options.CanChangeSpeed)
                {
                    Change(s => s with { TicksPerSecond = SettingsSteps.TicksPerSecond(s.TicksPerSecond, d != MenuDirection.Left) });
                }
            })
            {
                Value = () => Number(settings.TicksPerSecond),
                ValueX = ValueX,
            },
            new("detail:", d => Change(s => s with { Detail = SettingsSteps.Detail(s.Detail, d != MenuDirection.Left) }))
            {
                Value = () => Number(settings.Detail),
                ValueX = ValueX,
            },
            new("apply & save settings", _ => shell.ApplySettings(settings)),
            new("resume", _ => options.Resume()),
        };
        if (options.ExitMultiplayer is { } exit)
        {
            entries.Add(new OptionMenuEntry("exit multiplayer", _ => exit()));
        }

        var layout = new OptionMenuLayout
        {
            LabelPosition = i => new Vector3(LabelX, TopY - (LineSpacing * i), TextZ),
            ArrowX = ArrowX,
            ArrowY = i => ArrowTopY - (ArrowStep * i),
            ArrowScale = new Vector3(0.3f),
            WithSceneOffset = true,
            Panel = Panel,
            Texts =
            [
                new MenuText(TitlePosition, () => "game settings menu", Color.Blue, TitleSize),
                new MenuText(new Vector3(CrossX, TopY, TextZ), () => "x", Color.Red),
                new MenuText(new Vector3(HeightX, TopY, TextZ), () => Number(settings.Height), Color.Red),
            ],
        };
        return new OptionMenuScreen(shell.GraphicsDevice, shell.Assets, layout, entries);
    }

    private static OptionMenuEntry Toggle(string label, Func<bool> value, Action toggle) =>
        new(label, _ => toggle())
        {
            Value = () => value() ? "on" : "off",
            ValueX = ValueX,
        };

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
