using System.Globalization;
using Microsoft.Xna.Framework;
using Saoss.Game.Input;
using Saoss.Game.Multiplayer;
using Saoss.Game.Settings;
using Saoss.Network.Protocol;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The join menu (net.cpp joinMenu and drawJoinMPMenu): "change IP" with the current host below it, "players on
/// this computer:", "audio & visual settings", "change nicknames", "connect" and "exit"; "connect" is highlighted at
/// first. The host is remembered in the settings (the original's lasthost.txt). "find games on LAN" above them opens
/// the <see cref="LanGamesScreen"/>, which the original did not have.
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

    // The arrow plane is 15 units from the camera and the text 75, so text heights shrink to a fifth there.
    private const float ArrowPerTextUnit = 0.2f;
    private const int ConnectItem = 5;

    private static readonly float[] LabelY = [11f, 8f, 2f, -1f, -4f, -7f, -10f];
    private static readonly MenuPanel Panel = new(-2.25f, -0.825f, 0.25f, 0.84f);

    /// <summary>
    /// Builds the menu; <paramref name="connect"/> gets the host and TCP port and <paramref name="exit"/> goes back.
    /// With <paramref name="showLanGames"/> the list of games on the LAN opens at once.
    /// </summary>
    public static OptionMenuScreen Create(IGameShell shell, Action<string, int> connect, Action exit, bool showLanGames = false)
    {
        OptionMenuScreen? menu = null;
        OptionMenuEntry[] entries =
        [
            new("find games on LAN", _ => menu!.Child.Open(CreateLanGames(shell, connect, menu.Child.Close))),
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
            new("connect", _ => connect(shell.Settings.LastHost, ProtocolVersion.DefaultPort)),
            new("exit", _ => exit()),
        ];
        var layout = new OptionMenuLayout
        {
            LabelPosition = i => new Vector3(LabelX, LabelY[i], TextZ),
            ArrowX = ArrowX,
            ArrowY = i => LabelY[i] * ArrowPerTextUnit,
            Panel = Panel,
            Texts = [new MenuText(new Vector3(CurrentHostX, HostY, HostZ), () => $"current: {shell.Settings.LastHost}", Color.Red)],
        };
        menu = new OptionMenuScreen(shell.GraphicsDevice, shell.Assets, layout, entries, ConnectItem);
        if (showLanGames)
        {
            menu.Child.Open(CreateLanGames(shell, connect, menu.Child.Close));
        }

        return menu;
    }

    // Picking a game works like typing its address and choosing "connect".
    private static LanGamesScreen CreateLanGames(IGameShell shell, Action<string, int> connect, Action close) =>
        new(shell, (address, port) =>
        {
            shell.ApplySettings(shell.Settings with { LastHost = address });
            connect(address, port);
        }, close);

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
