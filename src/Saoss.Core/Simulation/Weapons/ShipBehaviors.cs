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
    private static readonly ShipBehavior Twinblade = new TwinbladeBehavior();
    private static readonly ShipBehavior Leechmother = new LeechmotherBehavior();
    private static readonly ShipBehavior Trek = new TrekBehavior();
    private static readonly ShipBehavior Ufo = new UfoBehavior();
    private static readonly ShipBehavior Thunderbolt = new ThunderboltBehavior();
    private static readonly ShipBehavior ScatterRay = new ScatterRayBehavior();
    private static readonly ShipBehavior PeaceCannon = new PeaceCannonBehavior();
    private static readonly ShipBehavior Vampire = new VampireBehavior();
    private static readonly ShipBehavior Shapper = new ShapperBehavior();
    private static readonly ShipBehavior Moth = new MothBehavior();
    private static readonly ShipBehavior PowerVacuum = new PowerVacuumBehavior();
    private static readonly ShipBehavior Bulldog = new BulldogBehavior();
    private static readonly ShipBehavior Wiiii = new WiiiiBehavior();
    private static readonly ShipBehavior Elephant = new ElephantBehavior();
    private static readonly ShipBehavior Cubix = new CubixBehavior();
    private static readonly ShipBehavior Electra = new ElectraBehavior();
    private static readonly ShipBehavior Tongu = new TonguBehavior();
    private static readonly ShipBehavior SpaceTurtle = new SpaceTurtleBehavior();
    private static readonly ShipBehavior StickInsect = new StickInsectBehavior();
    private static readonly ShipBehavior StarWorm = new StarWormBehavior();
    private static readonly ShipBehavior Popper = new PopperBehavior();

    /// <summary>Returns the behavior for the ship type.</summary>
    public static ShipBehavior For(ShipType type) => type switch
    {
        ShipType.Magneto => Magneto,
        ShipType.Tringu => Tringu,
        ShipType.Wasp => Wasp,
        ShipType.Twinblade => Twinblade,
        ShipType.Leechmother => Leechmother,
        ShipType.Trek => Trek,
        ShipType.Ufo => Ufo,
        ShipType.Thunderbolt => Thunderbolt,
        ShipType.ScatterRay => ScatterRay,
        ShipType.PeaceCannon => PeaceCannon,
        ShipType.Vampire => Vampire,
        ShipType.Shapper => Shapper,
        ShipType.Moth => Moth,
        ShipType.PowerVacuum => PowerVacuum,
        ShipType.Bulldog => Bulldog,
        ShipType.Wiiii => Wiiii,
        ShipType.Elephant => Elephant,
        ShipType.Cubix => Cubix,
        ShipType.Electra => Electra,
        ShipType.Tongu => Tongu,
        ShipType.SpaceTurtle => SpaceTurtle,
        ShipType.StickInsect => StickInsect,
        ShipType.StarWorm => StarWorm,
        ShipType.Popper => Popper,
        _ => Standard,
    };
}
