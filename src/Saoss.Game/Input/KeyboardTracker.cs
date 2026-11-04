using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Tracks keyboard state between updates so screens can react to key presses instead of held keys, and collects
/// the characters typed since the previous update for text entry, plus the clipboard text on Ctrl+V or Shift+Insert.
/// </summary>
/// <param name="readClipboard">Reads the clipboard text; the Windows clipboard by default.</param>
public sealed class KeyboardTracker(Func<string?>? readClipboard = null)
{
    private readonly Func<string?> _readClipboard = readClipboard ?? ClipboardText.Read;
    private readonly List<char> _pendingCharacters = [];
    private KeyboardState _previous;
    private KeyboardState _current;
    private char[] _typedCharacters = [];

    /// <summary>Characters typed before the last <see cref="Update"/>, including '\b' for Backspace.</summary>
    public IReadOnlyList<char> TypedCharacters => _typedCharacters;

    /// <summary>Clipboard text pasted on the last <see cref="Update"/>, or an empty string.</summary>
    public string PastedText { get; private set; } = string.Empty;

    /// <summary>
    /// Reads the current keyboard state. Input is ignored while the window is not focused.
    /// </summary>
    public void Update(bool isWindowActive)
    {
        _previous = _current;
        _current = isWindowActive ? Keyboard.GetState() : default;
        _typedCharacters = isWindowActive ? [.. _pendingCharacters] : [];
        _pendingCharacters.Clear();
        PastedText = IsPastePressed() ? _readClipboard() ?? string.Empty : string.Empty;
    }

    /// <summary>Records a typed character (from the window's text input event) for the next update.</summary>
    public void AddTypedCharacter(char c) => _pendingCharacters.Add(c);

    /// <summary>Returns true only on the update where the key went down.</summary>
    public bool WasPressed(Keys key) => _current.IsKeyDown(key) && !_previous.IsKeyDown(key);

    /// <summary>Returns true while the key is held down.</summary>
    public bool IsDown(Keys key) => _current.IsKeyDown(key);

    private bool IsPastePressed() =>
        (WasPressed(Keys.V) && (IsDown(Keys.LeftControl) || IsDown(Keys.RightControl))) ||
        (WasPressed(Keys.Insert) && (IsDown(Keys.LeftShift) || IsDown(Keys.RightShift)));
}
