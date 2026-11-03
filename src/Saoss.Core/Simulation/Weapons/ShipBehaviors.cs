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
    private static readonly ShipBehavior Tringu = new TringuBehavior();
    private static readonly ShipBehavior Wasp = new WaspBehavior();
    private static readonly ShipBehavior Subni = new SubniBehavior();
    private static readonly ShipBehavior Rollu = new RolluBehavior();
    private static readonly ShipBehavior Trek = new TrekBehavior();
    private static readonly ShipBehavior Ufo = new UfoBehavior();
    private static readonly ShipBehavior Bomba = new BombaBehavior();
    private static readonly ShipBehavior Rayska = new RayskaBehavior();
    private static readonly ShipBehavior PeaceCannon = new PeaceCannonBehavior();
    private static readonly ShipBehavior Bloddu = new BlodduBehavior();
    private static readonly ShipBehavior Shapperi = new ShapperiBehavior();
    private static readonly ShipBehavior Moth = new MothBehavior();
    private static readonly ShipBehavior PowerVacuum = new PowerVacuumBehavior();
    private static readonly ShipBehavior Bulldog = new BulldogBehavior();
    private static readonly ShipBehavior Wiiii = new WiiiiBehavior();
    private static readonly ShipBehavior Elephant = new ElephantBehavior();
    private static readonly ShipBehavior Cubix = new CubixBehavior();
    private static readonly ShipBehavior Electra = new ElectraBehavior();
    private static readonly ShipBehavior Tongu = new TonguBehavior();

    /// <summary>Returns the behavior for the ship type.</summary>
    public static ShipBehavior For(ShipType type) => type switch
    {
        ShipType.Magneto => Magneto,
        ShipType.Tringu => Tringu,
        ShipType.Wasp => Wasp,
        ShipType.Subni => Subni,
        ShipType.Rollu => Rollu,
        ShipType.Trek => Trek,
        ShipType.Ufo => Ufo,
        ShipType.Bomba => Bomba,
        ShipType.Rayska => Rayska,
        ShipType.PeaceCannon => PeaceCannon,
        ShipType.Bloddu => Bloddu,
        ShipType.Shapperi => Shapperi,
        ShipType.Moth => Moth,
        ShipType.PowerVacuum => PowerVacuum,
        ShipType.Bulldog => Bulldog,
        ShipType.Wiiii => Wiiii,
        ShipType.Elephant => Elephant,
        ShipType.Cubix => Cubix,
        ShipType.Electra => Electra,
        ShipType.Tongu => Tongu,
        _ => Standard,
    };
}
