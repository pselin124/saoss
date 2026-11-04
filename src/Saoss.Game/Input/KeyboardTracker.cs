using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Tracks keyboard and gamepad state between updates so screens can react to key presses instead of held keys, and
/// collects the characters typed since the previous update for text entry, plus the clipboard text on Ctrl+V or
/// Shift+Insert. For the menus, gamepads also press the keys of <see cref="GamePadMenuKeys"/>: see
/// <see cref="WasPressed"/>.
/// </summary>
/// <param name="readClipboard">Reads the clipboard text; the Windows clipboard by default.</param>
/// <param name="readKeyboard">Reads the keyboard; the real one by default.</param>
/// <param name="readGamePad">Reads gamepad 0-3; the real ones by default. A missing pad reads as disconnected.</param>
public sealed class KeyboardTracker(
    Func<string?>? readClipboard = null,
    Func<KeyboardState>? readKeyboard = null,
    Func<int, GamePadState>? readGamePad = null)
{
    /// <summary>Gamepads read, one per possible local player.</summary>
    public const int GamePadCount = 4;

    private readonly Func<string?> _readClipboard = readClipboard ?? ClipboardText.Read;
    private readonly Func<KeyboardState> _readKeyboard = readKeyboard ?? Keyboard.GetState;
    private readonly Func<int, GamePadState> _readGamePad = readGamePad ?? (index => GamePad.GetState(index));
    private readonly List<char> _pendingCharacters = [];
    private readonly GamePadState[] _pads = new GamePadState[GamePadCount];
    private readonly GamePadState[] _previousPads = new GamePadState[GamePadCount];
    private HashSet<Keys> _padKeys = [];
    private HashSet<Keys> _previousPadKeys = [];
    private KeyboardState _previous;
    private KeyboardState _current;
    private char[] _typedCharacters = [];

    /// <summary>Characters typed before the last <see cref="Update"/>, including '\b' for Backspace.</summary>
    public IReadOnlyList<char> TypedCharacters => _typedCharacters;

    /// <summary>Clipboard text pasted on the last <see cref="Update"/>, or an empty string.</summary>
    public string PastedText { get; private set; } = string.Empty;

    /// <summary>
    /// Reads the current keyboard and gamepad state. Input is ignored while the window is not focused.
    /// </summary>
    public void Update(bool isWindowActive)
    {
        _previous = _current;
        _current = isWindowActive ? _readKeyboard() : default;
        UpdatePads(isWindowActive);
        _typedCharacters = isWindowActive ? [.. _pendingCharacters] : [];
        _pendingCharacters.Clear();
        PastedText = IsPastePressed() ? _readClipboard() ?? string.Empty : string.Empty;
    }

    /// <summary>Records a typed character (from the window's text input event) for the next update.</summary>
    public void AddTypedCharacter(char c) => _pendingCharacters.Add(c);

    /// <summary>
    /// Returns true only on the update where the key went down, on the keyboard or as a menu key of any gamepad.
    /// For menus; players' own keys use <see cref="WasKeyPressed"/>.
    /// </summary>
    public bool WasPressed(Keys key) => WasKeyPressed(key) || (_padKeys.Contains(key) && !_previousPadKeys.Contains(key));

    /// <summary>Returns true only on the update where the key went down on the keyboard.</summary>
    public bool WasKeyPressed(Keys key) => _current.IsKeyDown(key) && !_previous.IsKeyDown(key);

    /// <summary>Returns true while the key is held down on the keyboard.</summary>
    public bool IsDown(Keys key) => _current.IsKeyDown(key);

    /// <summary>The first key that went down on the keyboard on the last update, or null.</summary>
    public Keys? FirstPressedKey() => _current.GetPressedKeys().Where(k => !_previous.IsKeyDown(k)).Select(k => (Keys?)k).FirstOrDefault();

    /// <summary>The state of gamepad <paramref name="index"/> (0-3) on the last update; disconnected if missing.</summary>
    public GamePadState PadState(int index) => _pads[index];

    /// <summary>The state of gamepad <paramref name="index"/> (0-3) on the update before the last.</summary>
    public GamePadState PreviousPadState(int index) => _previousPads[index];

    /// <summary>Returns true only on the update where <paramref name="button"/> went down on any gamepad.</summary>
    public bool WasAnyPadPressed(Buttons button)
    {
        for (var i = 0; i < GamePadCount; i++)
        {
            if (_pads[i].IsButtonDown(button) && !_previousPads[i].IsButtonDown(button))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdatePads(bool isWindowActive)
    {
        (_previousPadKeys, _padKeys) = (_padKeys, _previousPadKeys);
        _padKeys.Clear();
        for (var i = 0; i < GamePadCount; i++)
        {
            _previousPads[i] = _pads[i];
            _pads[i] = isWindowActive ? _readGamePad(i) : default;
            _padKeys.UnionWith(GamePadMenuKeys.Held(_pads[i]));
        }
    }

    private bool IsPastePressed() =>
        (WasKeyPressed(Keys.V) && (IsDown(Keys.LeftControl) || IsDown(Keys.RightControl))) ||
        (WasKeyPressed(Keys.Insert) && (IsDown(Keys.LeftShift) || IsDown(Keys.RightShift)));
}
