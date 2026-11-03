namespace Saoss.Core.Ships;

/// <summary>
/// Static properties of one ship type, from the original <c>alota()</c> stat blocks.
/// Original field names are given in brackets.
/// </summary>
public sealed record ShipDefinition
{
    /// <summary>Ship type; also selects the weapon behavior.</summary>
    public required ShipType Type { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Mass; scales gravity and collision response [paino].</summary>
    public required float Mass { get; init; }

    /// <summary>Acceleration per tick while thrusting, or speed for inertia-free ships [nopeus].</summary>
    public required float Thrust { get; init; }

    /// <summary>Collision radius [sade].</summary>
    public required float Radius { get; init; }

    /// <summary>Turning speed in degrees per tick [kaantonopeus].</summary>
    public required float TurnRate { get; init; }

    /// <summary>Hit points at spawn [energia].</summary>
    public required float MaxEnergy { get; init; }

    /// <summary>Fuel capacity [fuel].</summary>
    public required float MaxFuel { get; init; }

    /// <summary>Fuel regained per tick [tankkaus].</summary>
    public required float FuelRegen { get; init; }

    /// <summary>Size of the exhaust trail [jalkikoko].</summary>
    public required float TrailSize { get; init; }

    /// <summary>Price in credits games [pistemaara].</summary>
    public required int Cost { get; init; }

    /// <summary>Primary weapon.</summary>
    public required WeaponDefinition Primary { get; init; }

    /// <summary>Special weapon.</summary>
    public required SpecialWeaponDefinition Special { get; init; }

    /// <summary>
    /// Ships that move at a fixed speed without inertia, gravity or friction (Ufo, Electra, Star Worm).
    /// </summary>
    public bool IsInertiaFree => Type is ShipType.Ufo or ShipType.Electra or ShipType.StarWorm;
}
