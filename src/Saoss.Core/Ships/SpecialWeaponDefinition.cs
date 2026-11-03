namespace Saoss.Core.Ships;

/// <summary>
/// Static properties of a ship's special weapon ("ease" in the original). How the values are used
/// depends on the ship type. Original field names are given in brackets.
/// </summary>
public sealed record SpecialWeaponDefinition
{
    /// <summary>Fuel needed to activate, or used per tick for continuous specials [easefuel].</summary>
    public required float FuelCost { get; init; }

    /// <summary>How long the special stays active, in ticks [easekesto].</summary>
    public required int Duration { get; init; }

    /// <summary>Ticks before the special can be used again [easenopeus].</summary>
    public required int Cooldown { get; init; }

    /// <summary>Speed of special projectiles [easevauhti].</summary>
    public required float Speed { get; init; }

    /// <summary>Maximum simultaneous special projectiles [maxepadit].</summary>
    public required int MaxProjectiles { get; init; }

    /// <summary>Damage on hit [edamage].</summary>
    public required float Damage { get; init; }

    /// <summary>Hit radius [epatisade].</summary>
    public required float Radius { get; init; }

    /// <summary>Velocity pushed into a ship on hit [epatipotkasu].</summary>
    public required float Knockback { get; init; }

    /// <summary>Hit points of one special projectile when shot at [epatienergia].</summary>
    public required float ProjectileEnergy { get; init; }

    /// <summary>Whether special projectiles damage other projectiles [epatihajottaa].</summary>
    public required bool DestroysProjectiles { get; init; }

    /// <summary>Whether special projectiles can be shot down [epatiVoiHajota].</summary>
    public required bool CanBeDestroyed { get; init; }
}
