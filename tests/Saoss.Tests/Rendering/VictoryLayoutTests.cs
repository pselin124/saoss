using Saoss.Core.Simulation;
using Saoss.Game;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests what the end-of-match screen shows (drawVictory) for matches ended through <see cref="VictoryPreview"/>.
/// </summary>
[TestClass]
public sealed class VictoryLayoutTests
{
    [TestMethod]
    public void PointMode_ShowsWinningTeamMembersAndOneBasedTeam()
    {
        var world = EndedWorld(LocalGameKind.FlowerDomination);

        var layout = VictoryLayout.Create(world, world.Outcome!);

        CollectionAssert.AreEqual(new[] { 1, 2 }, layout.Ships.Select(s => s.Index).ToArray());
        CollectionAssert.AreEqual(new[] { "team", "2", "won" }, layout.Texts.Select(t => t.Text).ToArray());
        Assert.AreEqual(0.5f, layout.ShipScale);
    }

    [TestMethod]
    public void CreditsGame_ShowsPlayersStillInTheGameAndTheirCredits()
    {
        var world = EndedWorld(LocalGameKind.CreditsRandomShips);
        var outcome = world.Outcome!;

        var layout = VictoryLayout.Create(world, outcome);

        CollectionAssert.AreEqual(new[] { 1, 2 }, layout.Ships.Select(s => s.Index).ToArray());
        var expected = new[] { "team", "2", "won", "with", outcome.WinnerCredits.ToString(System.Globalization.CultureInfo.InvariantCulture), "credits" };
        CollectionAssert.AreEqual(expected, layout.Texts.Select(t => t.Text).ToArray());
    }

    [TestMethod]
    public void CreditsGame_WithNobodyLeft_IsADraw()
    {
        var world = EndedWorld(LocalGameKind.CreditsRandomShips);
        foreach (var ship in world.Ships)
        {
            ship.Score = -1;
        }

        var layout = VictoryLayout.Create(world, world.Outcome!);

        Assert.AreEqual(0, layout.Ships.Count);
        Assert.AreEqual(new VictoryText(-4, 0, "draw"), layout.Texts.Single());
    }

    [TestMethod]
    public void ShipRow_IsCenteredWithEightUnitSpacing()
    {
        var world = EndedWorld(LocalGameKind.FlowerDomination);

        var layout = VictoryLayout.Create(world, world.Outcome!);

        Assert.AreEqual(-4f, layout.ShipX(0));
        Assert.AreEqual(4f, layout.ShipX(1));
    }

    private static World EndedWorld(LocalGameKind kind)
    {
        var world = new World(MatchSettings.ForLocalMenu(kind, VictoryPreview.Players), seed: 7);
        VictoryPreview.EndOnNextStep(world);
        world.Step();
        Assert.IsNotNull(world.Outcome);
        return world;
    }
}
