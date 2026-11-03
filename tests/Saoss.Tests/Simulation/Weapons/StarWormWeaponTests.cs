using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class StarWormWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesSpinningShotAheadOfHead()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.StarWorm, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        var shot = ship.PrimaryProjectiles[0];
        Assert.IsTrue(shot.IsActive);
        Assert.IsFalse(ship.PrimaryProjectiles[1].IsActive);
        Assert.IsGreaterThan(ship.X + (ship.Definition.Radius * 2f), shot.X);
        Assert.AreEqual(MathF.PI / 45f, shot.Angle, 1e-5f);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_SpraysSparksBackwardFromTailEnd()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.StarWorm, 50f, 50f, angle: 0f);

        // Let the worm crawl until its whole tail lies behind it.
        for (var i = 0; i < 15; i++)
        {
            world.Step();
        }

        ship.Controls = new ShipControls { Special = true };
        world.Step();

        var sparks = ship.SpecialProjectiles.Where(p => p.IsActive).ToList();
        Assert.HasCount(25, sparks);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.SpecialFire));
        Assert.IsTrue(sparks.All(p => p.X < ship.TailX[^1] && p.VelocityX < 0));
    }

    [TestMethod]
    public void Special_SparkHitsShipBehindTail()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.StarWorm, 50f, 50f, angle: 0f);
        for (var i = 0; i < 15; i++)
        {
            world.Step();
        }

        var target = TestWorld.Place(world, 1, ShipType.Elephant, ship.TailX[^1] - 6f, 50f);
        ship.Controls = new ShipControls { Special = true };
        for (var i = 0; i < 10; i++)
        {
            world.Step();
            ship.Controls = default;
        }

        Assert.IsLessThan(target.Definition.MaxEnergy, target.Energy);
    }
}
