using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class CubixWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesShotFromTheNose()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Cubix, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        var shot = ship.PrimaryProjectiles[0];
        Assert.IsTrue(shot.IsActive);
        Assert.IsFalse(ship.PrimaryProjectiles[1].IsActive);
        Assert.IsGreaterThan(ship.X, shot.X);
        Assert.AreEqual(50f, shot.Y, 0.01f);
        Assert.AreEqual(ship.Definition.Primary.FireDelay - 1, ship.FireCooldown);
    }

    [TestMethod]
    public void Special_DropsMineBehindAndJumpsForward()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Cubix, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        var mine = ship.SpecialProjectiles[0];
        Assert.IsTrue(mine.IsActive);
        Assert.AreEqual(50f - (ship.Definition.Radius * 1.1f), mine.X, 1e-4f);
        Assert.AreEqual(0f, mine.VelocityX);
        Assert.IsGreaterThan(51f, ship.X);
        Assert.IsGreaterThan(0.2f, ship.VelocityX);
    }

    [TestMethod]
    public void Mine_DamagesEnemyThatTouchesIt()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Cubix, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Cubix, 45f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.IsFalse(ship.SpecialProjectiles[0].IsActive);
        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Special.Damage, target.Energy, 1e-4f);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.SpecialHit));
    }
}
