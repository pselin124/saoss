using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Keyboard layout of one local player, and the original four layouts. <see cref="Brake"/> only moves the
/// purchase menu selection; ships have no brake.
/// </summary>
public sealed record PlayerKeyBindings(Keys Thrust, Keys Left, Keys Right, Keys Fire, Keys Special, Keys Brake)
{
    /// <summary>Default layouts of local players 1-4, as in the original.</summary>
    public static IReadOnlyList<PlayerKeyBindings> Defaults { get; } =
    [
        new(Keys.Up, Keys.Left, Keys.Right, Keys.RightShift, Keys.RightControl, Keys.Down),
        new(Keys.W, Keys.A, Keys.D, Keys.LeftShift, Keys.LeftControl, Keys.S),
        new(Keys.I, Keys.J, Keys.L, Keys.B, Keys.N, Keys.K),
        new(Keys.NumPad8, Keys.NumPad4, Keys.NumPad6, Keys.NumPad0, Keys.Decimal, Keys.NumPad5),
    ];
}
