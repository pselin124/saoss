using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class PowerVacuumWeaponTests
{
    [TestMethod]
    public void HoldingFire_GrowsBallAndReleaseLaunchesIt()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.PowerVacuum, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };
        for (var i = 0; i < 10; i++)
        {
            world.Step();
        }

        var ball = ship.PrimaryProjectiles[0];
        Assert.IsTrue(ball.IsActive);
        Assert.AreEqual(109f, ball.Energy);
        Assert.IsLessThan(1f, ball.Green);

        ship.Controls = default;
        world.Step();

        var shot = ship.PrimaryProjectiles[1];
        Assert.IsFalse(ball.IsActive);
        Assert.IsTrue(shot.IsActive);
        Assert.AreEqual(109f, shot.Energy);
        Assert.IsGreaterThan(0f, shot.VelocityX);
        Assert.AreEqual(ship.Definition.Primary.FireDelay - 1, ship.FireCooldown);
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_SeekerChasesEnemyAndSlimesIt()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.PowerVacuum, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Moth, 40f, 55f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;

        Assert.AreEqual(1, ship.SpecialProjectiles.Count(p => p.IsActive));
        Assert.AreEqual(ship.Definition.MaxFuel - ship.Definition.Special.FuelCost + ship.Definition.FuelRegen, ship.Fuel, 1e-4f);

        for (var i = 0; i < 100 && target.SlimeCount == 0; i++)
        {
            world.Step();
        }

        Assert.AreEqual(1, target.SlimeCount);
        Assert.IsTrue(target.Slimes[0].IsActive);
        Assert.IsLessThan(1f, target.SlimeSlowdown);
        Assert.IsFalse(ship.SpecialProjectiles.Any(p => p.IsActive));
    }
}
