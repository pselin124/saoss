using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class LeechmotherWeaponTests
{
    [TestMethod]
    public void Bomb_BurstsIntoFragmentsWhenFireIsReleased()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Leechmother, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Trek, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        Assert.AreEqual(1, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.IsTrue(ship.PrimaryProjectiles[0].IsActive);

        ship.Controls = default;
        world.Step();

        Assert.IsFalse(ship.PrimaryProjectiles[0].IsActive);
        Assert.AreEqual(30, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.IsTrue(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.SpecialFire));
    }

    [TestMethod]
    public void Special_LeechDrainsEnemyFuel()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Leechmother, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Trek, 38f, 50f);
        ship.Controls = new ShipControls { Special = true };
        var drained = false;

        for (var i = 0; i < 300 && !drained; i++)
        {
            world.Step();
            ship.Controls = default;
            drained = target.Fuel < target.Definition.MaxFuel - 1f;
        }

        Assert.IsTrue(drained);
        Assert.AreEqual(1, ship.SpecialProjectiles.Count(p => p.IsActive));
        Assert.AreEqual(target.Definition.MaxEnergy, target.Energy);
    }
}
