using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Weapons.Behaviors;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class ElectraWeaponTests
{
    [TestMethod]
    public void Bolt_ZapsEnemyInReach()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        var target = TestWorld.Place(world, 1, ShipType.Elephant, 62f, 50f);
        var weapon = ship.Definition.Primary;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.IsLessThanOrEqualTo(target.Definition.MaxEnergy - (weapon.Damage * 0.5f), target.Energy);
        Assert.IsGreaterThan(target.Definition.MaxEnergy - weapon.Damage, target.Energy);
        Assert.IsFalse(ship.Beams[0].IsActive);
        Assert.IsTrue(ship.Beams.Skip(1).Take(20).All(b => b.IsActive));
        Assert.AreEqual(50f, ship.Beams[1].X1, 1e-4f);
        Assert.AreEqual(62f, ship.Beams[10].X2, 1e-4f);
        Assert.AreEqual(62f, ship.Beams[20].X2, 1e-4f);
        Assert.IsLessThan(ship.Definition.MaxFuel - (weapon.FuelCost / 3f), ship.Fuel);
    }

    [TestMethod]
    public void Bolt_WithoutTarget_PointsAheadForAThirdOfTheFuel()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 90f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -100f);
        var weapon = ship.Definition.Primary;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.AreEqual(ship.Definition.MaxFuel - (weapon.FuelCost / 3f) + ship.Definition.FuelRegen, ship.Fuel, 1e-4f);
        Assert.AreEqual(50f + weapon.Radius + 1f, ship.Beams[10].Y2, 1e-3f);
        Assert.AreEqual(90f, ship.WeaponAngle, 1e-4f);
    }

    [TestMethod]
    public void Bolt_ShootsDownEnemyProjectile()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        var enemy = TestWorld.Place(world, 1, ShipType.Cubix, 100f, 50f, angle: 180f);
        var shot = enemy.PrimaryProjectiles[0];
        shot.Lifetime = 50;
        shot.X = 60f;
        shot.Y = 50f;
        shot.Energy = 1f;
        ship.Controls = new ShipControls { Fire = true };

        world.Step();

        Assert.IsFalse(shot.IsActive);
        Assert.AreEqual(60f, ship.Beams[10].X2, 1e-4f);
    }

    [TestMethod]
    public void Asteroid_InReach_IsEatenAutomaticallyForAFullTank()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        ship.Fuel = 5f;
        var near = world.Asteroids.Asteroids[0];
        near.X = 55f;
        near.Y = 50f;
        var alsoNear = world.Asteroids.Asteroids[1];
        alsoNear.X = 50f;
        alsoNear.Y = 55f;

        world.Step();

        Assert.AreEqual(ship.Definition.MaxFuel, ship.Fuel, 1e-4f);
        Assert.AreEqual(1, world.Events.OfType<AsteroidExploded>().Count());
        Assert.AreEqual(0, ship.SpecialCooldown);
    }

    [TestMethod]
    public void Asteroid_InReach_IsLeftAloneWithAFullTank()
    {
        var world = TestWorld.Create();
        TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        var asteroid = world.Asteroids.Asteroids[0];
        asteroid.X = 55f;
        asteroid.Y = 50f;

        world.Step();

        Assert.AreEqual(0, world.Events.OfType<AsteroidExploded>().Count());
    }

    [TestMethod]
    public void Fuel_RegeneratesAtLessThanHalfTheTypicalRate()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -100f);
        ship.Fuel = 5f;

        world.Step();

        var regen = ship.Definition.FuelRegen;
        Assert.AreEqual(5f + regen, ship.Fuel, 1e-5f);
        var others = ShipCatalog.All.Where(d => d.Type != ShipType.Electra && d.FuelRegen > 0).Select(d => d.FuelRegen).Order().ToList();
        var median = (others[(others.Count - 1) / 2] + others[others.Count / 2]) / 2f;
        Assert.IsGreaterThan(0f, regen);
        Assert.IsLessThan(median / 2f, regen);
    }

    [TestMethod]
    public void Special_TriplesSpeedForItsDuration()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -100f);
        var special = ship.Definition.Special;
        ship.Controls = new ShipControls { Special = true };

        world.Step();

        Assert.AreEqual(ship.Definition.Thrust * ElectraBehavior.BoostSpeedFactor, ship.VelocityX, 1e-5f);
        Assert.AreEqual(special.Duration - 1, ship.SpecialTimeLeft);
        Assert.AreEqual(special.Cooldown - 1, ship.SpecialCooldown);
        Assert.AreEqual(ship.Definition.MaxFuel - special.FuelCost + ship.Definition.FuelRegen, ship.Fuel, 1e-4f);
        Assert.AreEqual(1, world.Events.OfType<ShipSoundPlayed>().Count(e => e.Sound == ShipSound.SpecialFire));

        ship.Controls = new ShipControls();
        for (var i = 1; i < special.Duration; i++)
        {
            world.Step();
        }

        Assert.AreEqual(0, ship.SpecialTimeLeft);
        Assert.AreEqual(ship.Definition.Thrust, ship.VelocityX, 1e-5f);
    }

    [TestMethod]
    public void BoostEnd_ChainLightningJumpsBetweenEnemiesGettingWeaker()
    {
        var world = TestWorld.Create(players: 5);
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 90f);
        var first = TestWorld.Place(world, 1, ShipType.Elephant, 60f, 50f);
        var second = TestWorld.Place(world, 2, ShipType.Elephant, 75f, 50f);
        var third = TestWorld.Place(world, 3, ShipType.Elephant, 90f, 50f);
        var outOfReach = TestWorld.Place(world, 4, ShipType.Elephant, 150f, 50f);
        var special = ship.Definition.Special;
        ship.SpecialTimeLeft = 1;

        world.Step();

        var max = first.Definition.MaxEnergy;
        Assert.AreEqual(max - special.Damage, first.Energy, 1e-3f);
        Assert.AreEqual(max - (special.Damage * ElectraBehavior.ChainFalloff), second.Energy, 1e-3f);
        Assert.AreEqual(max - (special.Damage * ElectraBehavior.ChainFalloff * ElectraBehavior.ChainFalloff), third.Energy, 1e-3f);
        Assert.AreEqual(max, outOfReach.Energy, 1e-3f);

        var segments = ElectraBehavior.ChainSegments;
        var start = ElectraBehavior.FirstChainBeam;
        Assert.AreEqual(50f, ship.Beams[start].X1, 1e-4f);
        Assert.AreEqual(60f, ship.Beams[start + segments - 1].X2, 1e-4f);
        Assert.AreEqual(75f, ship.Beams[start + (2 * segments) - 1].X2, 1e-4f);
        Assert.AreEqual(90f, ship.Beams[start + (3 * segments) - 1].X2, 1e-4f);
        Assert.IsTrue(ship.Beams.Skip(start).Take(3 * segments).All(b => b.IsActive));
        Assert.IsFalse(ship.Beams[start + (3 * segments)].IsActive);
    }

    [TestMethod]
    public void BoostEnd_ShieldedMagnetoTakesNoDamageButPassesTheChainOn()
    {
        var world = TestWorld.Create(players: 3);
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 90f);
        var magneto = TestWorld.Place(world, 1, ShipType.Magneto, 60f, 50f);
        magneto.IsShieldActive = true;
        magneto.SpecialTimeLeft = 50;
        var behind = TestWorld.Place(world, 2, ShipType.Elephant, 75f, 50f);
        ship.SpecialTimeLeft = 1;

        world.Step();

        Assert.AreEqual(magneto.Definition.MaxEnergy, magneto.Energy, 1e-3f);
        Assert.AreEqual(behind.Definition.MaxEnergy - (ship.Definition.Special.Damage * ElectraBehavior.ChainFalloff), behind.Energy, 1e-3f);
    }

    [TestMethod]
    public void BoostEnd_WithoutShipsInReach_ShootsDownEnemyProjectile()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 90f);
        var enemy = TestWorld.Place(world, 1, ShipType.Cubix, 150f, 50f, angle: 180f);
        var shot = enemy.PrimaryProjectiles[0];
        shot.Lifetime = 50;
        shot.X = 65f;
        shot.Y = 50f;
        shot.Energy = 1f;
        ship.SpecialTimeLeft = 1;

        world.Step();

        Assert.IsFalse(shot.IsActive);
        Assert.AreEqual(65f, ship.Beams[ElectraBehavior.FirstChainBeam + ElectraBehavior.ChainSegments - 1].X2, 1e-4f);
    }

    [TestMethod]
    public void BoostEnd_WithNothingInReach_CracklesAhead()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        TestWorld.Place(world, 1, ShipType.Elephant, -100f, -100f);
        ship.SpecialTimeLeft = 1;

        world.Step();

        var start = ElectraBehavior.FirstChainBeam;
        var segments = ElectraBehavior.ChainSegments;
        Assert.AreEqual(50f, ship.Beams[start].X1, 1e-4f);
        Assert.AreEqual(50f + (ship.Definition.Special.Radius / 3f), ship.Beams[start + segments - 1].X2, 1e-4f);
        Assert.IsFalse(ship.Beams[start + segments].IsActive);
    }

    [TestMethod]
    public void Respawn_CancelsABoostInProgress()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);
        ship.SpecialTimeLeft = 50;

        ship.Respawn(ShipCatalog.Get(ShipType.Electra), world.Random);

        Assert.AreEqual(0, ship.SpecialTimeLeft);
        Assert.IsFalse(ElectraBehavior.IsBoosting(ship));
    }
}
