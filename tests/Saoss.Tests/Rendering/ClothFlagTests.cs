using Microsoft.Xna.Framework;
using Saoss.Game.Rendering.Intro;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests the intro's cloth flag simulation (lippu.cpp alustaLippu and calculate).
/// </summary>
[TestClass]
public sealed class ClothFlagTests
{
    private const float Tolerance = 1e-5f;
    // The cloth is pushed back to about 0.29 from the axis, not exactly (the visible pole is 0.17 thick).
    private const float PoleClearance = 0.25f;
    // Vertices closer to the axis than this are stuck to the pole (lippu.cpp pushes them out to 0.29 and stops them).
    private const float OnPoleRadius = 0.3f;
    // The cubic ties are slack, so the cloth's weight lets the corners sag about a unit from their points.
    private const float AnchorReach = 1.5f;
    private static readonly Vector3 TopAnchor = new(0, ClothFlag.PoleHeight, 0);
    private static readonly Vector3 BottomAnchor = new(0, 9.2f, 0);

    [TestMethod]
    public void New_LiesFlatBesideThePoleTop()
    {
        var flag = new ClothFlag();

        Assert.AreEqual(ClothFlag.Columns * ClothFlag.Rows, flag.Positions.Count);
        Assert.AreEqual((ClothFlag.Columns - 1) * (ClothFlag.Rows - 1) * 2 * 3, flag.TriangleCorners.Count);
        AssertNear(new Vector3(0.2f, 14.7f, 0), flag.Positions[ClothFlag.TopLeft]);
        Assert.AreEqual(Vector2.Zero, flag.TexCoords[ClothFlag.TopLeft]);
        Assert.AreEqual(Vector2.One, flag.TexCoords[flag.TexCoords.Count - 1]);
        Assert.IsTrue(flag.Positions.All(p => p.Z == 0));
        Assert.IsTrue(flag.Normals.All(n => MathF.Abs(MathF.Abs(n.Z) - 1) < Tolerance));
    }

    [TestMethod]
    public void Step_OverALongIntro_StaysFinite()
    {
        var flag = new ClothFlag();

        for (var frame = 1; frame <= 15000; frame++)
        {
            flag.Step(frame);
        }

        Assert.IsTrue(flag.Positions.All(IsFinite));
        Assert.IsTrue(flag.Normals.All(IsFinite));
    }

    [TestMethod]
    public void Step_AfterCatchingOnThePole_ComesLooseAgain()
    {
        // The cloth first catches on the pole about 26 seconds in. Evaluated in float precision it wound up round the
        // pole for good there; with the original's double-precision arithmetic it is free again within two minutes.
        const int CaughtFrame = 60 * 150;
        const int DeadlineFrame = 120 * 150;
        var flag = new ClothFlag();

        var loose = false;
        for (var frame = 1; frame <= DeadlineFrame && !loose; frame++)
        {
            flag.Step(frame);
            loose = frame > CaughtFrame && flag.Positions.All(p => (p.X * p.X) + (p.Z * p.Z) > OnPoleRadius * OnPoleRadius);
        }

        Assert.IsTrue(loose);
    }

    [TestMethod]
    public void Step_KeepsTheTiedCornersAtThePoleAndTheClothOutsideIt()
    {
        var flag = new ClothFlag();

        var closest = float.MaxValue;
        for (var frame = 1; frame <= 3000; frame++)
        {
            flag.Step(frame);
            closest = MathF.Min(closest, flag.Positions.Min(p => MathF.Sqrt((p.X * p.X) + (p.Z * p.Z))));
        }

        Assert.IsTrue(closest >= PoleClearance);

        Assert.IsTrue(Vector3.Distance(flag.Positions[ClothFlag.TopLeft], TopAnchor) < AnchorReach);
        Assert.IsTrue(Vector3.Distance(flag.Positions[ClothFlag.BottomLeft], BottomAnchor) < AnchorReach);
    }

    [TestMethod]
    public void Step_BlowsTheClothDownwind()
    {
        var wind = new Vector3(0, 0, 0.0002f);
        var forward = new ClothFlag();
        var backward = new ClothFlag();
        var calm = new ClothFlag();

        for (var i = 0; i < 300; i++)
        {
            forward.Step(wind);
            backward.Step(-wind);
            calm.Step(Vector3.Zero);
        }

        Assert.IsTrue(forward.Positions.Average(p => p.Z) > 0.1f);
        Assert.IsTrue(backward.Positions.Average(p => p.Z) < -0.1f);
        Assert.IsTrue(calm.Positions.All(p => p.Z == 0));
    }

    [TestMethod]
    public void WindAt_SwingsAndReversesOverTheIntro()
    {
        var early = ClothFlag.WindAt(0);
        var late = ClothFlag.WindAt(3000);

        Assert.AreEqual(0f, early.Y);
        Assert.IsTrue(early.Z < 0, "The first thousand frames blow the other way.");
        Assert.IsTrue(late.Z > 0);
    }

    private static bool IsFinite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);

    private static void AssertNear(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.X, actual.X, Tolerance);
        Assert.AreEqual(expected.Y, actual.Y, Tolerance);
        Assert.AreEqual(expected.Z, actual.Z, Tolerance);
    }
}
