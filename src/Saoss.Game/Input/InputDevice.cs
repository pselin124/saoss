namespace Saoss.Game.Input;

/// <summary>
/// What a local player plays with: the shared keyboard or one of the first four gamepads.
/// </summary>
public enum InputDevice
{
    /// <summary>The player's keys on the shared keyboard.</summary>
    Keyboard,

    /// <summary>The first gamepad.</summary>
    Gamepad1,

    /// <summary>The second gamepad.</summary>
    Gamepad2,

    /// <summary>The third gamepad.</summary>
    Gamepad3,

    /// <summary>The fourth gamepad.</summary>
    Gamepad4,
}
