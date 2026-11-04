using System.Globalization;
using Microsoft.Xna.Framework;
using Saoss.Game.Input;
using Saoss.Game.Multiplayer;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The join menu (net.cpp joinMenu and drawJoinMPMenu): "change IP" with the current host below it, "players on
/// this computer:", "audio & visual settings", "change nicknames", "connect" and "exit"; "connect" is highlighted at
/// first. The host is remembered in the settings (the original's lasthost.txt).
/// </summary>
public static class JoinSetupMenu
{
    private const float LabelX = -30f;
    private const float CountX = -2f;
    private const float TextZ = -60f;
    private const float HostZ = -80f;
    private const float HostY = 6.5f;
    private const float CurrentHostX = -37f;
    private const float ArrowX = -7.2f;
    private const float FirstArrowY = 1.6f;
    private const float ArrowTopY = 1f;
    private const float ArrowStep = 0.6f;
    private const int ConnectItem = 4;

    private static readonly float[] LabelY = [8f, 2f, -1f, -4f, -7f, -10f];
    private static readonly MenuPanel Panel = new(-2.25f, -0.825f, 0.25f, 0.675f);

    /// <summary>Builds the menu; <paramref name="connect"/> gets the host and <paramref name="exit"/> goes back.</summary>
    public static OptionMenuScreen Create(IGameShell shell, Action<string> connect, Action exit)
    {
        OptionMenuScreen? menu = null;
        OptionMenuEntry[] entries =
        [
            new("change IP", _ => menu!.Child.Open(CreateHostEntry(shell, menu.Child.Close))),
            new("players on this computer:", direction => shell.ApplySettings(shell.Settings with
            {
                NetworkPlayerCount = HostSetupMenu.StepPlayerCount(shell.Settings.NetworkPlayerCount, direction),
            }))
            {
                Value = () => shell.Settings.NetworkPlayerCount.ToString(CultureInfo.InvariantCulture),
                ValueX = CountX,
            },
            new("audio & visual settings", _ => menu!.Child.Open(NetworkSettingsMenu.Create(shell, new NetworkSettingsMenuOptions(menu.Child.Close)))),
            new("change nicknames", _ => menu!.Child.Open(NicknameMenu.Create(shell, menu.Child.Close))),
            new("connect", _ => connect(shell.Settings.LastHost)),
            new("exit", _ => exit()),
        ];
        var layout = new OptionMenuLayout
        {
            LabelPosition = i => new Vector3(LabelX, LabelY[i], TextZ),
            ArrowX = ArrowX,
            ArrowY = i => i == 0 ? FirstArrowY : ArrowTopY - (ArrowStep * i),
            Panel = Panel,
            Texts = [new MenuText(new Vector3(CurrentHostX, HostY, HostZ), () => $"current: {shell.Settings.LastHost}", Color.Red)],
        };
        menu = new OptionMenuScreen(shell.GraphicsDevice, shell.Assets, layout, entries, ConnectItem);
        return menu;
    }

    private static TextEntryScreen CreateHostEntry(IGameShell shell, Action close)
    {
        var buffer = new TextInputBuffer(HostAddress.MaxLength, HostAddress.Accept);
        buffer.SetText(shell.Settings.LastHost);
        return new TextEntryScreen(shell.GraphicsDevice, shell.Assets, "enter IP address or host name:", buffer, text =>
        {
            if (HostAddress.IsValid(text))
            {
                shell.ApplySettings(shell.Settings with { LastHost = text });
            }

            close();
        });
    }
}
