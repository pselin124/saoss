using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Keyboard layout of one local player, and the original four layouts. <see cref="Down"/> only moves the
/// purchase menu selection; ships have no brake.
/// </summary>
public sealed record PlayerKeyBindings(Keys Thrust, Keys Left, Keys Right, Keys Fire, Keys Special, Keys Down)
{
    /// <summary>Default layouts of local players 1-4, as in the original.</summary>
    public static IReadOnlyList<PlayerKeyBindings> Defaults { get; } =
    [
        new(Keys.Up, Keys.Left, Keys.Right, Keys.RightShift, Keys.RightControl, Keys.Down),
        new(Keys.W, Keys.A, Keys.D, Keys.LeftShift, Keys.LeftControl, Keys.S),
        new(Keys.I, Keys.J, Keys.L, Keys.B, Keys.N, Keys.K),
        new(Keys.NumPad8, Keys.NumPad4, Keys.NumPad6, Keys.NumPad0, Keys.Decimal, Keys.NumPad5),
    ];

    /// <summary>Keys the menus need, which no action can use: Escape goes back and Enter confirms.</summary>
    public static IReadOnlyList<Keys> ReservedKeys { get; } = [Keys.Escape, Keys.Enter];

    /// <summary>Whether <paramref name="key"/> is a real key that an action may use.</summary>
    public static bool IsBindable(Keys key) => key != Keys.None && Enum.IsDefined(key) && !ReservedKeys.Contains(key);

    /// <summary>The key of <paramref name="action"/>.</summary>
    public Keys KeyFor(PlayerAction action) => action switch
    {
        PlayerAction.Thrust => Thrust,
        PlayerAction.Left => Left,
        PlayerAction.Right => Right,
        PlayerAction.Down => Down,
        PlayerAction.Fire => Fire,
        PlayerAction.Special => Special,
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };

    /// <summary>A copy with <paramref name="action"/> on <paramref name="key"/>.</summary>
    public PlayerKeyBindings With(PlayerAction action, Keys key) => action switch
    {
        PlayerAction.Thrust => this with { Thrust = key },
        PlayerAction.Left => this with { Left = key },
        PlayerAction.Right => this with { Right = key },
        PlayerAction.Down => this with { Down = key },
        PlayerAction.Fire => this with { Fire = key },
        PlayerAction.Special => this with { Special = key },
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };

    /// <summary>A copy where every key that is unknown or reserved is replaced by <paramref name="fallback"/>'s.</summary>
    public PlayerKeyBindings Validated(PlayerKeyBindings fallback) =>
        Enum.GetValues<PlayerAction>().Aggregate(this, (keys, action) =>
            IsBindable(keys.KeyFor(action)) ? keys : keys.With(action, fallback.KeyFor(action)));

    /// <summary>Which of these keys are held.</summary>
    public PlayerButtons Held(KeyboardTracker keyboard) => new(
        keyboard.IsDown(Thrust),
        keyboard.IsDown(Left),
        keyboard.IsDown(Right),
        keyboard.IsDown(Down),
        keyboard.IsDown(Fire),
        keyboard.IsDown(Special));

    /// <summary>Which of these keys went down on the last update.</summary>
    public PlayerButtons Pressed(KeyboardTracker keyboard) => new(
        keyboard.WasKeyPressed(Thrust),
        keyboard.WasKeyPressed(Left),
        keyboard.WasKeyPressed(Right),
        keyboard.WasKeyPressed(Down),
        keyboard.WasKeyPressed(Fire),
        keyboard.WasKeyPressed(Special));
}
