using Microsoft.Xna.Framework;
using Saoss.Core.Ships;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Tests.Rendering;

/// <summary>Tests the screen-space quads that stand in for glLineWidth above one pixel.</summary>
[TestClass]
public sealed class GlWideLineTests
{
    private const float Tolerance = 1e-5f;
    private static readonly Vector2 Viewport = new(200, 100);

    [TestMethod]
    public void HorizontalLine_IsWidenedVerticallyByHalfTheWidthEachSide()
    {
        var quad = GlWideLine.Expand(new Vector4(-0.5f, 0, 0, 1), new Vector4(0.5f, 0, 0, 1), 4f, Viewport)!.Value;

        // 2 pixels of a 100-pixel-high viewport are 0.04 in normalized device coordinates.
        AssertClose(new Vector4(-0.5f, -0.04f, 0, 1), quad.StartRight);
        AssertClose(new Vector4(-0.5f, 0.04f, 0, 1), quad.StartLeft);
        AssertClose(new Vector4(0.5f, 0.04f, 0, 1), quad.EndLeft);
        AssertClose(new Vector4(0.5f, -0.04f, 0, 1), quad.EndRight);
    }

    [TestMethod]
    public void DiagonalLine_IsPerpendicularOnScreen()
    {
        // 50 pixels right and 50 pixels up on the 200x100 viewport.
        var quad = GlWideLine.Expand(new Vector4(0, 0, 0, 1), new Vector4(0.5f, 1f, 0, 1), 2f, Viewport)!.Value;

        var acrossPixels = (new Vector2(quad.StartLeft.X, quad.StartLeft.Y) - new Vector2(quad.StartRight.X, quad.StartRight.Y)) * Viewport / 2f;
        var alongPixels = new Vector2(50, 50);
        Assert.AreEqual(0f, Vector2.Dot(acrossPixels, alongPixels), 1e-3f);
        Assert.AreEqual(2f, acrossPixels.Length(), 1e-4f);
    }

    [TestMethod]
    public void Offsets_ScaleWithW_SoThePixelWidthSurvivesThePerspectiveDivide()
    {
        var quad = GlWideLine.Expand(new Vector4(-1f, 0, 0.5f, 2), new Vector4(1f, 0, 0.5f, 2), 4f, Viewport)!.Value;

        Assert.AreEqual(0.08f, quad.StartLeft.Y, Tolerance);
        Assert.AreEqual(0.04f, quad.StartLeft.Y / quad.StartLeft.W, Tolerance);
        Assert.AreEqual(0.5f, quad.StartLeft.Z, Tolerance);
    }

    [TestMethod]
    public void BehindTheCameraOrZeroLength_IsNotExpanded()
    {
        Assert.IsNull(GlWideLine.Expand(new Vector4(0, 0, 0, -1), new Vector4(1, 0, 0, 1), 2f, Viewport));
        Assert.IsNull(GlWideLine.Expand(new Vector4(0.3f, 0.3f, 0, 1), new Vector4(0.6f, 0.6f, 0, 2), 2f, Viewport));
    }

    [TestMethod]
    public void ScaledWidth_FollowsTheWindowHeight()
    {
        Assert.AreEqual(2f, GlWideLine.ScaledWidth(2f, 768), Tolerance);
        Assert.AreEqual(2.8125f, GlWideLine.ScaledWidth(2f, 1080), Tolerance);
    }

    [TestMethod]
    public void BeamWidths_MatchTheOriginal()
    {
        Assert.AreEqual(2f, ProjectileRenderer.BeamWidth(ShipType.Ufo));
        Assert.AreEqual(2f, ProjectileRenderer.BeamWidth(ShipType.PeaceCannon));
        Assert.AreEqual(2f, ProjectileRenderer.BeamWidth(ShipType.Shapperi));
        Assert.AreEqual(1.5f, ProjectileRenderer.BeamWidth(ShipType.Electra));
        Assert.AreEqual(1f, ProjectileRenderer.BeamWidth(ShipType.Magneto));
    }

    private static void AssertClose(Vector4 expected, Vector4 actual) =>
        Assert.IsTrue(Vector4.Distance(expected, actual) < Tolerance, $"expected {expected}, got {actual}");
}
