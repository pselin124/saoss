using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class VampireWeaponTests
{
    [TestMethod]
    public void Fire_ShotHitsEnemy()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Vampire, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Thunderbolt, 60f, 50f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        ship.Controls = default;
        Assert.AreEqual(ship.Definition.Special.ProjectileEnergy, ship.PrimaryProjectiles[0].Energy);

        for (var i = 0; i < 100 && target.Energy == target.Definition.MaxEnergy; i++)
        {
            world.Step();
        }

        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Primary.Damage, target.Energy, 1e-4f);
    }

    [TestMethod]
    public void Special_BitesNearbyEnemyIntoPickups()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Vampire, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 55f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        var bitten = target.Definition.MaxEnergy - target.Energy;
        Assert.IsGreaterThanOrEqualTo(1f, bitten);
        Assert.AreEqual((int)bitten, world.Pickups.Count(p => p.IsActive));
        Assert.IsTrue(world.Pickups.Where(p => p.IsActive).All(p => p.Delay == 69));
    }

    [TestMethod]
    public void Special_DoesNothingToFarEnemy()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Vampire, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 80f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy, target.Energy);
        Assert.AreEqual(0, world.Pickups.Count(p => p.IsActive));
        Assert.AreEqual(ship.Definition.Special.Cooldown - 1, ship.SpecialCooldown);
    }
}
