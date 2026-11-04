using Saoss.Core.Simulation;

namespace Saoss.Game.Input;

/// <summary>
/// Turns one player's keys or gamepad into <see cref="ShipControls"/>. The special button is latched like the
/// original's key-down flag: if the simulation clears it (the Moth does), it stays off until the button is pressed
/// again.
/// </summary>
public sealed class LocalPlayerInput(PlayerControls controls)
{
    private bool _isSpecialConsumed;

    /// <summary>Builds this tick's controls from the player's device.</summary>
    public ShipControls Read(KeyboardTracker input) => Read(controls.Held(input));

    /// <summary>Builds this tick's controls from the buttons held.</summary>
    public ShipControls Read(PlayerButtons held)
    {
        if (!held.Special)
        {
            _isSpecialConsumed = false;
        }

        return new ShipControls
        {
            Thrust = held.Thrust,
            Left = held.Left,
            Right = held.Right,
            Fire = held.Fire,
            Special = held.Special && !_isSpecialConsumed,
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
