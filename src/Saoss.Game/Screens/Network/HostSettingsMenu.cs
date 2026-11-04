using System.Globalization;
using Microsoft.Xna.Framework;
using Saoss.Core;
using Saoss.Game.Multiplayer;
using Saoss.Network.Lobby;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The host's settings (net.cpp hostSettingsMenu and drawHostSettingsMenu): "maximum players:", "credits / score limit:",
/// "audio &amp; visual settings", "game mode:" and "exit menu". Left lowers and Right raises a value; Enter raises the
/// player count and the mode but leaves the credits alone, as in the original. Values wrap around.
/// </summary>
public static class HostSettingsMenu
{
    private const float LabelX = -32f;
    private const float TextZ = -60f;
    private const float MaxPlayersX = -13f;
    private const float CreditsX = -9f;
    private const float ModeX = -20f;
    private const float ArrowX = -7.5f;

    private static readonly float[] LabelY = [8f, 4.5f, 1f, -2.5f, -6f];
    private static readonly float[] ArrowYs = [1.6f, 0.9f, 0.2f, -0.5f, -1.2f];
    private static readonly MenuPanel Panel = new(-2.25f, -0.55f, 1f, 0.625f);
    private static readonly Color ValueColor = new(0.2f, 0.2f, 1f);
    private static readonly GameMode[] Modes = Enum.GetValues<GameMode>();

    /// <summary>Builds the menu for <paramref name="lobby"/>; <paramref name="close"/> runs on "exit menu".</summary>
    public static OptionMenuScreen Create(IGameShell shell, LobbyState lobby, Action close)
    {
        OptionMenuScreen? menu = null;
        OptionMenuEntry[] entries =
        [
            new("maximum players:", d => StepMaxPlayers(lobby, up: d != MenuDirection.Left))
            {
                Value = () => lobby.MaxPlayers.ToString(CultureInfo.InvariantCulture),
                ValueX = MaxPlayersX,
                ValueColor = ValueColor,
            },
            new("credits / score limit:", d =>
            {
                if (d != MenuDirection.Enter)
                {
                    lobby.StepCredits(up: d == MenuDirection.Right);
                }
            })
            {
                Value = () => lobby.Credits.ToString(CultureInfo.InvariantCulture),
                ValueX = CreditsX,
                ValueColor = ValueColor,
            },
            new("audio & visual settings", _ => menu!.Child.Open(NetworkSettingsMenu.Create(shell, new NetworkSettingsMenuOptions(menu.Child.Close) { CanChangeSpeed = true }))),
            new("game mode:", d => lobby.SetMode(StepMode(lobby.Mode, up: d != MenuDirection.Left)))
            {
                Value = () => LobbyTexts.Mode(lobby.Mode),
                ValueX = ModeX,
                ValueColor = ValueColor,
            },
            new("exit menu", _ => close()),
        ];
        var layout = new OptionMenuLayout
        {
            LabelPosition = i => new Vector3(LabelX, LabelY[i], TextZ),
            ArrowX = ArrowX,
            ArrowY = i => ArrowYs[i],
            Panel = Panel,
        };
        menu = new OptionMenuScreen(shell.GraphicsDevice, shell.Assets, layout, entries);
        return menu;
    }

    /// <summary>
    /// Moves the player limit one step (2-16, wrapping), skipping limits the human players would not fit in.
    /// </summary>
    public static void StepMaxPlayers(LobbyState lobby, bool up)
    {
        var count = LobbyState.SlotCount - LobbyState.MinPlayers + 1;
        var value = lobby.MaxPlayers;
        for (var i = 0; i < count; i++)
        {
            value = up ? (value < LobbyState.SlotCount ? value + 1 : LobbyState.MinPlayers) : (value > LobbyState.MinPlayers ? value - 1 : LobbyState.SlotCount);
            if (lobby.TrySetMaxPlayers(value))
            {
                return;
            }
        }
    }

    /// <summary>The next game mode in <see cref="GameMode"/> order, wrapping [gamemode 0-MAXGAMEMODE].</summary>
    public static GameMode StepMode(GameMode mode, bool up)
    {
        var index = Array.IndexOf(Modes, mode);
        return Modes[(index + (up ? 1 : Modes.Length - 1)) % Modes.Length];
    }
}
