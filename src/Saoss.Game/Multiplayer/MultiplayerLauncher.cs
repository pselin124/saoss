using System.Net.Sockets;
using Saoss.Game.Screens.Network;
using Saoss.Network.Protocol;
using Saoss.Network.Sessions;

namespace Saoss.Game.Multiplayer;

/// <summary>
/// Entry to network play from the main menu (net.cpp hostMultiplayer and joinMultiplayer): the host and join setup
/// menus, and the running <see cref="IMultiplayerFlow"/>, which the game updates every frame.
/// </summary>
/// <param name="shell">The running game.</param>
public sealed class MultiplayerLauncher(IGameShell shell) : IDisposable
{
    private IMultiplayerFlow? _flow;

    /// <summary>Shows the first host menu.</summary>
    public void ShowHostSetup() => shell.Show(HostSetupMenu.Create(shell, () => Host(), shell.ShowMainMenu));

    /// <summary>Shows the join menu.</summary>
    public void ShowJoinSetup() => shell.Show(JoinSetupMenu.Create(shell, Join, shell.ShowMainMenu));

    /// <summary>Shows the join menu with its list of games on the LAN open.</summary>
    public void ShowLanGames() => shell.Show(JoinSetupMenu.Create(shell, Join, shell.ShowMainMenu, showLanGames: true));

    /// <summary>
    /// Starts listening with the players from this machine and opens the lobby; the first match starts by itself
    /// once <paramref name="autoStartPlayers"/> humans are in, when given.
    /// </summary>
    public void Host(int? autoStartPlayers = null)
    {
        var settings = shell.Settings;
        HostSession session;
        try
        {
            session = new HostSession([.. settings.Nicknames.Take(settings.NetworkPlayerCount)]);
        }
        catch (SocketException)
        {
            shell.Show(new MessageScreen(shell.GraphicsDevice, shell.Assets, NetworkMessages.CannotHost(ProtocolVersion.DefaultPort), ShowHostSetup));
            return;
        }

        var flow = new HostFlow(shell, session, () => End(toJoinMenu: false), autoStartPlayers);
        _flow = flow;
        flow.ShowLobby();
    }

    /// <summary>Connects to <paramref name="host"/> on TCP <paramref name="port"/>.</summary>
    public void Join(string host, int port = ProtocolVersion.DefaultPort) => _flow = new ClientFlow(shell, host, port, toJoinMenu => End(toJoinMenu));

    /// <summary>Updates the running network game, if any.</summary>
    public void Update() => _flow?.Update();

    /// <inheritdoc/>
    public void Dispose()
    {
        _flow?.Dispose();
        _flow = null;
    }

    private void End(bool toJoinMenu)
    {
        Dispose();
        if (toJoinMenu)
        {
            ShowJoinSetup();
        }
        else
        {
            shell.ShowMainMenu();
        }
    }
}
