using Saoss.Core;
using Saoss.Game.Diagnostics;
using Saoss.Game.Settings;

namespace Saoss.Tests.Diagnostics;

/// <summary>Tests the benchmark match and the crash log entry.</summary>
[TestClass]
public sealed class BenchmarkAndCrashTests
{
    [TestMethod]
    public void BenchmarkMatch_IsAllAiOnSeparateTeams()
    {
        var settings = BenchmarkMatch.Create(new GameSettings(), players: 16);

        Assert.HasCount(16, settings.Players);
        Assert.IsTrue(settings.Players.All(p => !p.IsHuman));
        Assert.HasCount(16, settings.Players.Select(p => p.Team).Distinct());
    }

    [TestMethod]
    public void BenchmarkMatch_ClampsThePlayerCount()
    {
        Assert.HasCount(GameSettings.MinPlayers, BenchmarkMatch.Create(new GameSettings(), players: 1).Players);
        Assert.HasCount(GameSettings.MaxPlayers, BenchmarkMatch.Create(new GameSettings(), players: 99).Players);
    }

    [TestMethod]
    public void BenchmarkMatch_IsADeathmatchWithoutCredits()
    {
        var settings = BenchmarkMatch.Create(new GameSettings(), players: 4);

        // AI pilots cannot pick ships, so the plain deathmatch is the menu's "Start With Random Ships".
        Assert.AreEqual(GameMode.RandomShipsDeathmatch, settings.Mode);
        Assert.IsFalse(settings.UsesCredits);
    }

    [TestMethod]
    public void CrashEntry_HoldsTimeAndException()
    {
        var entry = CrashReporter.FormatEntry(new InvalidOperationException("boom"), new DateTime(2026, 1, 2, 3, 4, 5));

        StringAssert.StartsWith(entry, "==== 2026-01-02 03:04:05 Saoss ");
        StringAssert.Contains(entry, "System.InvalidOperationException: boom");
    }
}
