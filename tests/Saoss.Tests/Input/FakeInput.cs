using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;

namespace Saoss.Tests.Input;

/// <summary>
/// A <see cref="KeyboardTracker"/> fed with scripted keyboard and gamepad states instead of the real devices.
/// </summary>
internal sealed class FakeInput
{
    private readonly GamePadState[] _pads = new GamePadState[KeyboardTracker.GamePadCount];
    private Keys[] _keys = [];

    public FakeInput() => Tracker = new KeyboardTracker(() => null, () => new KeyboardState(_keys), i => _pads[i]);

    public KeyboardTracker Tracker { get; }

    /// <summary>A connected pad with the given stick, triggers, D-pad and buttons.</summary>
    public static GamePadState Pad(
        Vector2 stick = default,
        float leftTrigger = 0f,
        float rightTrigger = 0f,
        GamePadDPad dPad = default,
        Buttons[]? buttons = null) =>
        new(
            new GamePadThumbSticks(stick, Vector2.Zero),
            new GamePadTriggers(leftTrigger, rightTrigger),
            new GamePadButtons((buttons ?? []).Aggregate((Buttons)0, (all, b) => all | b)),
            dPad);

    /// <summary>A D-pad with only <paramref name="direction"/> held: "up", "down", "left" or "right".</summary>
    public static GamePadDPad DPad(string direction) => new(
        direction == "up" ? ButtonState.Pressed : ButtonState.Released,
        direction == "down" ? ButtonState.Pressed : ButtonState.Released,
        direction == "left" ? ButtonState.Pressed : ButtonState.Released,
        direction == "right" ? ButtonState.Pressed : ButtonState.Released);

    /// <summary>Holds <paramref name="keys"/> on the keyboard and runs one update.</summary>
    public FakeInput Step(params Keys[] keys)
    {
        _keys = keys;
        Tracker.Update(isWindowActive: true);
        return this;
    }

    /// <summary>Sets pad <paramref name="index"/> to <paramref name="state"/>, releases the keyboard and runs one update.</summary>
    public FakeInput StepPad(int index, GamePadState state)
    {
        _pads[index] = state;
        return Step();
    }
}
