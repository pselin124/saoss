using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation;

/// <summary>
/// Tests the match-level rules: planet, collisions, pickups, death and respawn, determinism.
/// </summary>
[TestClass]
public sealed class WorldTests
{
    [TestMethod]
    public void Planet_PullsShipTowardCenter()
    {
        var world = TestWorld.Create(players: 1);
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, 30f, 0f);

        world.Step();

        Assert.IsLessThan(0f, ship.VelocityX);
    }

    [TestMethod]
    public void Planet_Contact_CostsEnergyAndBouncesOutward()
    {
        var world = TestWorld.Create(players: 1);
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, 9f, 0f);
        ship.VelocityX = -0.5f;
        var energyBefore = ship.Energy;

        world.Step();

        Assert.AreEqual(energyBefore - GameConstants.PlanetDamage, ship.Energy, 1e-4f);
        Assert.IsGreaterThan(0f, ship.VelocityX);
    }

    [TestMethod]
    public void ShipCollision_ConservesMomentumAndSeparates()
    {
        var world = TestWorld.Create();
        var heavy = TestWorld.Place(world, 0, ShipType.Elephant, 50f, 50f);
        var light = TestWorld.Place(world, 1, ShipType.Cubix, 54f, 50f);
        heavy.VelocityX = 0.3f;
        var momentumBefore = (heavy.Definition.Mass * heavy.VelocityX) + (light.Definition.Mass * light.VelocityX);

        var collided = ShipCollisions.Resolve(heavy, light);

        var momentumAfter = (heavy.Definition.Mass * heavy.VelocityX) + (light.Definition.Mass * light.VelocityX);
        Assert.IsTrue(collided);
        Assert.AreEqual(momentumBefore, momentumAfter, 1e-3f);
        Assert.IsGreaterThan(heavy.VelocityX, light.VelocityX);
    }

    [TestMethod]
    public void BulldogRam_DamagesByFuelShareAndEmptiesFuel()
    {
        var world = TestWorld.Create();
        var bulldog = TestWorld.Place(world, 0, ShipType.Bulldog, 50f, 50f);
        var target = TestWorld.Place(world, 1, ShipType.Trek, 53f, 50f);
        bulldog.Fuel = bulldog.Definition.MaxFuel / 2f;
        var energyBefore = target.Energy;

        ShipCollisions.Resolve(bulldog, target);

        Assert.AreEqual(energyBefore - (bulldog.Definition.Special.Damage / 2f), target.Energy, 1e-4f);
        Assert.AreEqual(0f, bulldog.Fuel);
    }

    [TestMethod]
    public void MagnetoShield_BlocksRamDamage()
    {
        var world = TestWorld.Create();
        var wiiii = TestWorld.Place(world, 0, ShipType.Wiiii, 50f, 50f);
        var magneto = TestWorld.Place(world, 1, ShipType.Magneto, 53f, 50f);
        magneto.IsShieldActive = true;
        var energyBefore = magneto.Energy;

        ShipCollisions.Resolve(wiiii, magneto);

        Assert.AreEqual(energyBefore, magneto.Energy);
    }

    [TestMethod]
    public void Pickup_GivesOneEnergyAndDisappears()
    {
        var world = TestWorld.Create(players: 1);
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 60f, 60f);
        var pickup = world.Pickups[0];
        pickup.X = 60f;
        pickup.Y = 60f;
        pickup.Lifetime = 100;
        var energyBefore = ship.Energy;

        world.Step();

        Assert.AreEqual(energyBefore + 1f, ship.Energy, 1e-4f);
        Assert.IsFalse(pickup.IsActive);
    }

    [TestMethod]
    public void DestroyedShip_ExplodesWaitsAndRespawnsWithFullEnergy()
    {
        var world = TestWorld.Create(players: 1);
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 60f, 60f);
        ship.Energy = 0;

        world.Step();
        Assert.IsFalse(ship.IsAlive);
        Assert.AreEqual(GameConstants.SpawnTicks + GameConstants.DeathTicks, ship.DeathTimer);
        Assert.IsTrue(world.Events.OfType<ShipExploded>().Any());

        StepUntil(world, () => world.Events.OfType<ShipRespawning>().Any(), maxSteps: GameConstants.DeathTicks);
        Assert.AreEqual(GameConstants.SpawnTicks, ship.DeathTimer);
        Assert.IsGreaterThanOrEqualTo(GameConstants.PlanetRadius * 10f, MathF.Sqrt((ship.X * ship.X) + (ship.Y * ship.Y)));

        StepUntil(world, () => ship.IsAlive, maxSteps: GameConstants.SpawnTicks);
        Assert.AreEqual(ship.Definition.MaxEnergy, ship.Energy);
        Assert.AreEqual(0f, ship.VelocityX);
    }

    [TestMethod]
    public void Moth_SometimesComesBackImmediately()
    {
        var revived = false;
        for (var seed = 0; seed < 50 && !revived; seed++)
        {
            var world = TestWorld.Create(players: 1, seed: seed);
            var ship = TestWorld.Place(world, 0, ShipType.Moth, 60f, 60f);
            ship.Energy = 0;

            world.Step();

            revived = world.Events.OfType<ShipRevived>().Any();
            if (revived)
            {
                Assert.IsTrue(ship.IsAlive);
                Assert.AreEqual(ShipType.Moth, ship.Type);
                Assert.AreEqual(ship.Definition.MaxEnergy, ship.Energy);
            }
        }

        Assert.IsTrue(revived);
    }

    [TestMethod]
    public void AsteroidHittingPlanet_BreaksAndRespawnsOutside()
    {
        var world = TestWorld.Create(players: 1);
        TestWorld.Place(world, 0, ShipType.Trek, 100f, 100f);
        var asteroid = world.Asteroids.Asteroids[0];
        asteroid.X = 1f;
        asteroid.Y = 1f;

        world.Step();

        Assert.IsTrue(world.Events.OfType<AsteroidExploded>().Any());
        Assert.IsTrue(MathF.Abs(asteroid.X) > GameConstants.ArenaMaxX || MathF.Abs(asteroid.Y) > GameConstants.ArenaMaxX);
        Assert.AreEqual(GameConstants.AsteroidEnergy, asteroid.Energy);
    }

    [TestMethod]
    public void SameSeedAndInputs_GiveIdenticalMatches()
    {
        var first = new World(TwoPlayers(), seed: 42);
        var second = new World(TwoPlayers(), seed: 42);

        for (var tick = 0; tick < 500; tick++)
        {
            var controls = new ShipControls { Thrust = tick % 3 == 0, Left = tick % 7 < 3 };
            first.Ships[0].Controls = second.Ships[0].Controls = controls;
            first.Step();
            second.Step();
        }

        for (var i = 0; i < 2; i++)
        {
            Assert.AreEqual(first.Ships[i].X, second.Ships[i].X);
            Assert.AreEqual(first.Ships[i].Y, second.Ships[i].Y);
            Assert.AreEqual(first.Ships[i].Type, second.Ships[i].Type);
        }
    }

    private static MatchSettings TwoPlayers() => new()
    {
        Players = [new PlayerSetup(Team: 0, IsHuman: true), new PlayerSetup(Team: 1, IsHuman: false)],
    };

    private static void StepUntil(World world, Func<bool> condition, int maxSteps)
    {
        for (var i = 0; i < maxSteps; i++)
        {
            world.Step();
            if (condition())
            {
                return;
            }
        }

        Assert.Fail($"Condition not reached within {maxSteps} steps.");
    }
}
