using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class WaspWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesOneShot()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Wasp, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Trek, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(1, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_SelfDestructsDamagingNearbyShipsAndProjectiles()
    {
        var world = TestWorld.Create();
        var wasp = TestWorld.Place(world, 0, ShipType.Wasp, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Trek, 60f, 50f);
        var shot = target.PrimaryProjectiles[0];
        shot.X = 55f;
        shot.Y = 50f;
        shot.Lifetime = 50;
        shot.Energy = target.Definition.Primary.ProjectileEnergy;
        wasp.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.IsFalse(wasp.IsAlive);
        Assert.IsTrue(world.Events.OfType<ShipExploded>().Any(e => e.ShipIndex == wasp.Index));

        // 100 damage minus 4.5 per unit of distance (10).
        Assert.AreEqual(target.Definition.MaxEnergy - 55f, target.Energy, 1e-3f);
        Assert.IsFalse(shot.IsActive);
    }
}
