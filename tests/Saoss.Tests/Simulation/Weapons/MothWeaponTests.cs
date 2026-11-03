using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class MothWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesLeftForwardAndRightShots()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Moth, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        var shots = ship.PrimaryProjectiles.Where(p => p.IsActive).ToList();
        Assert.HasCount(3, shots);
        Assert.IsLessThan(0f, shots[0].VelocityY);
        Assert.IsGreaterThan(0f, shots[1].VelocityX);
        Assert.IsGreaterThan(0f, shots[2].VelocityY);
        Assert.AreEqual(3, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_RefillsAThirdOfTheTank()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Moth, 50f, 50f);
        ship.Fuel = 0;
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(ship.Definition.MaxFuel / 3f, ship.Fuel, 1e-4f);
        Assert.IsFalse(ship.Controls.Special);
        Assert.AreEqual(ship.Definition.Special.Cooldown - 1, ship.SpecialCooldown);
    }
}
