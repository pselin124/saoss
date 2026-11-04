using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class TrekWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesOneShotWithRecoil()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Twinblade, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(1, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.IsLessThan(0f, ship.VelocityX);
    }

    [TestMethod]
    public void Special_DronesFlyToEnemyAndZapIt()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Twinblade, 80f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;

        Assert.AreEqual(2, ship.SpecialProjectiles.Count(p => p.IsActive));
        Assert.AreEqual(ship.Definition.MaxEnergy - 2f, ship.Energy);

        var energyBefore = target.Energy;
        for (var i = 0; i < 400 && target.Energy == energyBefore; i++)
        {
            world.Step();
        }

        // Both drones may arrive in the same tick.
        var damage = energyBefore - target.Energy;
        Assert.IsGreaterThan(0f, damage);
        Assert.IsLessThanOrEqualTo((2f * ship.Definition.Special.Damage) + 1e-4f, damage);
        Assert.IsTrue(ship.Beams.Any(b => b.IsActive));
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.SpecialHit));
    }

    [TestMethod]
    public void Special_DronesReturnAndGiveBackEnergy()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Twinblade, -100f, -100f);
        ship.Controls = new ShipControls { Special = true };
        world.Step();
        ship.Controls = default;
        foreach (var drone in ship.SpecialProjectiles.Where(p => p.IsActive))
        {
            drone.Lifetime = 10;
        }

        for (var i = 0; i < 5; i++)
        {
            world.Step();
        }

        Assert.IsFalse(ship.SpecialProjectiles.Any(p => p.IsActive));
        Assert.AreEqual(ship.Definition.MaxEnergy, ship.Energy);
    }
}
