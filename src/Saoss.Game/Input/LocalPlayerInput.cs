using Saoss.Core.Simulation;

namespace Saoss.Game.Input;

/// <summary>
/// Turns one player's keys into <see cref="ShipControls"/>. The special button is latched like the original's
/// key-down flag: if the simulation clears it (the Moth does), it stays off until the key is pressed again.
/// </summary>
public sealed class LocalPlayerInput(PlayerKeyBindings bindings)
{
    private bool _isSpecialConsumed;

    /// <summary>Builds this tick's controls from the keyboard.</summary>
    public ShipControls Read(KeyboardTracker keyboard)
    {
        if (!keyboard.IsDown(bindings.Special))
        {
            _isSpecialConsumed = false;
        }

        return new ShipControls
        {
            Thrust = keyboard.IsDown(bindings.Thrust),
            Left = keyboard.IsDown(bindings.Left),
            Right = keyboard.IsDown(bindings.Right),
            Fire = keyboard.IsDown(bindings.Fire),
            Special = keyboard.IsDown(bindings.Special) && !_isSpecialConsumed,
        };
    }

    /// <summary>Call after the simulation step with the controls the ship ended up with.</summary>
    public void AfterStep(ShipControls issued, ShipControls afterStep)
    {
        if (issued.Special && !afterStep.Special)
        {
            _isSpecialConsumed = true;
        }
    }
}
