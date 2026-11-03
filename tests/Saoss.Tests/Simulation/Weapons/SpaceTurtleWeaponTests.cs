using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class SpaceTurtleWeaponTests
{
    [TestMethod]
    public void Fire_ShotHitsOtherShipForDoubleDamage()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.SpaceTurtle, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.SpaceTurtle, 63f, 50f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();
        ship.Controls = default;
        Assert.IsTrue(ship.PrimaryProjectiles[0].IsActive);
        Assert.IsFalse(ship.PrimaryProjectiles[1].IsActive);

        var hits = 0;
        for (var i = 0; i < 10; i++)
        {
            world.Step();
            hits += world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Hit);
        }

        Assert.AreEqual(1, hits);
        Assert.IsFalse(ship.PrimaryProjectiles[0].IsActive);
        Assert.AreEqual(target.Definition.MaxEnergy - (2f * ship.Definition.Primary.Damage), target.Energy, 1e-4f);
    }

    [TestMethod]
    public void Special_FiresOneRingShotPerTickAllAround()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.SpaceTurtle, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -80f);
        ship.Controls = new ShipControls { Special = true };

        var shots = 0;
        for (var i = 0; i < ship.Definition.Special.Cooldown; i++)
        {
            world.Step();
            ship.Controls = default;
            shots += world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.SpecialFire);
        }

        Assert.AreEqual(ship.Definition.Special.Cooldown - 1, shots);
        Assert.AreEqual(1, ship.FireSide);
        Assert.IsFalse(ship.SpecialProjectiles[0].IsActive);
        Assert.IsTrue(ship.SpecialProjectiles[1].IsActive);
        Assert.IsTrue(ship.SpecialProjectiles[49].IsActive);
        Assert.IsGreaterThan(ship.X, ship.SpecialProjectiles[1].X);
        Assert.IsLessThan(ship.X, ship.SpecialProjectiles[25].X);
    }

    [TestMethod]
    public void Special_RingDamagesNearbyShip()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.SpaceTurtle, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Elephant, 60f, 50f);
        ship.Controls = new ShipControls { Special = true };

        for (var i = 0; i < 10; i++)
        {
            world.Step();
            ship.Controls = default;
        }

        Assert.IsLessThan(target.Definition.MaxEnergy, target.Energy);
    }
}
