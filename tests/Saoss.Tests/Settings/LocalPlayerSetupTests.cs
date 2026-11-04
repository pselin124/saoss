using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Game;
using Saoss.Game.Settings;

namespace Saoss.Tests.Settings;

/// <summary>
/// Tests how the settings become the players and rules of a local match.
/// </summary>
[TestClass]
public sealed class LocalPlayerSetupTests
{
    [TestMethod]
    public void Defaults_MatchTheOriginalSetsFile()
    {
        var players = LocalPlayerSetup.Players(new GameSettings());

        CollectionAssert.AreEqual(
            new[] { new PlayerSetup(Team: 0, IsHuman: true), new PlayerSetup(Team: 2, IsHuman: false), new PlayerSetup(Team: 2, IsHuman: false) },
            players.ToArray());
    }

    [TestMethod]
    public void Players_TakesThePlayerCountFirstSlots()
    {
        var settings = new GameSettings { PlayerCount = 16 };

        var players = LocalPlayerSetup.Players(settings);

        Assert.HasCount(16, players);
        CollectionAssert.AreEqual(settings.Players.Select(p => p.Team).ToArray(), players.Select(p => p.Team).ToArray());
    }

    [TestMethod]
    public void Players_MoreThanFourHumans_ExtraOnesPlayAsCpu()
    {
        var settings = new GameSettings
        {
            PlayerCount = 6,
            Players = [.. Enumerable.Range(0, 16).Select(i => new PlayerSlotSettings(IsHuman: i != 1, Team: i))],
        };

        var players = LocalPlayerSetup.Players(settings);

        CollectionAssert.AreEqual(new[] { true, false, true, true, true, false }, players.Select(p => p.IsHuman).ToArray());
    }

    [TestMethod]
    public void CreateMatch_UsesCreditsAndAiTargeting()
    {
        var settings = new GameSettings { Credits = 650, AiTargetsOnlyEnemies = true, PlayerCount = 4 };

        var match = LocalPlayerSetup.CreateMatch(LocalGameKind.CreditsChooseShips, settings);

        Assert.AreEqual(650, match.Credits);
        Assert.IsTrue(match.AiTargetsOnlyEnemies);
        Assert.IsTrue(match.UsesCredits);
        Assert.IsTrue(match.PlayersChooseShips);
        Assert.HasCount(4, match.Players);
    }

    [TestMethod]
    public void CreateMatch_ByDefault_AiMayAttackTeammates()
    {
        var match = LocalPlayerSetup.CreateMatch(LocalGameKind.RandomShips, new GameSettings());

        Assert.IsFalse(match.AiTargetsOnlyEnemies);
        Assert.AreEqual(GameMode.RandomShipsDeathmatch, match.Mode);
        Assert.AreEqual(400, match.Credits);
    }
}
