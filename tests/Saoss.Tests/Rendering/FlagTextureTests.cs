using Microsoft.Xna.Framework;
using Saoss.Game.Rendering.Intro;

namespace Saoss.Tests.Rendering;

/// <summary>Tests the painted intro flag picture.</summary>
[TestClass]
public sealed class FlagTextureTests
{
    [TestMethod]
    public void ColorAt_PaintsAWhiteFieldWithABlueCrossAndARedShip()
    {
        var field = FlagTexture.ColorAt(new Vector2(3.5f, 2.6f));
        var crossing = FlagTexture.ColorAt(new Vector2(1.5f, 1.5f));
        var hull = FlagTexture.ColorAt(new Vector2(2.85f, 0.6f));

        Assert.IsTrue(field.R > 230 && field.G > 230 && field.B > 230);
        Assert.IsTrue(crossing.B > 150 && crossing.R < 50);
        Assert.IsTrue(hull.R > 180 && hull.B < 80);
    }

    [TestMethod]
    public void Paint_FillsTheSquareTextureWithSmoothedEdges()
    {
        var pixels = FlagTexture.Paint();

        Assert.AreEqual(FlagTexture.Size * FlagTexture.Size, pixels.Length);
        Assert.IsTrue(pixels.All(p => p.A == 255));

        // Along a row through the field the cross's edges are blended over a pixel instead of stepping.
        var row = pixels.Skip(FlagTexture.Size * FlagTexture.Size / 8).Take(FlagTexture.Size).ToArray();
        Assert.IsTrue(row.Any(p => p.R is > 60 and < 200));
    }
}
