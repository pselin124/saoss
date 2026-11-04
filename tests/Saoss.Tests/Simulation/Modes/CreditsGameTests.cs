using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;

namespace Saoss.Tests.Simulation.Modes;

/// <summary>
/// Tests credits games: buying ships at the start and on respawn, the purchase pause for human players,
/// the AI's purchases, dropping out and the end of the match.
/// </summary>
[TestClass]
public sealed class CreditsGameTests
{
    [TestMethod]
    public void RandomShips_EveryPlayerBuysAShipAtTheStart()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, false, false]));

        Assert.IsNull(world.PendingChoice);
        foreach (var ship in world.Ships)
        {
            Assert.AreEqual(MatchSettings.DefaultCredits - ship.Definition.Cost, ship.Score);
        }
    }

    [TestMethod]
    public void RandomShips_RespawnBuysAnotherShip()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, true]));
        var ship = world.Ships[0];
        var before = ship.Score;
        ModeTestSettings.KillBeforeRespawnPoint(ship);

        world.Step();

        Assert.AreEqual(before - ship.Definition.Cost, ship.Score);
        Assert.IsTrue(world.Events.OfType<ShipRespawning>().Any(e => e.ShipIndex == 0));
    }

    [TestMethod]
    public void PlayerWhoCannotAffordMoreThanTheCheapestShip_DropsOut()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, true, true]));
        var ship = world.Ships[0];
        ship.Score = ShipShop.FallbackShip.Cost;
        ModeTestSettings.KillBeforeRespawnPoint(ship);

        world.Step();

        Assert.IsFalse(ship.IsInGame);
        Assert.AreEqual(-1f, ship.Score);
        Assert.IsNull(world.Outcome);
    }

    [TestMethod]
    public void LastTeamWithPlayers_WinsTheCreditsGame()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, true]));
        var survivor = world.Ships[0];
        world.Ships[1].Score = 0;
        ModeTestSettings.KillBeforeRespawnPoint(world.Ships[1]);

        world.Step();

        Assert.IsNotNull(world.Outcome);
        Assert.AreEqual(0, world.Outcome.WinningTeam);
        Assert.AreEqual((int)survivor.Score + survivor.Definition.Cost, world.Outcome.WinnerCredits);
        Assert.AreEqual(-1f, world.Outcome.FinalScores[1]);
    }

    [TestMethod]
    public void AllPlayersDroppingOut_IsADraw()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, true]));
        foreach (var ship in world.Ships)
        {
            ship.Score = 0;
            ModeTestSettings.KillBeforeRespawnPoint(ship);
        }

        world.Step();

        Assert.IsNotNull(world.Outcome);
        Assert.IsTrue(world.Outcome.IsDraw);
    }

    [TestMethod]
    public void HumanChoice_PausesTheMatchUntilTheShipIsBought()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: true, humans: [true, false]));

        Assert.AreEqual(new ShipChoice(0), world.PendingChoice);
        Assert.AreEqual(MatchSettings.DefaultCredits, world.Ships[1].Score, "The AI waits for its turn.");
        world.Step();
        Assert.AreEqual(0, world.Tick);

        Assert.IsTrue(world.ChooseShip(0, ShipType.Ufo));

        Assert.IsNull(world.PendingChoice);
        Assert.AreEqual(ShipType.Ufo, world.Ships[0].Type);
        Assert.AreEqual(MatchSettings.DefaultCredits - ShipCatalog.Get(ShipType.Ufo).Cost, world.Ships[0].Score);
        Assert.IsLessThan(MatchSettings.DefaultCredits, world.Ships[1].Score);
        world.Step();
        Assert.AreEqual(1, world.Tick);
    }

    [TestMethod]
    public void HumanChoice_UnaffordableShipIsRefused()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: true, credits: 20, humans: [true, true]));

        Assert.IsFalse(world.ChooseShip(0, ShipType.Elephant));

        Assert.AreEqual(new ShipChoice(0), world.PendingChoice);
        Assert.AreEqual(20f, world.Ships[0].Score);
    }

    [TestMethod]
    public void HumanChoice_RandomEntryBuysAnAffordableShip()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: true, credits: 20, humans: [true, true]));

        Assert.IsTrue(world.ChooseShip(0, null));

        Assert.IsGreaterThanOrEqualTo(0f, world.Ships[0].Score);
        Assert.AreEqual(new ShipChoice(1), world.PendingChoice);
    }

    [TestMethod]
    public void HumanRespawn_WaitsForChoiceAndReportsRespawnAfterIt()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: true, humans: [true, true]));
        world.ChooseShip(0, ShipType.Trek);
        world.ChooseShip(1, ShipType.Trek);
        ModeTestSettings.KillBeforeRespawnPoint(world.Ships[1]);

        world.Step();
        Assert.AreEqual(new ShipChoice(1), world.PendingChoice);
        Assert.AreEqual(GameConstants.SpawnTicks, world.Ships[1].DeathTimer);

        world.ChooseShip(1, ShipType.Thunderbolt);
        Assert.IsTrue(world.Events.OfType<ShipRespawning>().Any(e => e.ShipIndex == 1));
        world.Step();

        Assert.AreEqual(ShipType.Thunderbolt, world.Ships[1].Type);
        Assert.AreEqual(GameConstants.SpawnTicks - 1, world.Ships[1].DeathTimer);
    }

    [TestMethod]
    public void AiPlayers_BuyTheirShipsWithoutPausing()
    {
        var world = TestWorld.Create(ModeTestSettings.Credits(choose: true, humans: [false, false, false]));

        Assert.IsNull(world.PendingChoice);
        Assert.IsTrue(world.Ships.All(s => s.Score < MatchSettings.DefaultCredits));
        world.Step();
        Assert.AreEqual(1, world.Tick);
    }

    [TestMethod]
    public void AiChoice_AgainstUfo_PicksUfoMothOrTheRandomEntry()
    {
        for (var seed = 1; seed <= 50; seed++)
        {
            var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, true]), seed);
            TestWorld.Place(world, 0, ShipType.Ufo, 50f, 50f);
            world.Ships[1].Score = MatchSettings.DefaultCredits;

            var pick = ShipShop.AiChoice(world, world.Ships[1]);

            Assert.IsTrue(pick == null || pick.Type is ShipType.Ufo or ShipType.Moth, $"seed {seed}: {pick?.Type}");
        }
    }

    [TestMethod]
    public void AiChoice_PoorAi_AlwaysPicksAnAffordableShip()
    {
        for (var seed = 1; seed <= 50; seed++)
        {
            var world = TestWorld.Create(ModeTestSettings.Credits(choose: false, humans: [true, true]), seed);
            TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f);
            world.Ships[1].Score = 20;

            var pick = ShipShop.AiChoice(world, world.Ships[1]);

            Assert.IsNotNull(pick);
            Assert.IsLessThanOrEqualTo(20, pick.Cost);
        }
    }

    [TestMethod]
    public void ForLocalMenu_MapsMenuEntriesToSettings()
    {
        PlayerSetup[] players = [new(0, true), new(1, false)];

        var credits = MatchSettings.ForLocalMenu(LocalGameKind.CreditsChooseShips, players, 300);
        var randomCredits = MatchSettings.ForLocalMenu(LocalGameKind.CreditsRandomShips, players);
        var ctf = MatchSettings.ForLocalMenu(LocalGameKind.CaptureTheFlower, players);

        Assert.AreEqual(GameMode.Deathmatch, credits.Mode);
        Assert.IsTrue(credits.UsesCredits && credits.PlayersChooseShips);
        Assert.AreEqual(300, credits.Credits);
        Assert.AreEqual(GameMode.RandomShipsDeathmatch, randomCredits.Mode);
        Assert.IsTrue(randomCredits.UsesCredits && !randomCredits.PlayersChooseShips);
        Assert.AreEqual(GameMode.CaptureTheFlower, ctf.Mode);
        Assert.IsFalse(ctf.UsesCredits);
    }

    [TestMethod]
    public void RandomShipsDeathmatchWithoutCredits_NeverEnds()
    {
        var world = TestWorld.Create(ModeTestSettings.Points(GameMode.RandomShipsDeathmatch, 0, 0));

        world.Step();

        Assert.IsNull(world.Outcome);
    }
}
