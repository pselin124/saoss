namespace Saoss.Core.Simulation;

/// <summary>
/// A small floating figure that gives one hit point to the ship that touches it [pikkumies].
/// Pickups are created by a special weapon.
/// </summary>
public sealed class Pickup
{
    /// <summary>Position in world units.</summary>
    public float X { get; set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; set; }

    /// <summary>Remaining ticks; zero or less means the slot is free.</summary>
    public int Lifetime { get; set; }

    /// <summary>Ticks before the pickup can be collected.</summary>
    public int Delay { get; set; }

    /// <summary>Whether the pickup is in the arena.</summary>
    public bool IsActive => Lifetime > 0;
}
