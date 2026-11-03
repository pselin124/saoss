namespace Saoss.Core.Ships;

/// <summary>
/// Static properties of a ship's primary weapon. Original field names are given in brackets.
/// </summary>
public sealed record WeaponDefinition
{
    /// <summary>Ticks between shots [ampumisnopeus].</summary>
    public required int FireDelay { get; init; }

    /// <summary>Projectile lifetime in ticks [patiaika].</summary>
    public required int Lifetime { get; init; }

    /// <summary>Maximum simultaneous projectiles [maxpadit].</summary>
    public required int MaxProjectiles { get; init; }

    /// <summary>Projectile speed per tick [patinopeus].</summary>
    public required float Speed { get; init; }

    /// <summary>Projectile hit radius [patisade].</summary>
    public required float Radius { get; init; }

    /// <summary>Damage to a ship on hit [damage].</summary>
    public required float Damage { get; init; }

    /// <summary>Velocity pushed into a ship on hit [patipotkasu].</summary>
    public required float Knockback { get; init; }

    /// <summary>Fuel used per shot [patifuel].</summary>
    public required float FuelCost { get; init; }

    /// <summary>Hit points of one projectile when shot at [patienergia].</summary>
    public required float ProjectileEnergy { get; init; }

    /// <summary>Whether these projectiles damage other projectiles [patihajottaa].</summary>
    public required bool DestroysProjectiles { get; init; }

    /// <summary>Whether these projectiles can be shot down [patiVoiHajota].</summary>
    public required bool CanBeDestroyed { get; init; }
}
