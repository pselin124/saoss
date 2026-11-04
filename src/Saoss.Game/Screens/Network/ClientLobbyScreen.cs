using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Network.Lobby;
using Saoss.Network.Sessions;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// A client's view of the lobby while waiting for the host to start (net.cpp joinwaitforstart and drawJoinWait):
/// "settings", "chat" and "exit" on the left and the host's slot table, read-only except the team column, where
/// Enter or Space asks the host to move one of this machine's players to the next team. Escape chooses "exit".
/// </summary>
public sealed class ClientLobbyScreen : IScreen, IDisposable
{
    private const int SettingsItem = 0;
    private const int ChatItem = 1;
    private const int TeamColumn = 1;
    private const float MenuX = -33f;

    private static readonly string[] MenuItems = ["settings", "chat", "exit"];
    private static readonly float[] ArrowX = [-7.7f, 3.3f];
    private static readonly Vector3 ArrowScale = new(0.22f, 0.3f, 0.3f);

    private readonly MenuScene _scene;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameAssets _assets;
    private readonly ClientSession _session;
    private readonly Action _exit;
    private readonly Func<Action, IScreen> _createSettings;
    private readonly LobbyCursor _cursor = new(MenuItems.Length, TeamColumn);
    private readonly SubScreen _child = new();
    private float _arrowSpinDegrees;

    /// <summary>
    /// Shows <paramref name="session"/>'s lobby. <paramref name="exit"/> leaves the game and
    /// <paramref name="createSettings"/> builds the settings menu, given the action that closes it.
    /// </summary>
    public ClientLobbyScreen(GraphicsDevice graphicsDevice, GameAssets assets, ClientSession session, Action exit, Func<Action, IScreen> createSettings)
    {
        _graphicsDevice = graphicsDevice;
        _assets = assets;
        _scene = new MenuScene(graphicsDevice, assets);
        _session = session;
        _exit = exit;
        _createSettings = createSettings;
    }

    private int TableRows => _session.Lobby?.MaxPlayers ?? 1;

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (_child.Update(keyboard))
        {
            return;
        }

        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        var rows = TableRows;
        _cursor.Clamp(rows);
        if (keyboard.WasPressed(Keys.Enter) || keyboard.WasPressed(Keys.Space))
        {
            Activate();
        }
        else if (keyboard.WasPressed(Keys.Left))
        {
            _cursor.Left(rows);
        }
        else if (keyboard.WasPressed(Keys.Right))
        {
            _cursor.Right(rows);
        }
        else if (keyboard.WasPressed(Keys.Up))
        {
            _cursor.Up(rows);
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            _cursor.Down(rows);
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            _cursor.SelectLastMenuItem();
            Activate();
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (_child.Draw())
        {
            return;
        }

        _scene.Begin(_assets.LobbyBackground, LobbyTable.BackgroundDistance);
        for (var i = 0; i < MenuItems.Length; i++)
        {
            _scene.DrawText(new Vector3(MenuX, LobbyTable.RowY(i), LobbyTable.TextZ), MenuItems[i], Color.Red);
        }

        if (_session.Lobby is { } lobby)
        {
            LobbyTable.Draw(_scene, lobby);
        }

        var y = LobbyTable.ArrowTopY - (_cursor.Row * LobbyTable.ArrowRowStep);
        _scene.DrawArrow(ArrowX[_cursor.Column], y, ArrowScale, _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _child.Dispose();
        _scene.Dispose();
    }

    private void Activate()
    {
        if (_cursor.Column == TeamColumn)
        {
            RequestNextTeam(_cursor.Row);
            return;
        }

        switch (_cursor.Row)
        {
            case SettingsItem:
                _child.Open(_createSettings(_child.Close));
                break;
            case ChatItem:
                _child.Open(new ChatScreen(_graphicsDevice, _assets, () => _session.ChatLog, _session.SendChat, _child.Close));
                break;
            default:
                _exit();
                break;
        }
    }

    // Only this machine's players can change team; the host checks it again.
    private void RequestNextTeam(int slot)
    {
        if (_session.Lobby?.Slots[slot] is { Status: SlotStatus.Human } own && own.OwnerId == _session.ClientId)
        {
            _session.RequestTeam(slot, (own.Team + 1) % LobbyState.SlotCount);
        }
    }
}
