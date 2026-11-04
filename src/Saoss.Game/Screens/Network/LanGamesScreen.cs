using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Multiplayer;
using Saoss.Network.Discovery;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The games found on the LAN, opened from the join menu: one row per host with its first nickname, humans and
/// slots, and whether it can be joined, then "back". Games in a match, full or of another version are grey and
/// skipped by the arrow. Enter on a game connects to it; Escape or "back" returns to the join menu. Searching runs
/// only while this screen is open.
/// </summary>
public sealed class LanGamesScreen : IScreen, IDisposable
{
    private const int MaxRows = 6;
    private const float TextZ = -60f;
    private const float NameX = -30f;
    private const float PlayersX = -11f;
    private const float StatusX = -3f;
    private const float TitleY = 9f;
    private const float FirstRowY = 4f;
    private const float RowSpacing = 3f;
    private const float BackY = -15f;
    private const float DetailY = -19f;
    private const float DetailSize = 0.8f;
    private const float ArrowX = -7.2f;
    private const float ArrowPerTextUnit = 0.2f;

    private static readonly Vector3 ArrowScale = new(0.28f, 0.3f, 0.3f);
    private static readonly MenuPanel Panel = new(-2.25f, -1.4f, 1f, 0.75f);
    private static readonly Color Unavailable = new(0.45f, 0.45f, 0.45f);

    private readonly IGameShell _shell;
    private readonly MenuScene _scene;
    private readonly LanBrowser _browser = new();
    private readonly Action<string, int> _connect;
    private readonly Action _close;
    private int? _selectedId;
    private bool _hasMoved;
    private float _arrowSpinDegrees;

    /// <summary>Starts searching; <paramref name="connect"/> gets a picked game's address and port.</summary>
    public LanGamesScreen(IGameShell shell, Action<string, int> connect, Action close)
    {
        _shell = shell;
        _scene = new MenuScene(shell.GraphicsDevice, shell.Assets);
        _connect = connect;
        _close = close;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _browser.Update();
        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        var choices = Choices();
        if (!_hasMoved || !choices.Contains(_selectedId))
        {
            // Until the player moves the arrow it stays on the first game found; a game that goes away leaves it on "back".
            _selectedId = _hasMoved ? null : choices[0];
        }

        var index = choices.IndexOf(_selectedId);
        if (keyboard.WasPressed(Keys.Up))
        {
            _hasMoved = true;
            _selectedId = choices[(index + choices.Count - 1) % choices.Count];
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            _hasMoved = true;
            _selectedId = choices[(index + 1) % choices.Count];
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            _close();
        }
        else if (keyboard.WasPressed(Keys.Enter))
        {
            Choose(choices[index]);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin();
        _scene.DrawPanel(Panel.Left, Panel.Bottom, Panel.Right, Panel.Top);
        _scene.DrawText(new Vector3(NameX, TitleY, TextZ), "games on LAN", Color.Red);
        var hosts = Rows();
        if (hosts.Count == 0)
        {
            _scene.DrawText(new Vector3(NameX, FirstRowY, TextZ), "searching...", Unavailable);
        }

        for (var i = 0; i < hosts.Count; i++)
        {
            DrawRow(hosts[i], RowY(i));
        }

        _scene.DrawText(new Vector3(NameX, BackY, TextZ), "back", Color.Red);
        var selected = hosts.FirstOrDefault(h => h.InstanceId == _selectedId);
        if (selected != null)
        {
            var detail = $"{selected.Address}  {(selected.Game.Mode is { } mode ? LobbyTexts.Mode(mode) : string.Empty)}";
            _scene.DrawText(new Vector3(NameX, DetailY, TextZ), detail, Color.Red, DetailSize);
        }

        var arrowRow = hosts.FindIndex(h => h.InstanceId == _selectedId);
        var arrowY = arrowRow >= 0 ? RowY(arrowRow) : BackY;
        _scene.DrawArrow(ArrowX, arrowY * ArrowPerTextUnit, ArrowScale, _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _browser.Dispose();
        _scene.Dispose();
    }

    private static float RowY(int row) => FirstRowY - (row * RowSpacing);

    private List<DiscoveredHost> Rows() => [.. _browser.Hosts.Take(MaxRows)];

    // The joinable games in display order, then null for "back".
    private List<int?> Choices() =>
        [.. Rows().Where(h => h.IsJoinable(_shell.Settings.NetworkPlayerCount)).Select(h => (int?)h.InstanceId), null];

    private void Choose(int? instanceId)
    {
        if (_browser.Hosts.FirstOrDefault(h => h.InstanceId == instanceId) is { } host)
        {
            _connect(host.Address.ToString(), host.Port);
        }
        else
        {
            _close();
        }
    }

    private void DrawRow(DiscoveredHost host, float y)
    {
        var players = _shell.Settings.NetworkPlayerCount;
        var color = host.IsJoinable(players) ? Color.Red : Unavailable;
        var game = host.Game;
        var count = string.Create(CultureInfo.InvariantCulture, $"{game.HumanPlayers}/{game.MaxPlayers}");
        _scene.DrawText(new Vector3(NameX, y, TextZ), game.HostNickname, color);
        _scene.DrawText(new Vector3(PlayersX, y, TextZ), count, color);
        _scene.DrawText(new Vector3(StatusX, y, TextZ), Status(host, players), color);
    }

    private static string Status(DiscoveredHost host, int players) => host switch
    {
        { IsCompatible: false } => "other version",
        { Game.IsInGame: true } => "in game",
        _ when host.Game.OpenSlots < players => "full",
        _ => "open",
    };
}
