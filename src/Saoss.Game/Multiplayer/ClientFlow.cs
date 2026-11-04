using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Match;
using Saoss.Game.Screens;
using Saoss.Game.Screens.Network;
using Saoss.Network.Sessions;

namespace Saoss.Game.Multiplayer;

/// <summary>
/// Joining a network game (net.cpp joinMultiplayer, joinwaitforstart and netpeli.cpp netJoinPeli): connecting, the
/// lobby view, every match the host starts with its in-match menu and end-of-match screen, and the error screen when
/// the session ends. A match that starts while the lobby, its chat or its settings are open replaces them at once.
/// </summary>
public sealed class ClientFlow : IMultiplayerFlow
{
    private readonly IGameShell _shell;
    private readonly ClientSession _session;
    private readonly string _host;
    private readonly Action<bool> _end;
    private ClientPhase _phase = ClientPhase.Connecting;
    private GameScreen? _match;
    private World? _matchWorld;

    /// <summary>
    /// Starts connecting to <paramref name="host"/> on TCP <paramref name="port"/> with the players from this machine.
    /// <paramref name="end"/> runs when the session is over, with true to go back to the join menu (it never got to
    /// the lobby).
    /// </summary>
    public ClientFlow(IGameShell shell, string host, int port, Action<bool> end)
    {
        _shell = shell;
        _host = host;
        _end = end;
        var settings = shell.Settings;
        _session = ClientSession.Connect(host, port, [.. settings.Nicknames.Take(settings.NetworkPlayerCount)]);
        shell.Show(new MessageScreen(shell.GraphicsDevice, shell.Assets, NetworkMessages.Connecting(host), Cancel, NetworkMessages.CancelHint));
    }

    /// <inheritdoc/>
    public void Update()
    {
        _session.Update();
        if (_phase == ClientPhase.Ended)
        {
            return;
        }

        if (_session.State == ClientState.Disconnected)
        {
            ShowDisconnected();
        }
        else if (_session.State == ClientState.InMatch && _session.World != _matchWorld && _phase != ClientPhase.Victory)
        {
            StartMatch();
        }
        else if (_session.State == ClientState.Lobby && _phase is ClientPhase.Connecting or ClientPhase.Match)
        {
            ShowLobby();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _session.Dispose();
        DisposeMatch();
    }

    private void Cancel()
    {
        _phase = ClientPhase.Ended;
        _end(true);
    }

    private void Leave()
    {
        _phase = ClientPhase.Ended;
        _end(false);
    }

    private void ShowLobby()
    {
        _phase = ClientPhase.Lobby;
        _shell.Show(new ClientLobbyScreen(
            _shell.GraphicsDevice,
            _shell.Assets,
            _session,
            Leave,
            close => NetworkSettingsMenu.Create(_shell, new NetworkSettingsMenuOptions(close))));
        DisposeMatch();
    }

    private void StartMatch()
    {
        var previous = _match;
        _phase = ClientPhase.Match;
        _matchWorld = _session.World;
        var names = _session.Lobby is { } lobby ? LobbyTexts.ShipNames(lobby, _session.ShipSlots) : null;
        var settings = _shell.Settings;
        _match = new GameScreen(_shell.GraphicsDevice, new ClientMatchDriver(_session), settings.LocalControls(), settings.Detail, settings.CameraMode, Environment.TickCount, _shell.Audio, OnMatchEnded, OpenMatchMenu, names);
        _shell.Show(_match);
        previous?.Dispose();
    }

    // Escape (netGameSets(true)): the mirrored match runs on behind the menu.
    private void OpenMatchMenu() => _match!.Overlay = NetworkSettingsMenu.Create(_shell, new NetworkSettingsMenuOptions(CloseMatchMenu) { ExitMultiplayer = Leave });

    private void CloseMatchMenu()
    {
        var menu = _match!.Overlay;
        _match.Overlay = null;
        (menu as IDisposable)?.Dispose();
    }

    private void OnMatchEnded(MatchOutcome? outcome)
    {
        if (outcome == null)
        {
            ShowLobby();
            return;
        }

        _phase = ClientPhase.Victory;
        _shell.Show(new VictoryScreen(_match!, outcome, AfterVictory), disposePrevious: false);
    }

    // The host may already have started the next match while this player looked at the result.
    private void AfterVictory()
    {
        if (_session.State == ClientState.InMatch && _session.World != _matchWorld)
        {
            StartMatch();
        }
        else
        {
            ShowLobby();
        }
    }

    private void ShowDisconnected()
    {
        var reachedLobby = _phase != ClientPhase.Connecting;
        _phase = ClientPhase.Ended;
        var message = NetworkMessages.Disconnected(_session.DisconnectReason, _session.RejectReason, _host);
        _shell.Show(new MessageScreen(_shell.GraphicsDevice, _shell.Assets, message, () => _end(!reachedLobby)));
        DisposeMatch();
    }

    private void DisposeMatch()
    {
        _match?.Dispose();
        _match = null;
    }
}
