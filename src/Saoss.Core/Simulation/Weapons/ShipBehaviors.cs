using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Weapons;

/// <summary>
/// Maps each ship type to its weapon behavior.
/// </summary>
public static class ShipBehaviors
{
    private static readonly ShipBehavior Unarmed = new();

    /// <summary>Returns the behavior for the ship type.</summary>
    public static ShipBehavior For(ShipType type) => type switch
    {
        _ => Unarmed,
    };
}
