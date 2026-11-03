using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class SubniWeaponTests
{
    [TestMethod]
    public void Fire_AlternatesBetweenSides()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Subni, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Trek, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        ship.FireCooldown = 0;
        ship.Fuel = ship.Definition.MaxFuel;
        world.Step();

        var first = ship.PrimaryProjectiles[0];
        var second = ship.PrimaryProjectiles[1];
        Assert.IsTrue(first.IsActive);
        Assert.IsTrue(second.IsActive);
        Assert.IsGreaterThan(ship.Y, first.Y);
        Assert.IsLessThan(ship.Y, second.Y);
    }

    [TestMethod]
    public void Special_MineCreepsToEnemyAndExplodes()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Subni, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Trek, 40f, 50f);
        ship.Controls = new ShipControls { Special = true };
        var energyBefore = target.Energy;
        var exploded = false;

        for (var i = 0; i < 100 && !exploded; i++)
        {
            world.Step();
            ship.Controls = default;
            exploded = world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.SpecialHit);
        }

        Assert.IsTrue(exploded);
        Assert.AreEqual(energyBefore - ship.Definition.Special.Damage, target.Energy, 1e-3f);
        Assert.IsFalse(ship.SpecialProjectiles.Any(p => p.IsActive));
    }
}
