using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class ShapperWeaponTests
{
    [TestMethod]
    public void Bolt_HurtsShipAheadAndWeakensProjectile()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Shapper, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Vampire, 60f, 50f);
        var shot = target.PrimaryProjectiles[0];
        shot.Lifetime = 100;
        shot.X = 56f;
        shot.Y = 50f;
        shot.Energy = 5f;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Special.Damage, target.Energy, 1e-4f);
        Assert.IsFalse(shot.IsActive);
        Assert.IsTrue(ship.Beams[0].IsActive);
        Assert.IsGreaterThan(0f, ship.Beams[0].VelocityX);
        Assert.IsTrue(ship.IsFireLoopPlaying);
        Assert.AreEqual(ship.Definition.Primary.FireDelay - 1, ship.FireCooldown);
    }

    [TestMethod]
    public void Special_TogglesShieldModeThatFiresShotPairs()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Shapper, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Special = true, Fire = true };

        world.Step();

        Assert.IsTrue(ship.IsShieldActive);
        Assert.IsFalse(ship.Beams[0].IsActive);
        var shots = ship.PrimaryProjectiles.Where(p => p.IsActive).ToList();
        Assert.HasCount(2, shots);
        Assert.AreEqual(0f, shots[0].Energy);
        Assert.AreEqual(ship.Definition.Primary.ProjectileEnergy, shots[1].Energy);
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.SpecialFire));

        ship.SpecialCooldown = 0;
        ship.Controls = new ShipControls { Special = true };
        world.Step();

        Assert.IsFalse(ship.IsShieldActive);
    }
}
