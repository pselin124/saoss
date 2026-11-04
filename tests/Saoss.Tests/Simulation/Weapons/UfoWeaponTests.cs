using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class UfoWeaponTests
{
    [TestMethod]
    public void Fire_LasersEnemyInRange()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Ufo, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 60f, 50f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Primary.Damage, target.Energy, 1e-4f);
        var beam = ship.Beams[0];
        Assert.IsTrue(beam.IsActive);
        Assert.AreEqual(1f, beam.Green);
        Assert.AreEqual(1f, beam.Blue);
        Assert.AreEqual(0f, ship.WeaponAngle, 1f);
        Assert.AreEqual(ship.Definition.Primary.FireDelay - 1, ship.FireCooldown);
    }

    [TestMethod]
    public void Fire_DestroysBreakableEnemyShotWhenNoShipInRange()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Ufo, 50f, 50f);
        var enemy = TestWorld.Place(world, 1, ShipType.Vampire, -100f, -100f);
        var shot = enemy.PrimaryProjectiles[0];
        shot.Lifetime = 100;
        shot.X = 50f;
        shot.Y = 60f;
        shot.Energy = 1f;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.IsFalse(shot.IsActive);
        // The gun swings back toward the heading by 1.5 degrees in the same tick.
        Assert.AreEqual(90f, ship.WeaponAngle, 2f);
    }

    [TestMethod]
    public void Fire_WithoutTargetPointsBeamForward()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Ufo, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Vampire, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        var beam = ship.Beams[0];
        Assert.IsTrue(beam.IsActive);
        Assert.AreEqual(50f + ship.Definition.Primary.Radius + 1f, beam.X1, 1e-3f);
        Assert.AreEqual(50f, beam.X2, 1e-3f);
    }

    [TestMethod]
    public void Special_TeleportsAwayFromPlanet()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Ufo, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Vampire, -100f, -100f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.IsFalse(ship.IsTeleportPending);
        Assert.IsTrue(ship.X != 50f || ship.Y != 50f);
        Assert.IsGreaterThanOrEqualTo(20f, MathF.Sqrt((ship.X * ship.X) + (ship.Y * ship.Y)));
        Assert.IsLessThan(ship.Definition.MaxFuel, ship.Fuel);
        Assert.AreEqual(ship.Definition.Special.Cooldown - 1, ship.SpecialCooldown);
    }
}
