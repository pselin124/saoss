using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class WiiiiWeaponTests
{
    [TestMethod]
    public void Fire_DropsStillMineNextToShip()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Wiiii, 50f, 50f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        var mine = ship.PrimaryProjectiles[0];
        Assert.IsTrue(mine.IsActive);
        Assert.AreEqual(0f, mine.VelocityX);
        Assert.IsLessThan(1.1f, MathF.Abs(mine.X - 50f));
        Assert.IsLessThan(1.1f, MathF.Abs(mine.Y - 50f));
    }

    [TestMethod]
    public void Mine_HitsOtherShipForDoubleDamage()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Wiiii, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 70f, 50f);
        var mine = ship.PrimaryProjectiles[0];
        mine.Lifetime = 100;
        mine.X = 70f;
        mine.Y = 50f;

        world.Step();

        Assert.IsFalse(mine.IsActive);
        Assert.AreEqual(target.Definition.MaxEnergy - (2f * ship.Definition.Primary.Damage), target.Energy, 1e-4f);
    }

    [TestMethod]
    public void Special_StartsBoostThatBlocksFiring()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Wiiii, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Special = true, Fire = true };

        world.Step();

        Assert.IsTrue(ship.IsShieldActive);
        Assert.IsFalse(ship.PrimaryProjectiles.Any(p => p.IsActive));
        Assert.IsGreaterThan(0f, ship.VelocityX);
    }
}
