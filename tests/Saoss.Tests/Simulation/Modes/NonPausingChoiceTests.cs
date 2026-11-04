using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Modes;

/// <summary>
/// Tests ship choices that do not pause the match (network games, where netValitseAlus kept the match running):
/// buyers wait out of the game while everybody else plays on.
/// </summary>
[TestClass]
public sealed class NonPausingChoiceTests
{
    [TestMethod]
    public void HumansChooseAtTheStart_WhileTheMatchRuns()
    {
        var world = TestWorld.Create(Settings(true, true, false));

        Assert.IsNull(world.PendingChoice);
        Assert.IsTrue(world.IsChoosingShip(0));
        Assert.IsTrue(world.IsChoosingShip(1));
        Assert.IsFalse(world.IsChoosingShip(2));
        Assert.IsFalse(world.Ships[0].IsAlive);
        Assert.IsTrue(world.Ships[2].IsAlive);

        world.Step();
        world.Step();

        Assert.AreEqual(2, world.Tick);
        Assert.AreEqual(GameConstants.SpawnTicks, world.Ships[0].DeathTimer, "A buyer stays out until the purchase.");
    }

    [TestMethod]
    public void AnyWaitingHuman_CanBuyInAnyOrder()
    {
        var world = TestWorld.Create(Settings(true, true));

        Assert.IsTrue(world.ChooseShip(1, ShipType.Ufo));

        Assert.IsTrue(world.Events.OfType<ShipRespawning>().Any(e => e.ShipIndex == 1));
        Assert.IsFalse(world.IsChoosingShip(1));
        Assert.IsTrue(world.IsChoosingShip(0));
        world.Step();
        Assert.AreEqual(ShipType.Ufo, world.Ships[1].Type);
        Assert.AreEqual(GameConstants.SpawnTicks - 1, world.Ships[1].DeathTimer);
    }

    [TestMethod]
    public void RespawningHuman_WaitsAtTheRespawnPoint()
    {
        var world = TestWorld.Create(Settings(true, true));
        world.ChooseShip(0, ShipType.Trek);
        world.ChooseShip(1, ShipType.Trek);
        ModeTestSettings.KillBeforeRespawnPoint(world.Ships[0]);

        world.Step();
        world.Step();

        Assert.IsTrue(world.IsChoosingShip(0));
        Assert.AreEqual(GameConstants.SpawnTicks, world.Ships[0].DeathTimer);
        Assert.AreEqual(2, world.Tick);
    }

    [TestMethod]
    public void BuyerWhoTurnsAi_BuysOnTheNextStep()
    {
        var world = TestWorld.Create(Settings(true, true));
        world.Ships[1].IsHuman = false;

        world.Step();

        Assert.IsFalse(world.IsChoosingShip(1));
        Assert.IsLessThan(MatchSettings.DefaultCredits, world.Ships[1].Score);
        Assert.IsTrue(world.Events.OfType<ShipRespawning>().Any(e => e.ShipIndex == 1));
    }

    [TestMethod]
    public void ChoosingForAShipThatIsNotWaiting_Throws()
    {
        var world = TestWorld.Create(Settings(true, false));

        Assert.ThrowsExactly<InvalidOperationException>(() => world.ChooseShip(1, null));
    }

    private static MatchSettings Settings(params bool[] humans) =>
        ModeTestSettings.Credits(choose: true, humans: humans) with { ChoicesPauseMatch = false };
}
