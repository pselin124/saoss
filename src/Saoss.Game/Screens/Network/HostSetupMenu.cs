using System.Globalization;
using Microsoft.Xna.Framework;
using Saoss.Game.Multiplayer;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The first host menu (net.cpp montakoHostia and drawMontakoHostia): "players from this machine:" with the count,
/// "change nicknames", "next" and "exit"; "next" is highlighted at first. Left lowers and Right or Enter raises the
/// count, 1-4 with wrap-around (the original also allowed 0, a host without players).
/// </summary>
public static class HostSetupMenu
{
    private const float LabelX = -32f;
    private const float CountX = -3f;
    private const float TopY = 8f;
    private const float LineSpacing = 3.5f;
    private const float TextZ = -60f;
    private const float ArrowX = -7.5f;
    private const float ArrowTopY = 1.6f;
    private const float ArrowStep = 0.7f;
    private const int NextItem = 2;

    private static readonly MenuPanel Panel = new(-2.25f, -0.375f, 0f, 0.625f);

    /// <summary>Builds the menu; <paramref name="next"/> opens the lobby and <paramref name="exit"/> goes back.</summary>
    public static OptionMenuScreen Create(IGameShell shell, Action next, Action exit)
    {
        OptionMenuScreen? menu = null;
        OptionMenuEntry[] entries =
        [
            new("players from this machine:", direction => shell.ApplySettings(shell.Settings with
            {
                NetworkPlayerCount = StepPlayerCount(shell.Settings.NetworkPlayerCount, direction),
            }))
            {
                Value = () => shell.Settings.NetworkPlayerCount.ToString(CultureInfo.InvariantCulture),
                ValueX = CountX,
            },
            new("change nicknames", _ => menu!.Child.Open(NicknameMenu.Create(shell, menu.Child.Close))),
            new("next", _ => next()),
            new("exit", _ => exit()),
        ];
        menu = new OptionMenuScreen(shell.GraphicsDevice, shell.Assets, Layout(), entries, NextItem);
        return menu;
    }

    /// <summary>The next player count in <paramref name="direction"/>, 1-4 with wrap-around.</summary>
    public static int StepPlayerCount(int count, MenuDirection direction) => direction == MenuDirection.Left
        ? (count > 1 ? count - 1 : GameSettings.MaxNetworkPlayers)
        : (count < GameSettings.MaxNetworkPlayers ? count + 1 : 1);

    private static OptionMenuLayout Layout() => new()
    {
        LabelPosition = i => new Vector3(LabelX, TopY - (LineSpacing * i), TextZ),
        ArrowX = ArrowX,
        ArrowY = i => ArrowTopY - (ArrowStep * i),
        Panel = Panel,
    };
}
