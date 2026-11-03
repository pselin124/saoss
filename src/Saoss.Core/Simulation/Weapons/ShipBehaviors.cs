using Saoss.Core.Ships;
using Saoss.Core.Simulation.Weapons.Behaviors;

namespace Saoss.Core.Simulation.Weapons;

/// <summary>
/// Maps each ship type to its weapon behavior. Behaviors are stateless; all state lives in <see cref="Ship"/>.
/// </summary>
public static class ShipBehaviors
{
    private static readonly ShipBehavior Standard = new();
    private static readonly ShipBehavior Magneto = new MagnetoBehavior();

    /// <summary>Returns the behavior for the ship type.</summary>
    public static ShipBehavior For(ShipType type) => type switch
    {
        ShipType.Magneto => Magneto,
        _ => Standard,
    };
}
