using Saoss.Game.Input;

namespace Saoss.Game.Screens;

/// <summary>
/// A full-window game state such as a menu or a running match.
/// </summary>
public interface IScreen
{
    /// <summary>Advances the screen by one fixed simulation tick.</summary>
    void Update(KeyboardTracker keyboard);

    /// <summary>Renders the screen to the back buffer.</summary>
    void Draw();
}
