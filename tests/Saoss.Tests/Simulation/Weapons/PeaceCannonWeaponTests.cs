using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class PeaceCannonWeaponTests
{
    [TestMethod]
    public void Beam_BurnsShipAndProjectileAheadEveryTick()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.PeaceCannon, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Bloddu, 60f, 50f);
        var shot = target.PrimaryProjectiles[0];
        shot.Lifetime = 100;
        shot.X = 56f;
        shot.Y = 50f;
        shot.Energy = 0.1f;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy - ship.Definition.Primary.Damage, target.Energy, 1e-4f);
        Assert.IsFalse(shot.IsActive);
        Assert.IsTrue(ship.Beams[0].IsActive);
        Assert.IsTrue(ship.IsFireLoopPlaying);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.Fire && e.Loop));

        world.Step();

        Assert.AreEqual(target.Definition.MaxEnergy - (2f * ship.Definition.Primary.Damage), target.Energy, 1e-4f);
        Assert.IsFalse(world.Events.OfType<ShipSoundPlayed>().Any(e => e.Sound == ShipSound.Fire));
    }

    [TestMethod]
    public void Special_BlackHolePullsAndHurtsShip()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.PeaceCannon, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Bloddu, 30f, 50f);
        ship.Controls = new ShipControls { Special = true };

        world.Step();
        ship.Controls = default;

        Assert.AreEqual(1, ship.SpecialProjectiles.Count(p => p.IsActive));
        Assert.IsGreaterThan(0f, target.VelocityX);

        for (var i = 0; i < 200 && target.Energy == target.Definition.MaxEnergy; i++)
        {
            world.Step();
        }

        Assert.IsLessThan(target.Definition.MaxEnergy, target.Energy);
    }
}
