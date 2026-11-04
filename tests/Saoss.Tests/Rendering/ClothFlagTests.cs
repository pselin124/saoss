using Microsoft.Xna.Framework;
using Saoss.Game.Rendering.Intro;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests the intro's cloth flag simulation (lippu.cpp alustaLippu and calculate, with stretch, rope and pole constraints).
/// </summary>
[TestClass]
public sealed class ClothFlagTests
{
    private const float Tolerance = 1e-5f;
    // Below the knob the cloth is kept 0.23 from the pole's axis (the visible pole is 0.17 thick).
    private const float PoleClearance = 0.22f;
    // Vertices closer to the axis than this lie against the pole.
    private const float OnPoleRadius = 0.25f;
    // The ropes let the top corner hang up to 0.35 from its tie 0.3 below the knob and the bottom one a unit from its
    // point; pushing the cloth off the pole can add a little.
    private const float TopReach = 0.7f;
    private const float BottomReach = 1.05f;
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
    public void Step_NeverStaysWoundRoundThePole()
    {
        // The original stopped every vertex that touched the pole, so a wound flag could stay frozen there for good.
        const int Seconds = 120;
        const int FramesPerSecond = 150;
        const int MaxWoundSeconds = 5;
        var flag = new ClothFlag();

        var wound = 0;
        var longestWound = 0;
        for (var frame = 1; frame <= Seconds * FramesPerSecond; frame++)
        {
            flag.Step(frame);
            if (frame % FramesPerSecond == 0)
            {
                var onPole = flag.Positions.Count(p => (p.X * p.X) + (p.Z * p.Z) < OnPoleRadius * OnPoleRadius);
                wound = onPole > flag.Positions.Count / 3 ? wound + 1 : 0;
                longestWound = Math.Max(longestWound, wound);
            }
        }

        Assert.IsTrue(longestWound < MaxWoundSeconds, $"Wound round the pole for {longestWound} s.");
    }

    [TestMethod]
    public void Step_InAStorm_StaysTiedAndHardlyStretches()
    {
        // Five times the intro's strongest wind; the original's soft springs blew apart (NaN) under it. The flag is 6
        // wide, and in the intro's own wind the original stretched it to reach 8.4 from the pole.
        var storm = new Vector3(0.001f, 0, 0.0003f);
        const float MaxReach = 8f;
        var flag = new ClothFlag();

        var reach = 0f;
        for (var i = 0; i < 3000; i++)
        {
            flag.Step(storm);
            reach = MathF.Max(reach, flag.Positions.Max(p => MathF.Sqrt((p.X * p.X) + (p.Z * p.Z))));
        }

        Assert.IsTrue(flag.Positions.All(IsFinite));
        Assert.IsTrue(reach < MaxReach, $"The cloth reached {reach} from the pole.");
        Assert.IsTrue(Vector3.Distance(flag.Positions[ClothFlag.TopLeft], TopAnchor) < TopReach);
        Assert.IsTrue(Vector3.Distance(flag.Positions[ClothFlag.BottomLeft], BottomAnchor) < BottomReach);
    }

    [TestMethod]
    public void Step_KeepsTheTiedCornersAtThePoleAndTheClothOutsideIt()
    {
        var flag = new ClothFlag();

        var closest = float.MaxValue;
        for (var frame = 1; frame <= 3000; frame++)
        {
            flag.Step(frame);
            closest = MathF.Min(closest, flag.Positions.Where(p => p.Y < ClothFlag.PoleHeight).Min(p => MathF.Sqrt((p.X * p.X) + (p.Z * p.Z))));
        }

        Assert.IsTrue(closest >= PoleClearance);

        Assert.IsTrue(Vector3.Distance(flag.Positions[ClothFlag.TopLeft], TopAnchor) < TopReach);
        Assert.IsTrue(Vector3.Distance(flag.Positions[ClothFlag.BottomLeft], BottomAnchor) < BottomReach);
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
