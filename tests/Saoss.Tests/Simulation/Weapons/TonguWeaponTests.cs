using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class TonguWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesTwoShotsFromBothSidesAndBuildsSpread()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tongu, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        var left = ship.PrimaryProjectiles[0];
        var right = ship.PrimaryProjectiles[1];
        Assert.IsTrue(left.IsActive);
        Assert.IsTrue(right.IsActive);
        Assert.IsFalse(ship.PrimaryProjectiles[2].IsActive);
        Assert.IsGreaterThan(50f, left.Y);
        Assert.IsLessThan(50f, right.Y);
        Assert.IsLessThanOrEqualTo(ship.Definition.Primary.ProjectileEnergy, left.Energy);
        Assert.AreEqual(7f - 0.3f, ship.WeaponCharge, 1e-4f);
        Assert.AreEqual(2, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_TongueHitsShipInFrontOnce()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tongu, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Elephant, 58f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;
        Assert.AreEqual(2, ship.FireSide);

        var hits = 0;
        for (var i = 0; i < ship.Definition.Special.Cooldown; i++)
        {
            world.Step();
            hits += world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.SpecialHit);
        }

        Assert.AreEqual(1, hits);
        Assert.AreEqual(1, ship.FireSide);
        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Special.Damage, target.Energy, 1e-4f);
    }

    [TestMethod]
    public void Special_TongueMissesShipOutOfReach()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tongu, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Elephant, 70f, 50f);
        ship.Controls = new ShipControls { Special = true };

        for (var i = 0; i <= ship.Definition.Special.Cooldown; i++)
        {
            world.Step();
            ship.Controls = default;
        }

        Assert.AreEqual(target.Definition.MaxEnergy, target.Energy);
        Assert.AreEqual(2, ship.FireSide);
    }
}
