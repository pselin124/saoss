using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation.Modes;

namespace Saoss.Tests.Simulation.Modes;

/// <summary>
/// Tests Eat The Mushroom: eating, the mushroom's new position, the death penalty and the end of the match.
/// </summary>
[TestClass]
public sealed class MushroomModeTests
{
    [TestMethod]
    public void EatingMushroom_ScoresForTeamAndMovesItAwayFromPlanet()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.EatTheMushroom, 0, 0, 1));
        var mode = (MushroomMode)world.Rules;
        TestWorld.Place(world, 0, ShipType.Elephant, mode.X, mode.Y);
        TestWorld.Place(world, 1, ShipType.Elephant, 60f, -60f);
        TestWorld.Place(world, 2, ShipType.Elephant, -60f, -60f);

        world.Step();

        Assert.AreEqual(MushroomMode.EatPoints, world.Ships[0].Score);
        Assert.AreEqual(MushroomMode.EatPoints, world.Ships[1].Score);
        Assert.AreEqual(0f, world.Ships[2].Score);
        Assert.IsGreaterThanOrEqualTo(500, (mode.X * mode.X) + (mode.Y * mode.Y));
        Assert.IsLessThan(GameConstants.ArenaMaxX, Math.Abs(mode.X));
    }

    [TestMethod]
    public void Death_CostsWholeTeamOnePoint()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.EatTheMushroom, 0, 1));
        TestWorld.Place(world, 0, ShipType.Elephant, 60f, -60f);
        TestWorld.Place(world, 1, ShipType.Elephant, -60f, -60f);
        ModeTestSettings.KillBeforeRespawnPoint(world.Ships[1]);

        world.Step();

        Assert.AreEqual(-MushroomMode.DeathPenalty, world.Ships[1].Score);
        Assert.AreEqual(0f, world.Ships[0].Score);
    }

    [TestMethod]
    public void ReachingTargetScore_EndsMatch()
    {
        var settings = ModeTestSettings.Points(GameMode.EatTheMushroom, 0, 1) with { Credits = 10 };
        var world = TestWorld.Create(settings);
        var mode = (MushroomMode)world.Rules;
        TestWorld.Place(world, 0, ShipType.Elephant, 60f, -60f);
        TestWorld.Place(world, 1, ShipType.Elephant, mode.X, mode.Y);

        world.Step();

        Assert.IsNotNull(world.Outcome);
        Assert.AreEqual(1, world.Outcome.WinningTeam);
        Assert.IsFalse(world.Outcome.IsDraw);
    }
}
