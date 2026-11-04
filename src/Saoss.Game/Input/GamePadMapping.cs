using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// The fixed gamepad layout of a player: left stick or D-pad left and right turn, stick or D-pad up or the right
/// shoulder thrusts, stick or D-pad down moves the ship shop selection down, A or the right trigger fires and B or
/// the left trigger uses the special weapon. A disconnected pad presses nothing.
/// </summary>
public static class GamePadMapping
{
    /// <summary>How far a stick or trigger must move before it counts as pressed (0 to 1).</summary>
    public const float Threshold = 0.5f;

    /// <summary>The player's buttons held on <paramref name="pad"/>.</summary>
    public static PlayerButtons Held(GamePadState pad)
    {
        var stick = pad.ThumbSticks.Left;
        return new PlayerButtons(
            Thrust: stick.Y >= Threshold || IsDown(pad.DPad.Up) || pad.IsButtonDown(Buttons.RightShoulder),
            Left: stick.X <= -Threshold || IsDown(pad.DPad.Left),
            Right: stick.X >= Threshold || IsDown(pad.DPad.Right),
            Down: stick.Y <= -Threshold || IsDown(pad.DPad.Down),
            Fire: pad.IsButtonDown(Buttons.A) || pad.Triggers.Right >= Threshold,
            Special: pad.IsButtonDown(Buttons.B) || pad.Triggers.Left >= Threshold);
    }

    private static bool IsDown(ButtonState state) => state == ButtonState.Pressed;
}
