using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// The menu keys a gamepad stands for: D-pad or left stick directions are the arrow keys, A and Start are Enter,
/// B and Back are Escape.
/// </summary>
public static class GamePadMenuKeys
{
    /// <summary>The menu keys held on <paramref name="pad"/>.</summary>
    public static IEnumerable<Keys> Held(GamePadState pad)
    {
        var stick = pad.ThumbSticks.Left;
        if (stick.Y >= GamePadMapping.Threshold || pad.DPad.Up == ButtonState.Pressed)
        {
            yield return Keys.Up;
        }

        if (stick.Y <= -GamePadMapping.Threshold || pad.DPad.Down == ButtonState.Pressed)
        {
            yield return Keys.Down;
        }

        if (stick.X <= -GamePadMapping.Threshold || pad.DPad.Left == ButtonState.Pressed)
        {
            yield return Keys.Left;
        }

        if (stick.X >= GamePadMapping.Threshold || pad.DPad.Right == ButtonState.Pressed)
        {
            yield return Keys.Right;
        }

        if (pad.IsButtonDown(Buttons.A) || pad.IsButtonDown(Buttons.Start))
        {
            yield return Keys.Enter;
        }

        if (pad.IsButtonDown(Buttons.B) || pad.IsButtonDown(Buttons.Back))
        {
            yield return Keys.Escape;
        }
    }
}
