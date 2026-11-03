using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

/// <summary>
/// Tests the shared weapon rules with Magneto as the example ship.
/// </summary>
[TestClass]
public sealed class MagnetoWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesTwoShotsAndUsesCooldownAndFuel()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Trek, -100f, -100f);
        var fuelBefore = ship.Fuel;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(2, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.AreEqual(fuelBefore - ship.Definition.Primary.FuelCost + ship.Definition.FuelRegen, ship.Fuel, 1e-4f);
        Assert.AreEqual(ship.Definition.Primary.FireDelay - 1, ship.FireCooldown);
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Shot_HitsEnemy_DamagesPushesAndDisappears()
    {
        var world = TestWorld.Create();
        var shooter = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Trek, 58f, 50f);
        shooter.Controls = new ShipControls { Fire = true };
        var energyBefore = target.Energy;

        for (var i = 0; i < 30 && target.Energy == energyBefore; i++)
        {
            world.Step();
            shooter.Controls = default;
        }

        Assert.IsLessThan(energyBefore, target.Energy);
        Assert.IsGreaterThan(0f, target.VelocityX);
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Hit));
    }

    [TestMethod]
    public void Special_RaisesShieldThatBlocksShotsUntilItExpires()
    {
        var world = TestWorld.Create();
        var magneto = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f);
        magneto.Controls = new ShipControls { Special = true };

        world.Step();
        magneto.Controls = default;

        Assert.IsTrue(magneto.IsShieldActive);
        var shot = new Projectile { X = magneto.X, Y = magneto.Y, Lifetime = 10 };
        Assert.IsFalse(Saoss.Core.Simulation.Weapons.Combat.Touches(magneto, shot, 1f));

        for (var i = 0; i < magneto.Definition.Special.Duration; i++)
        {
            world.Step();
        }

        Assert.IsFalse(magneto.IsShieldActive);
    }

    [TestMethod]
    public void OpposingShots_DestroyEachOther()
    {
        var world = TestWorld.Create();
        var first = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f);
        var second = TestWorld.Place(world, 1, ShipType.Magneto, 50f, -50f);
        var a = first.PrimaryProjectiles[0];
        var b = second.PrimaryProjectiles[0];
        a.X = b.X = 0f;
        a.Y = b.Y = 90f;
        a.Lifetime = b.Lifetime = 50;
        a.Energy = b.Energy = first.Definition.Primary.ProjectileEnergy;

        for (var i = 0; i < 3; i++)
        {
            world.Step();
        }

        // Each Magneto shot has 3 energy and deals 1 damage per tick of contact.
        Assert.IsFalse(a.IsActive);
        Assert.IsFalse(b.IsActive);
    }
}
