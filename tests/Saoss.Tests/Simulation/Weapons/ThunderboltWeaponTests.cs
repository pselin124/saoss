using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class ThunderboltWeaponTests
{
    [TestMethod]
    public void Mine_HomesInOnEnemyAndDealsDoubleDamage()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Thunderbolt, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 50f, 70f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        ship.Controls = default;
        Assert.AreEqual(1, ship.PrimaryProjectiles.Count(p => p.IsActive));

        for (var i = 0; i < 1000 && target.Energy == target.Definition.MaxEnergy; i++)
        {
            world.Step();
        }

        Assert.AreEqual(target.Definition.MaxEnergy - (2f * ship.Definition.Primary.Damage), target.Energy, 1e-4f);
        Assert.IsFalse(ship.PrimaryProjectiles.Any(p => p.IsActive));
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Hit));
    }

    [TestMethod]
    public void Special_CostsNothingWithoutTarget()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Thunderbolt, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Vampire, -100f, -100f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(ship.Definition.MaxFuel, ship.Fuel);
        Assert.AreEqual(0, ship.SpecialCooldown);
        Assert.IsFalse(ship.Beams.Any(b => b.IsActive));
    }

    [TestMethod]
    public void Special_ZapsNearbyEnemy()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Thunderbolt, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 55.5f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Special.Damage, target.Energy, 1e-4f);
        Assert.AreEqual(ship.Definition.Special.Cooldown - 1, ship.SpecialCooldown);
        Assert.IsTrue(ship.Beams.Any(b => b.IsActive));
    }
}
