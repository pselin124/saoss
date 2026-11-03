using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class ElephantWeaponTests
{
    [TestMethod]
    public void Shots_AlternateSidesAndHitForDoubleDamage()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Elephant, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Elephant, 70f, 52f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        ship.Controls = default;

        var shot = ship.PrimaryProjectiles[0];
        Assert.IsTrue(shot.IsActive);
        Assert.IsGreaterThan(ship.Y, shot.Y);
        Assert.AreEqual(-1, ship.FireSide);

        for (var i = 0; i < 100 && shot.IsActive; i++)
        {
            world.Step();
        }

        Assert.AreEqual(target.Definition.MaxEnergy - (2f * ship.Definition.Primary.Damage), target.Energy, 1e-4f);
    }

    [TestMethod]
    public void Special_BombBurstsIntoTwentyThenIntoMore()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Elephant, 60f, 60f, angle: 0f);
        var special = ship.Definition.Special;
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;

        var bomb = ship.SpecialProjectiles[0];
        Assert.IsTrue(bomb.IsActive);
        Assert.IsLessThan(ship.X, bomb.X);

        for (var i = 0; i < special.Duration - 1; i++)
        {
            world.Step();
        }

        var active = ship.SpecialProjectiles.Take(special.MaxProjectiles).Where(p => p.IsActive).ToList();
        Assert.HasCount(20, active);
        Assert.IsTrue(active.All(p => p.Green == 1));

        for (var i = 0; i < special.Duration / 2; i++)
        {
            world.Step();
        }

        active = [.. ship.SpecialProjectiles.Take(special.MaxProjectiles).Where(p => p.IsActive)];
        Assert.IsGreaterThan(20, active.Count);
        Assert.IsTrue(active.All(p => p.Blue == 1));
        Assert.AreEqual(ship.Definition.MaxEnergy, ship.Energy);
    }
}
