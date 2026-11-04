namespace Saoss.Game.Input;

/// <summary>
/// How one local player plays: their keys, used when <see cref="Device"/> is the keyboard, and the device.
/// Gamepads use the fixed <see cref="GamePadMapping"/>.
/// </summary>
/// <param name="Keys">The player's keyboard layout.</param>
/// <param name="Device">The keyboard or a gamepad.</param>
public sealed record PlayerControls(PlayerKeyBindings Keys, InputDevice Device)
{
    /// <summary>The player's buttons held on their device.</summary>
    public PlayerButtons Held(KeyboardTracker input) =>
        PadIndex is { } pad ? GamePadMapping.Held(input.PadState(pad)) : Keys.Held(input);

    /// <summary>The player's buttons that went down on their device on the last update.</summary>
    public PlayerButtons Pressed(KeyboardTracker input) =>
        PadIndex is { } pad
            ? GamePadMapping.Held(input.PadState(pad)).PressedSince(GamePadMapping.Held(input.PreviousPadState(pad)))
            : Keys.Pressed(input);

    private int? PadIndex => Device == InputDevice.Keyboard ? null : Device - InputDevice.Gamepad1;
}
