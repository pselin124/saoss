using Saoss.Core.Simulation;
using Saoss.Game;
using Saoss.Game.Rendering;
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
        CollectionAssert.AreEqual(new[] { "team 2 won" }, layout.Texts.Select(t => t.Text).ToArray());
        Assert.AreEqual(0.5f, layout.ShipScale);
    }

    [TestMethod]
    public void CreditsGame_ShowsPlayersStillInTheGameAndTheirCredits()
    {
        var world = EndedWorld(LocalGameKind.CreditsRandomShips);
        var outcome = world.Outcome!;

        var layout = VictoryLayout.Create(world, outcome);

        CollectionAssert.AreEqual(new[] { 1, 2 }, layout.Ships.Select(s => s.Index).ToArray());
        var expected = new[] { "team 2 won", $"with {outcome.WinnerCredits.ToString(System.Globalization.CultureInfo.InvariantCulture)} credits" };
        CollectionAssert.AreEqual(expected, layout.Texts.Select(t => t.Text).ToArray());
    }

    [TestMethod]
    public void Lines_AreCenteredOverTheShipRow()
    {
        var world = EndedWorld(LocalGameKind.CreditsRandomShips);

        var layout = VictoryLayout.Create(world, world.Outcome!);

        foreach (var text in layout.Texts)
        {
            var inkLeft = text.X - (GlyphMetrics.Advance(text.Text[0]) / 2f);
            var inkRight = text.X + GlyphMetrics.MeasureWidth(text.Text) - (GlyphMetrics.Advance(text.Text[^1]) / 2f);
            Assert.AreEqual(0f, inkLeft + inkRight, 1e-4f, text.Text);
        }
    }

    [TestMethod]
    public void ManyWinners_ShrinkTheShipRow()
    {
        var world = EndedWorld(LocalGameKind.FlowerDomination, winners: 6);

        var layout = VictoryLayout.Create(world, world.Outcome!);

        Assert.AreEqual(6, layout.Ships.Count);
        Assert.AreEqual(0.32f, layout.ShipScale);
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
        Assert.AreEqual("draw", layout.Texts.Single().Text);
        Assert.AreEqual(GlyphMetrics.CenteredStart("draw"), layout.Texts.Single().X);
    }

    [TestMethod]
    public void ShipRow_IsCenteredWithEightUnitSpacing()
    {
        var world = EndedWorld(LocalGameKind.FlowerDomination);

        var layout = VictoryLayout.Create(world, world.Outcome!);

        Assert.AreEqual(-4f, layout.ShipX(0));
        Assert.AreEqual(4f, layout.ShipX(1));
    }

    private static World EndedWorld(LocalGameKind kind, int winners = VictoryPreview.DefaultWinners)
    {
        var world = new World(MatchSettings.ForLocalMenu(kind, VictoryPreview.Players(winners)), seed: 7);
        VictoryPreview.EndOnNextStep(world);
        world.Step();
        Assert.IsNotNull(world.Outcome);
        return world;
    }
}
