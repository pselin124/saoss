using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Network.Lobby;
using Saoss.Network.Sessions;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The host's lobby (net.cpp hostMenu and drawHostMPMenu): "start game", "settings", "chat" and "exit" on the left,
/// the slot table with a status and a team column on the right. Enter or Space on a status cycles it open, closed,
/// cpu (human slots stay); on a team it moves the player to the next team. Escape chooses "exit".
/// </summary>
public sealed class HostLobbyScreen : IScreen, IDisposable
{
    private const int StartItem = 0;
    private const int SettingsItem = 1;
    private const int ChatItem = 2;
    private const int StatusColumn = 1;
    private const int TeamColumn = 2;
    private const float MenuX = -33f;

    private static readonly string[] MenuItems = ["start game", "settings", "chat", "exit"];
    private static readonly float[] ArrowX = [-7.7f, 0.3f, 3.3f];
    private static readonly Vector3[] ArrowScales = [new(0.25f, 0.3f, 0.3f), new(0.3f, 0.3f, 0.3f), new(0.27f, 0.3f, 0.3f)];

    private readonly MenuScene _scene;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameAssets _assets;
    private readonly HostSession _session;
    private readonly Action _start;
    private readonly Action _exit;
    private readonly Func<Action, IScreen> _createSettings;
    private readonly LobbyCursor _cursor = new(MenuItems.Length, TeamColumn);
    private readonly SubScreen _child = new();
    private float _arrowSpinDegrees;

    /// <summary>
    /// Shows <paramref name="session"/>'s lobby. <paramref name="start"/> starts the match, <paramref name="exit"/>
    /// ends hosting and <paramref name="createSettings"/> builds the settings menu, given the action that closes it.
    /// </summary>
    public HostLobbyScreen(GraphicsDevice graphicsDevice, GameAssets assets, HostSession session, Action start, Action exit, Func<Action, IScreen> createSettings)
    {
        _graphicsDevice = graphicsDevice;
        _assets = assets;
        _scene = new MenuScene(graphicsDevice, assets);
        _session = session;
        _start = start;
        _exit = exit;
        _createSettings = createSettings;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (_child.Update(keyboard))
        {
            return;
        }

        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        var rows = _session.Lobby.MaxPlayers;
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

        LobbyTable.Draw(_scene, _session.Lobby);
        var y = LobbyTable.ArrowTopY - (_cursor.Row * LobbyTable.ArrowRowStep);
        _scene.DrawArrow(ArrowX[_cursor.Column], y, ArrowScales[_cursor.Column], _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _child.Dispose();
        _scene.Dispose();
    }

    private void Activate()
    {
        var lobby = _session.Lobby;
        switch (_cursor.Column)
        {
            case StatusColumn:
                lobby.CycleStatus(_cursor.Row);
                break;
            case TeamColumn:
                lobby.SetTeam(_cursor.Row, (lobby.Slots[_cursor.Row].Team + 1) % LobbyState.SlotCount);
                break;
            default:
                ChooseMenuItem();
                break;
        }
    }

    private void ChooseMenuItem()
    {
        switch (_cursor.Row)
        {
            case StartItem:
                _start();
                break;
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
}
