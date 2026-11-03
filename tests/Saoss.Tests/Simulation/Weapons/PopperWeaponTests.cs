using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class PopperWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesShotAndKicksPopperBack()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Popper, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.IsTrue(ship.PrimaryProjectiles[0].IsActive);
        Assert.IsFalse(ship.PrimaryProjectiles[1].IsActive);
        Assert.IsLessThan(0f, ship.VelocityX);
    }

    [TestMethod]
    public void Fire_ShotHitsShipAndStaysAlive()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Popper, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Popper, 59.5f, 50f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Primary.Damage, target.Energy, 1e-4f);
        Assert.IsTrue(ship.PrimaryProjectiles[0].IsActive);
        Assert.IsGreaterThan(0f, target.VelocityX);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Hit));
    }

    [TestMethod]
    public void Special_MineExplodesLaterAndDamagesByDistance()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Popper, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.SpaceTurtle, 60f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;
        Assert.IsTrue(ship.SpecialProjectiles[0].IsActive);
        ship.X = -100f;
        ship.Y = -80f;

        var explosions = 0;
        for (var i = 1; i < ship.Definition.Special.Duration - 1; i++)
        {
            target.X = 60f;
            target.Y = 50f;
            target.VelocityX = 0;
            target.VelocityY = 0;
            world.Step();
            explosions += world.Events.OfType<EffectEvent>().Count(e => e.Kind == EffectKind.PopperMineExplosion);
        }

        Assert.AreEqual(1, explosions);
        Assert.IsFalse(ship.SpecialProjectiles[0].IsActive);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.SpecialHit));
        Assert.AreEqual(target.Definition.MaxEnergy - (ship.Definition.Special.Damage - 32f), target.Energy, 1e-3f);
        Assert.AreEqual(ship.Definition.MaxEnergy, ship.Energy);
    }

    [TestMethod]
    public void Special_MineHurtsThePopperItself()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Popper, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -80f);
        ship.Controls = new ShipControls { Special = true };

        var exploded = false;
        for (var i = 0; i < ship.Definition.Special.Duration && !exploded; i++)
        {
            ship.X = 50f;
            ship.Y = 50f;
            ship.VelocityX = 0;
            ship.VelocityY = 0;
            world.Step();
            ship.Controls = default;
            exploded = world.Events.OfType<ShipExploded>().Any(e => e.ShipIndex == ship.Index);
        }

        Assert.IsTrue(exploded);
    }
}
