using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class StickInsectWeaponTests
{
    [TestMethod]
    public void Fire_LaunchesFanOfEighteenShots()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.StickInsect, 50f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(18, ship.PrimaryProjectiles.Count(p => p.IsActive));
        Assert.IsFalse(ship.PrimaryProjectiles[18].IsActive);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Fire));

        // The first shot is on the right edge of the fan, the last one on the left, and the middle ones are fastest.
        Assert.IsLessThan(ship.Y, ship.PrimaryProjectiles[0].Y);
        Assert.IsGreaterThan(ship.Y, ship.PrimaryProjectiles[17].Y);
        Assert.IsGreaterThan(Speed(ship.PrimaryProjectiles[0]), Speed(ship.PrimaryProjectiles[9]));
    }

    [TestMethod]
    public void Special_BombBehindShipBurstsIntoFragments()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.StickInsect, 100f, 50f, angle: 0f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;
        var bomb = ship.SpecialProjectiles[0];
        Assert.IsTrue(bomb.IsActive);
        Assert.AreEqual(1f, bomb.Red);
        Assert.IsLessThan(ship.X, bomb.X);
        Assert.AreEqual(1, ship.SpecialProjectiles.Count(p => p.IsActive));

        for (var i = 1; i < ship.Definition.Special.Duration; i++)
        {
            world.Step();
        }

        var fragments = ship.SpecialProjectiles.Where(p => p.IsActive).ToList();
        Assert.HasCount(30, fragments);
        Assert.IsTrue(fragments.All(p => p.Green == 1f && p.Red == 0f));
    }

    private static float Speed(Projectile projectile) =>
        MathF.Sqrt((projectile.VelocityX * projectile.VelocityX) + (projectile.VelocityY * projectile.VelocityY));
}
