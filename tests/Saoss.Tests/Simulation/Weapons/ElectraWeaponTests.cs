using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class ElectraWeaponTests
{
    [TestMethod]
    public void Bolt_ZapsEnemyInReach()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Elephant, 62f, 50f);
        var weapon = ship.Definition.Primary;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.IsLessThanOrEqualTo(target.Definition.MaxEnergy - (weapon.Damage * 0.5f), target.Energy);
        Assert.IsGreaterThan(target.Definition.MaxEnergy - weapon.Damage, target.Energy);
        Assert.IsFalse(ship.Beams[0].IsActive);
        Assert.IsTrue(ship.Beams.Skip(1).Take(20).All(b => b.IsActive));
        Assert.AreEqual(50f, ship.Beams[1].X1, 1e-4f);
        Assert.AreEqual(62f, ship.Beams[10].X2, 1e-4f);
        Assert.AreEqual(62f, ship.Beams[20].X2, 1e-4f);
        Assert.IsLessThan(ship.Definition.MaxFuel - (weapon.FuelCost / 3f), ship.Fuel);
    }

    [TestMethod]
    public void Bolt_WithoutTarget_PointsAheadForAThirdOfTheFuel()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 90f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -100f);
        var weapon = ship.Definition.Primary;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(ship.Definition.MaxFuel - (weapon.FuelCost / 3f), ship.Fuel, 1e-4f);
        Assert.AreEqual(50f + weapon.Radius + 1f, ship.Beams[10].Y2, 1e-3f);
        Assert.AreEqual(90f, ship.WeaponAngle, 1e-4f);
    }

    [TestMethod]
    public void Bolt_ShootsDownEnemyProjectile()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        var enemy = TestWorld.Place(world, 1, ShipType.Cubix, 100f, 50f, angle: 180f);
        var shot = enemy.PrimaryProjectiles[0];
        shot.Lifetime = 50;
        shot.X = 60f;
        shot.Y = 50f;
        shot.Energy = 1f;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.IsFalse(shot.IsActive);
        Assert.AreEqual(60f, ship.Beams[10].X2, 1e-4f);
    }

    [TestMethod]
    public void Special_EatsNearbyAsteroidAndRefuels()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        ship.Fuel = 5f;
        var asteroid = world.Asteroids.Asteroids[0];
        asteroid.X = 55f;
        asteroid.Y = 50f;
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(ship.Definition.MaxFuel, ship.Fuel, 1e-4f);
        Assert.AreEqual(1, world.Events.OfType<AsteroidExploded>().Count());
        Assert.AreEqual(ship.Definition.Special.Cooldown - 1, ship.SpecialCooldown);
    }

    [TestMethod]
    public void Special_WithoutAsteroid_DoesNothing()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        ship.Fuel = 5f;
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(5f, ship.Fuel, 1e-4f);
        Assert.AreEqual(0, ship.SpecialCooldown);
    }
}
