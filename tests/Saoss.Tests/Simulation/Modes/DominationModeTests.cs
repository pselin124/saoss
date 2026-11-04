using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation.Modes;

namespace Saoss.Tests.Simulation.Modes;

/// <summary>
/// Tests Flower Domination: taking, contesting and scoring control points, and the end of the match.
/// </summary>
[TestClass]
public sealed class DominationModeTests
{
    [TestMethod]
    public void ShipAloneAtPoint_TakesItAndTeamScoresEachTick()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.FlowerDomination, 0, 0, 1));
        var mode = (DominationMode)world.Rules;
        var point = mode.Points[0];
        TestWorld.Place(world, 0, ShipType.Elephant, point.X, point.Y);
        TestWorld.Place(world, 1, ShipType.Elephant, 50f, 50f);
        TestWorld.Place(world, 2, ShipType.Elephant, -50f, 50f);

        world.Step();
        world.Step();

        Assert.AreEqual(0, point.OwnerTeam);
        Assert.AreEqual(2 * DominationMode.PointsPerTick, world.Ships[0].Score, 1e-6f);
        Assert.AreEqual(2 * DominationMode.PointsPerTick, world.Ships[1].Score, 1e-6f);
        Assert.AreEqual(0f, world.Ships[2].Score);
    }

    [TestMethod]
    public void ContestedPoint_KeepsItsOwner()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.FlowerDomination, 0, 1));
        var point = ((DominationMode)world.Rules).Points[1];
        point.OwnerTeam = 0;
        TestWorld.Place(world, 0, ShipType.Elephant, point.X + 3f, point.Y);
        TestWorld.Place(world, 1, ShipType.Elephant, point.X - 3f, point.Y);

        world.Step();

        Assert.AreEqual(0, point.OwnerTeam);
        Assert.AreEqual(0f, world.Ships[1].Score);
    }

    [TestMethod]
    public void ReachingTargetScore_EndsMatchWithThatTeamWinning()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.FlowerDomination, 0, 1));
        var point = ((DominationMode)world.Rules).Points[0];
        TestWorld.Place(world, 0, ShipType.Elephant, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Elephant, point.X, point.Y);
        world.Ships[1].Score = world.Settings.Credits - (DominationMode.PointsPerTick / 2f);

        world.Step();
        var tick = world.Tick;
        world.Step();

        Assert.IsNotNull(world.Outcome);
        Assert.AreEqual(1, world.Outcome.WinningTeam);
        Assert.AreEqual(tick, world.Tick);
    }

    [TestMethod]
    public void Death_CostsNoPoints()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.FlowerDomination, 0, 1));
        TestWorld.Place(world, 0, ShipType.Elephant, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Elephant, -50f, 50f);
        ModeTestSettings.KillBeforeRespawnPoint(world.Ships[0]);

        world.Step();

        Assert.AreEqual(0f, world.Ships[0].Score);
        Assert.AreEqual(GameConstants.SpawnTicks, world.Ships[0].DeathTimer);
    }
}
