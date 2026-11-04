using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;

namespace Saoss.Tests.Simulation.Modes;

/// <summary>
/// Tests Capture The Flower: picking up, capturing, dropping and returning flowers, and the death penalty.
/// </summary>
[TestClass]
public sealed class CaptureTheFlowerModeTests
{
    [TestMethod]
    public void OnlyTeamsInTheMatch_HaveFlowersInPlay()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.CaptureTheFlower, 0, 2));
        var mode = (CaptureTheFlowerMode)world.Rules;

        Assert.AreEqual(2, mode.Flowers.Count(f => f.IsEnabled));
        Assert.IsTrue(mode.Flowers[2].IsEnabled);
        Assert.AreEqual(FlowerState.AtHome, mode.Flowers[2].State);
    }

    [TestMethod]
    public void EnemyTouchingFlower_PicksItUp_OwnerDoesNot()
    {
        var world = CreateWorld(out var mode);
        var flower = mode.Flowers[0];
        TestWorld.Place(world, 0, ShipType.Elephant, flower.HomeX, flower.HomeY);

        world.Step();
        Assert.AreEqual(FlowerState.AtHome, flower.State);

        TestWorld.Place(world, 1, ShipType.Elephant, flower.HomeX, flower.HomeY);
        world.Step();

        Assert.AreEqual(FlowerState.Carried, flower.State);
        Assert.AreSame(flower, mode.CarriedBy(1));
    }

    [TestMethod]
    public void CarrierReachingOwnBase_ScoresForTeamAndReturnsFlower()
    {
        var world = CreateWorld(out var mode);
        var enemyFlower = mode.Flowers[0];
        var ownBase = mode.Flowers[1];
        TestWorld.Place(world, 1, ShipType.Elephant, enemyFlower.HomeX, enemyFlower.HomeY);
        world.Step();

        TestWorld.Place(world, 1, ShipType.Elephant, ownBase.HomeX, ownBase.HomeY);
        world.Step();

        Assert.AreEqual(CaptureTheFlowerMode.CapturePoints, world.Ships[1].Score);
        Assert.AreEqual(0f, world.Ships[0].Score);
        Assert.AreEqual(FlowerState.AtHome, enemyFlower.State);
    }

    [TestMethod]
    public void CarrierCannotScore_WhileOwnFlowerIsAway()
    {
        var world = CreateWorld(out var mode);
        TestWorld.Place(world, 0, ShipType.Elephant, mode.Flowers[1].HomeX, mode.Flowers[1].HomeY);
        TestWorld.Place(world, 1, ShipType.Elephant, mode.Flowers[0].HomeX, mode.Flowers[0].HomeY);
        world.Step();

        TestWorld.Place(world, 0, ShipType.Elephant, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Elephant, mode.Flowers[1].HomeX, mode.Flowers[1].HomeY);
        world.Step();

        Assert.AreEqual(0f, world.Ships[1].Score);
        Assert.AreSame(mode.Flowers[0], mode.CarriedBy(1));
    }

    [TestMethod]
    public void DestroyedCarrier_DropsFlower_OwnerTouchReturnsIt()
    {
        var world = CreateWorld(out var mode);
        var flower = mode.Flowers[0];
        TestWorld.Place(world, 1, ShipType.Elephant, flower.HomeX, flower.HomeY);
        world.Step();
        var carrier = TestWorld.Place(world, 1, ShipType.Elephant, 60f, 40f);
        carrier.Energy = 0;

        world.Step();
        Assert.AreEqual(FlowerState.Dropped, flower.State);
        Assert.AreEqual(60f, flower.X, 0.5f);

        // Move the wreck away so only the owner touches the flower.
        carrier.X = -60f;
        TestWorld.Place(world, 0, ShipType.Elephant, 60f, 40f);
        world.Step();

        Assert.AreEqual(FlowerState.AtHome, flower.State);
        Assert.AreEqual(flower.HomeX, flower.X);
    }

    [TestMethod]
    public void Death_CostsWholeTeamTwoPoints()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.CaptureTheFlower, 0, 0, 1));
        TestWorld.Place(world, 0, ShipType.Elephant, 50f, 50f);
        TestWorld.Place(world, 1, ShipType.Elephant, -50f, 50f);
        TestWorld.Place(world, 2, ShipType.Elephant, 50f, -50f);
        ModeTestSettings.KillBeforeRespawnPoint(world.Ships[0]);

        world.Step();

        Assert.AreEqual(-2f * CaptureTheFlowerMode.DeathPenalty, world.Ships[0].Score);
        Assert.AreEqual(-2f * CaptureTheFlowerMode.DeathPenalty, world.Ships[1].Score);
        Assert.AreEqual(0f, world.Ships[2].Score);
        Assert.IsTrue(world.Ships[0].IsInGame);
    }

    private static World CreateWorld(out CaptureTheFlowerMode mode)
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.CaptureTheFlower, 0, 1));
        TestWorld.Place(world, 0, ShipType.Elephant, 50f, -50f);
        TestWorld.Place(world, 1, ShipType.Elephant, -50f, 50f);
        mode = (CaptureTheFlowerMode)world.Rules;
        return world;
    }
}
