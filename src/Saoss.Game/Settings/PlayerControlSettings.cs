using Saoss.Game.Input;

namespace Saoss.Game.Settings;

/// <summary>
/// Saved controls of one local player: the device and the keyboard layout (kept while a gamepad is chosen).
/// </summary>
/// <param name="Device">The keyboard or a gamepad.</param>
/// <param name="Keys">The keyboard layout; null or missing keys mean the player's default ones.</param>
public sealed record PlayerControlSettings(InputDevice Device, PlayerKeyBindings? Keys)
{
    /// <summary>The original layout of local player <paramref name="player"/> (0-3) on the keyboard.</summary>
    public static PlayerControlSettings Default(int player) => new(InputDevice.Keyboard, PlayerKeyBindings.Defaults[player]);

    /// <summary>The controls to play with.</summary>
    public PlayerControls ToControls(int player) => new(Keys ?? PlayerKeyBindings.Defaults[player], Device);

    /// <summary>
    /// A copy for local player <paramref name="player"/> (0-3) with an unknown device replaced by the keyboard and
    /// every unknown or reserved key by the player's default.
    /// </summary>
    public PlayerControlSettings Validated(int player)
    {
        var defaults = PlayerKeyBindings.Defaults[player];
        return new PlayerControlSettings(
            Enum.IsDefined(Device) ? Device : InputDevice.Keyboard,
            Keys?.Validated(defaults) ?? defaults);
    }
}
