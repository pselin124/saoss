using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class RayskaWeaponTests
{
    [TestMethod]
    public void Fire_SpraysShotsWithinSpread()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Rayska, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Bloddu, -100f, -100f);
        ship.Controls = new ShipControls { Fire = true };

        for (var i = 0; i < 10; i++)
        {
            world.Step();
        }

        var shots = ship.PrimaryProjectiles.Where(p => p.IsActive).ToList();
        Assert.IsGreaterThanOrEqualTo(4, shots.Count);
        foreach (var shot in shots)
        {
            var direction = MathF.Atan2(shot.VelocityY, shot.VelocityX) * 180f / MathF.PI;
            Assert.IsLessThanOrEqualTo(20.5f, MathF.Abs(direction));
            Assert.AreEqual(ship.Definition.Special.ProjectileEnergy, shot.Energy);
        }
    }

    [TestMethod]
    public void Special_MineHitsShipBehind()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Rayska, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Bloddu, 35f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;
        Assert.AreEqual(1, ship.SpecialProjectiles.Count(p => p.IsActive));

        var impact = false;
        for (var i = 0; i < 200 && !impact; i++)
        {
            world.Step();
            impact = world.Events.OfType<EffectEvent>().Any(e => e.Kind == EffectKind.DriftMineImpact);
        }

        Assert.IsTrue(impact);
        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Special.Damage, target.Energy, 1e-4f);
        Assert.IsLessThan(-0.1f, target.VelocityX);
        Assert.IsFalse(ship.SpecialProjectiles.Any(p => p.IsActive));
    }
}
