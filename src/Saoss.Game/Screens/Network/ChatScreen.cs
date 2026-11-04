using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Network.Protocol;
using Saoss.Network.Sessions;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The lobby chat (chat.cpp chatScreen): 23 rows of history above a write line, with "write" and "exit" below.
/// The arrow keys switch between them and Enter chooses. While writing, typed text goes to the write line, Enter
/// sends it and keeps writing, and Escape stops writing; otherwise Escape leaves the chat.
/// </summary>
public sealed class ChatScreen : IScreen, IDisposable
{
    private const float HistoryX = -36f;
    private const float HistoryBottomY = -17f;
    private const float HistoryRowSpacing = 2f;
    private const float TextZ = -60f;
    private const float ChatTextSize = 0.8f;
    private const int VisibleWriteCharacters = 85;

    private static readonly Vector3 WriteLinePosition = new(-35f, -20f, TextZ);
    private static readonly Vector3 WritePosition = new(-15f, -24f, TextZ);
    private static readonly Vector3 ExitPosition = new(10f, -24f, TextZ);
    private static readonly Vector2 WriteArrow = new(-3.6f, -4.75f);
    private static readonly Vector2 WritingArrow = new(-7.8f, -3.95f);
    private static readonly Vector2 ExitArrow = new(1.4f, -4.75f);
    private static readonly Vector3 ArrowScale = new(0.22f, 0.3f, 0.3f);

    private readonly MenuScene _scene;
    private readonly Func<IReadOnlyList<ChatLine>> _lines;
    private readonly Action<string> _send;
    private readonly Action _exit;
    private readonly TextInputBuffer _buffer = new(ChatText.MaxLength, c => c);
    private bool _isWriting;
    private bool _isExitSelected;
    private float _arrowSpinDegrees;

    /// <summary>
    /// Shows the session's chat. <paramref name="lines"/> reads the history, <paramref name="send"/> sends a line and
    /// <paramref name="exit"/> runs when the player leaves.
    /// </summary>
    public ChatScreen(GraphicsDevice graphicsDevice, GameAssets assets, Func<IReadOnlyList<ChatLine>> lines, Action<string> send, Action exit)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _lines = lines;
        _send = send;
        _exit = exit;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        if (_isWriting)
        {
            UpdateWriting(keyboard);
        }
        else if (keyboard.WasPressed(Keys.Escape) || (keyboard.WasPressed(Keys.Enter) && _isExitSelected))
        {
            _exit();
        }
        else if (keyboard.WasPressed(Keys.Enter))
        {
            _buffer.Clear();
            _isWriting = true;
        }
        else if (keyboard.WasPressed(Keys.Up) || keyboard.WasPressed(Keys.Down) || keyboard.WasPressed(Keys.Left) || keyboard.WasPressed(Keys.Right))
        {
            _isExitSelected = !_isExitSelected;
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin(withBackground: false);
        var rows = ChatView.Rows(_lines());
        for (var i = 0; i < rows.Count; i++)
        {
            _scene.DrawText(new Vector3(HistoryX, HistoryBottomY + (i * HistoryRowSpacing), TextZ), rows[i], Color.Red, ChatTextSize);
        }

        _scene.DrawText(WriteLinePosition, _buffer.VisibleTail(VisibleWriteCharacters), Color.Red, ChatTextSize);
        _scene.DrawText(WritePosition, "write", Color.Red);
        _scene.DrawText(ExitPosition, "exit", Color.Red);
        var arrow = _isWriting ? WritingArrow : _isExitSelected ? ExitArrow : WriteArrow;
        _scene.DrawArrow(arrow.X, arrow.Y, ArrowScale, _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();

    private void UpdateWriting(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Escape))
        {
            _buffer.Clear();
            _isWriting = false;
            _isExitSelected = false;
        }
        else if (keyboard.WasPressed(Keys.Enter))
        {
            _send(_buffer.Text);
            _buffer.Clear();
        }
        else
        {
            _buffer.Type(keyboard.TypedCharacters);
        }
    }
}
