using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class BulldogWeaponTests
{
    [TestMethod]
    public void Rocket_AcceleratesAndHitsForDoubleDamage()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Bulldog, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 70f, 50f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        ship.Controls = default;

        var rocket = ship.PrimaryProjectiles[0];
        Assert.IsTrue(rocket.IsActive);
        var launchSpeed = rocket.VelocityX;
        world.Step();
        Assert.IsGreaterThan(launchSpeed, rocket.VelocityX);

        for (var i = 0; i < 200 && rocket.IsActive; i++)
        {
            world.Step();
        }

        Assert.AreEqual(target.Definition.MaxEnergy - (2f * ship.Definition.Primary.Damage), target.Energy, 1e-4f);
        Assert.IsGreaterThan(0f, target.VelocityX);
    }

    [TestMethod]
    public void Special_BoostsForwardWithExhaust()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Bulldog, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.IsGreaterThan(0.4f, ship.VelocityX);
        Assert.AreEqual(1, world.Events.OfType<EffectEvent>().Count(e => e.Kind == EffectKind.BulldogBoostExhaust));
        Assert.AreEqual(ship.Definition.MaxFuel - ship.Definition.Special.FuelCost + ship.Definition.FuelRegen, ship.Fuel, 1e-4f);
    }
}
