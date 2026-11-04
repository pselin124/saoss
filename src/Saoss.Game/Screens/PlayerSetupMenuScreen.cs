using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens;

/// <summary>
/// The player setup menu (Menu.cpp playerMenu and drawPlayerMenu): for each player, Human or CPU in the left column
/// and the team in the right column, then "Previous Menu". Up and Down move within a column with wrap-around,
/// Left and Right switch columns, Enter changes the value under the arrow and Escape goes back. Only players 1-4
/// can be human, as there are four keyboard layouts. The original's "ai attacks ai" line was commented out of the
/// menu, so the setting is kept in the file but not shown here.
/// </summary>
public sealed class PlayerSetupMenuScreen : IScreen
{
    private const float TitleX = -11f;
    private const float TitleY = 8f;
    private const float TitleZ = -8f;
    private const float RowTopY = 14f;
    private const float RowSpacing = 2f;
    private const float PlayerWordX = -20f;
    private const float KindX = -10f;
    private const float TeamPlayerWordX = 0f;
    private const float TeamWordX = 10f;
    private const float ArrowX = -6.7f;
    private const float ArrowColumnStep = 5.5f;
    private const float ArrowTopY = 3.8f;
    private const float ArrowRowStep = 0.54667f;
    private const int KindColumn = 0;
    private const int TeamColumn = 1;

    private static readonly Vector3 ArrowScale = new(0.3f);
    private static readonly Color HumanColor = new(0f, 1f, 0f);
    private static readonly Color CpuColor = new(0f, 0f, 1f);

    private readonly MenuScene _scene;
    private readonly GameSettings _settings;
    private readonly PlayerSlotSettings[] _players;
    private readonly Action<GameSettings> _close;
    private int _selection;
    private int _column;
    private float _arrowSpinDegrees;

    /// <summary>
    /// Opens the menu on <paramref name="settings"/>. <paramref name="close"/> receives the settings with the changed
    /// player slots when the player leaves the menu.
    /// </summary>
    public PlayerSetupMenuScreen(MenuScene scene, GameSettings settings, Action<GameSettings> close)
    {
        _scene = scene;
        _settings = settings;
        _players = [.. settings.Players];
        _close = close;
    }

    // The last row is "previous menu"; it only exists in the left column.
    private int PreviousMenuRow => _settings.PlayerCount;

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        if (keyboard.WasPressed(Keys.Enter))
        {
            Choose();
        }
        else if (keyboard.WasPressed(Keys.Left) || keyboard.WasPressed(Keys.Right))
        {
            _column = _column == KindColumn ? TeamColumn : KindColumn;
        }
        else if (keyboard.WasPressed(Keys.Up))
        {
            _selection = _selection > 0 ? _selection - 1 : PreviousMenuRow;
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            _selection = _selection < PreviousMenuRow ? _selection + 1 : 0;
            _selection = _selection == PreviousMenuRow && _column == TeamColumn ? 0 : _selection;
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            _selection = PreviousMenuRow;
            _column = KindColumn;
            Choose();
        }

        if (_column == TeamColumn && _selection == PreviousMenuRow)
        {
            _selection--;
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin(withBackground: false);
        _scene.DrawText(new Vector3(TitleX, TitleY, TitleZ), "Player Setup", Color.Red);
        _scene.DrawText(new Vector3(PlayerWordX, RowY(PreviousMenuRow), MenuScene.TextZ), "Previous Menu", Color.Red);
        for (var i = 0; i < _settings.PlayerCount; i++)
        {
            DrawPlayerRow(i);
        }

        var arrowX = ArrowX + (ArrowColumnStep * _column);
        _scene.DrawArrow(arrowX, ArrowTopY - (_selection * ArrowRowStep), ArrowScale, _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    private static float RowY(int row) => RowTopY - (row * RowSpacing);

    private void Choose()
    {
        if (_selection == PreviousMenuRow)
        {
            _close(_settings with { Players = _players });
        }
        else if (_column == KindColumn && _selection < GameSettings.MaxLocalHumans)
        {
            _players[_selection] = _players[_selection] with { IsHuman = !_players[_selection].IsHuman };
        }
        else if (_column == TeamColumn)
        {
            _players[_selection] = _players[_selection] with { Team = SettingsSteps.Team(_players[_selection].Team) };
        }
    }

    // "Player N: Human/CPU" on the left and "Player N  Team T" in the team's color on the right; values in columns.
    private void DrawPlayerRow(int index)
    {
        var y = RowY(index);
        var slot = _players[index];
        var player = $"Player {(index + 1).ToString(CultureInfo.InvariantCulture)}";
        DrawAt(PlayerWordX, y, player + ":", Color.Red);
        DrawAt(KindX, y, slot.IsHuman ? "Human" : "CPU", slot.IsHuman ? HumanColor : CpuColor);

        DrawAt(TeamPlayerWordX, y, player, Color.Red);
        var (r, g, b) = SceneDrawing.TeamRgb(slot.Team);
        DrawAt(TeamWordX, y, $"Team {(slot.Team + 1).ToString(CultureInfo.InvariantCulture)}", new Color(r, g, b));
    }

    private void DrawAt(float x, float y, string text, Color color) => _scene.DrawText(new Vector3(x, y, MenuScene.TextZ), text, color);
}
