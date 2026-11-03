using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class TringuWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesOneShotAndKicksShipBackwards()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tringu, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Trek, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(1, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.IsLessThan(-0.5f, ship.VelocityX);
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_ReleasesRingThatHitsNearbyEnemy()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tringu, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Trek, 63f, 50f);
        ship.Controls = new ShipControls { Special = true };
        var energyBefore = target.Energy;

        world.Step();

        var ring = ship.Definition.Special.MaxProjectiles;
        var hits = ring - ship.SpecialProjectiles.Take(ring).Count(p => p.IsActive);
        Assert.IsGreaterThan(0, hits);
        Assert.AreEqual(energyBefore - (hits * ship.Definition.Special.Damage), target.Energy, 1e-3f);
        Assert.IsGreaterThan(0f, target.VelocityX);
    }
}
