using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Ai;

namespace Saoss.Tests.Simulation;

/// <summary>
/// Tests the computer pilot: target choice, turning, firing, planet avoidance, and a full AI match.
/// </summary>
[TestClass]
public sealed class AiPilotTests
{
    [TestMethod]
    public void TurnsTowardEnemy()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 60f, 60f);
        TestWorld.Place(world, 1, ShipType.Trek, 60f, 90f);

        AiPilot.Update(ship, world);
        Assert.IsTrue(ship.Controls.Left);
        Assert.IsFalse(ship.Controls.Right);

        world.Ships[1].Y = 30f;
        AiPilot.Update(ship, world);
        Assert.IsFalse(ship.Controls.Left);
        Assert.IsTrue(ship.Controls.Right);
    }

    [TestMethod]
    public void FiresOnlyWhenAimedAndInRange()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tringu, 60f, 60f);
        var enemy = TestWorld.Place(world, 1, ShipType.Trek, 80f, 60f);

        AiPilot.Update(ship, world);
        Assert.IsTrue(ship.Controls.Fire);

        enemy.X = 40f;
        AiPilot.Update(ship, world);
        Assert.IsFalse(ship.Controls.Fire);
    }

    [TestMethod]
    public void PrefersFartherEnemyOverNearerTeammate()
    {
        var world = TestWorld.Create(Teams(0, 0, 1));
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 60f, 60f);
        TestWorld.Place(world, 1, ShipType.Trek, 60f, 70f);
        TestWorld.Place(world, 2, ShipType.Trek, 60f, 20f);

        AiPilot.Update(ship, world);

        Assert.IsTrue(ship.Controls.Right);
        Assert.IsFalse(ship.Controls.Left);
    }

    [TestMethod]
    public void TargetsTeammate_WhenNoEnemyAndAllowed()
    {
        var world = TestWorld.Create(Teams(0, 0));
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 60f, 60f);
        TestWorld.Place(world, 1, ShipType.Trek, 60f, 90f);

        AiPilot.Update(ship, world);

        Assert.IsTrue(ship.Controls.Left);
    }

    [TestMethod]
    public void HoldsFire_WhenNoEnemyAndOnlyEnemiesAreTargeted()
    {
        var world = TestWorld.Create(Teams(0, 0) with { AiTargetsOnlyEnemies = true });
        var ship = TestWorld.Place(world, 0, ShipType.Trek, 60f, 60f);
        TestWorld.Place(world, 1, ShipType.Trek, 60f, 90f);
        ship.Controls = new ShipControls { Right = true, Fire = true, Special = true };

        AiPilot.Update(ship, world);

        Assert.IsFalse(ship.Controls.Fire);
        Assert.IsFalse(ship.Controls.Special);
        Assert.IsTrue(ship.Controls.Right, "Turning is left as it was.");
        Assert.IsFalse(ship.Controls.Left);
    }

    [TestMethod]
    public void ThrustsAwayFromPlanet_WhenTooClose()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, 100f, 0f);
        var enemy = TestWorld.Place(world, 1, ShipType.Trek, -100f, 0f);

        AiPilot.Update(ship, world);
        Assert.IsFalse(ship.Controls.Thrust, "The enemy is behind, so a ship far from the planet does not thrust.");

        ship.X = 20f;
        enemy.X = -100f;
        AiPilot.Update(ship, world);
        Assert.IsTrue(ship.Controls.Thrust);
    }

    [TestMethod]
    public void AllAiMatch_RunsAndShipsGetDestroyed()
    {
        var settings = new MatchSettings
        {
            Players = [.. Enumerable.Range(0, 8).Select(i => new PlayerSetup(Team: i, IsHuman: false))],
        };
        var world = new World(settings, seed: 7);
        var explosions = 0;

        for (var tick = 0; tick < 3000; tick++)
        {
            world.Step();
            explosions += world.Events.OfType<ShipExploded>().Count();
            foreach (var ship in world.Ships)
            {
                Assert.IsTrue(float.IsFinite(ship.X) && float.IsFinite(ship.Y), $"Ship {ship.Index} position at tick {tick}.");
                Assert.IsTrue(float.IsFinite(ship.VelocityX) && float.IsFinite(ship.VelocityY), $"Ship {ship.Index} velocity at tick {tick}.");
            }
        }

        Assert.IsGreaterThan(0, explosions);
    }

    private static MatchSettings Teams(params int[] teams) => new()
    {
        Players = [.. teams.Select(team => new PlayerSetup(Team: team, IsHuman: true))],
    };
}
