using Saoss.Core.Simulation.Modes;
using Saoss.Game.Match;
using Saoss.Game.Screens;
using Saoss.Game.Screens.Network;
using Saoss.Network.Discovery;
using Saoss.Network.Lobby;
using Saoss.Network.Sessions;

namespace Saoss.Game.Multiplayer;

/// <summary>
/// Hosting a network game (net.cpp hostMenu, hostStart and netpeli.cpp netHostPeli): the lobby, the match with its
/// in-match menu, the end-of-match screen and back to the lobby. "exit" in the lobby or "exit multiplayer" in the
/// match ends hosting; clients are told the host quit. Joiners on the LAN can find the game meanwhile.
/// </summary>
public sealed class HostFlow : IMultiplayerFlow
{
    private readonly IGameShell _shell;
    private readonly HostSession _session;
    private readonly LanAnnouncer? _announcer = LanAnnouncer.TryStart();
    private readonly Action _end;
    private int? _autoStartPlayers;
    private GameScreen? _match;
    private bool _isInLobby;

    /// <summary>
    /// Takes over <paramref name="session"/>. <paramref name="end"/> runs when the player stops hosting; with
    /// <paramref name="autoStartPlayers"/> the first match starts by itself once that many humans are in the lobby.
    /// </summary>
    public HostFlow(IGameShell shell, HostSession session, Action end, int? autoStartPlayers = null)
    {
        _shell = shell;
        _session = session;
        _end = end;
        _autoStartPlayers = autoStartPlayers;
        session.Lobby.SetAiTargetsOnlyEnemies(shell.Settings.AiTargetsOnlyEnemies);
        session.Lobby.SetAiDifficulty(shell.Settings.AiDifficulty);
    }

    /// <summary>Shows the lobby.</summary>
    public void ShowLobby()
    {
        _isInLobby = true;
        _shell.Show(new HostLobbyScreen(
            _shell.GraphicsDevice,
            _shell.Assets,
            _session,
            StartMatch,
            _end,
            close => HostSettingsMenu.Create(_shell, _session.Lobby, close)));
    }

    /// <inheritdoc/>
    public void Update()
    {
        _session.Update();
        _announcer?.Update(() => LanGameInfo.FromSession(_session));
        if (_isInLobby && _autoStartPlayers is { } players && _session.Lobby.Slots.Count(s => s.Status == SlotStatus.Human) >= players)
        {
            _autoStartPlayers = null;
            StartMatch();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _session.Dispose();
        _announcer?.Dispose();
        _match?.Dispose();
    }

    private void StartMatch()
    {
        _isInLobby = false;
        var seed = Environment.TickCount;
        _session.StartMatch(seed);
        var names = LobbyTexts.ShipNames(_session.Lobby, _session.ShipSlots);
        var settings = _shell.Settings;
        _match = new GameScreen(_shell.GraphicsDevice, new HostMatchDriver(_session), settings.LocalControls(), settings.Detail, settings.CameraMode, seed + 1, _shell.Audio, OnMatchEnded, OpenMatchMenu, names);
        _shell.Show(_match);
    }

    // Escape (netGameSets(true)): the match runs on behind the menu.
    private void OpenMatchMenu() => _match!.Overlay = NetworkSettingsMenu.Create(_shell, new NetworkSettingsMenuOptions(CloseMatchMenu) { ExitMultiplayer = _end });

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
            BackToLobby();
            return;
        }

        _shell.Show(new VictoryScreen(_match!, outcome, BackToLobby), disposePrevious: false);
    }

    // The original ended the whole network game here; the lobby stays so the same players can play again.
    private void BackToLobby()
    {
        _session.ReturnToLobby();
        ShowLobby();
        _match?.Dispose();
        _match = null;
    }
}
