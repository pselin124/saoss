using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Tracks keyboard state between updates so screens can react to key presses instead of held keys.
/// </summary>
public sealed class KeyboardTracker
{
    private KeyboardState _previous;
    private KeyboardState _current;

    /// <summary>
    /// Reads the current keyboard state. Input is ignored while the window is not focused.
    /// </summary>
    public void Update(bool isWindowActive)
    {
        _previous = _current;
        _current = isWindowActive ? Keyboard.GetState() : default;
    }

    /// <summary>Returns true only on the update where the key went down.</summary>
    public bool WasPressed(Keys key) => _current.IsKeyDown(key) && !_previous.IsKeyDown(key);

    /// <summary>Returns true while the key is held down.</summary>
    public bool IsDown(Keys key) => _current.IsKeyDown(key);
}
