using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests the GL state added for the intro: stencil, polygon offset, GL_BLEND texturing and two-sided facing.
/// </summary>
[TestClass]
public sealed class LegacyGlStateTests
{
    private static readonly GlStencil ShadowStencil = new(GlCompare.Greater, 1, 1, GlStencilOp.Keep, GlStencilOp.Keep, GlStencilOp.Replace);

    [TestMethod]
    public void BatchKey_DiffersWhenOnlyTheStencilDiffers()
    {
        var plain = Key(stencil: null, offset: null);
        var stenciled = Key(ShadowStencil, offset: null);
        var otherReference = Key(ShadowStencil with { Reference = 2 }, offset: null);

        Assert.AreNotEqual(plain, stenciled);
        Assert.AreNotEqual(stenciled, otherReference);
        Assert.AreEqual(stenciled, Key(ShadowStencil, offset: null));
    }

    [TestMethod]
    public void BatchKey_DiffersWhenOnlyThePolygonOffsetDiffers()
    {
        Assert.AreNotEqual(Key(stencil: null, offset: null), Key(stencil: null, new GlPolygonOffset(0, -1)));
        Assert.AreNotEqual(Key(stencil: null, new GlPolygonOffset(0, -1)), Key(stencil: null, new GlPolygonOffset(0, -2)));
    }

    [TestMethod]
    public void CreateDepthStencil_MapsTheShadowStencil()
    {
        using var state = GlDeviceStates.CreateDepthStencil(depthTest: true, ShadowStencil);

        Assert.IsTrue(state.StencilEnable);
        Assert.IsTrue(state.DepthBufferEnable);
        Assert.IsTrue(state.DepthBufferWriteEnable);
        Assert.AreEqual(CompareFunction.LessEqual, state.DepthBufferFunction);
        Assert.AreEqual(CompareFunction.Greater, state.StencilFunction);
        Assert.AreEqual(1, state.ReferenceStencil);
        Assert.AreEqual(1, state.StencilMask);
        Assert.AreEqual(StencilOperation.Keep, state.StencilFail);
        Assert.AreEqual(StencilOperation.Keep, state.StencilDepthBufferFail);
        Assert.AreEqual(StencilOperation.Replace, state.StencilPass);
    }

    [TestMethod]
    public void CreateRasterizer_PullsTowardTheViewerByOneDepthUnit()
    {
        using var state = GlDeviceStates.CreateRasterizer(CullMode.None, new GlPolygonOffset(0.5f, -1));

        Assert.AreEqual(CullMode.None, state.CullMode);
        Assert.AreEqual(0.5f, state.SlopeScaleDepthBias);
        Assert.AreEqual(-1f / (1 << 23), state.DepthBias);
    }

    [TestMethod]
    public void InvertRgb_KeepsAlpha()
    {
        Color[] pixels = [new Color(255, 0, 64, 10), new Color(0, 0, 0, 255)];

        GlDeviceStates.InvertRgb(pixels);

        Assert.AreEqual(new Color(0, 255, 191, 10), pixels[0]);
        Assert.AreEqual(new Color(255, 255, 255, 255), pixels[1]);
    }

    [TestMethod]
    public void IsFrontFacing_FollowsWindingAndFrontFace()
    {
        var a = new Vector4(0, 0, 0, 1);
        var b = new Vector4(1, 0, 0, 1);
        var c = new Vector4(0, 1, 0, 1);

        Assert.IsTrue(GlFacing.IsFrontFacing(a, b, c, frontFaceClockwise: false));
        Assert.IsFalse(GlFacing.IsFrontFacing(a, c, b, frontFaceClockwise: false));
        Assert.IsFalse(GlFacing.IsFrontFacing(a, b, c, frontFaceClockwise: true));
    }

    [TestMethod]
    public void IsFrontFacing_IsNotFlippedByACornerBehindTheEye()
    {
        // A ground triangle seen from above whose near corner is behind the camera (negative w).
        var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, 0.1f, 100);
        var view = Matrix.CreateLookAt(new Vector3(0, 5, 0), new Vector3(0, 0, -10), Vector3.Up);
        Vector4 Clip(float x, float z) => Vector4.Transform(new Vector4(x, 0, z, 1), view * projection);

        // Counter-clockwise seen from above (+Y), so it faces the camera.
        var far = Clip(0, -30);
        var left = Clip(-10, 20);
        var right = Clip(10, 20);

        Assert.IsTrue(left.W < 0 && right.W < 0 && far.W > 0);
        Assert.IsTrue(GlFacing.IsFrontFacing(far, left, right, frontFaceClockwise: false));
    }

    private static GlBatchKey Key(GlStencil? stencil, GlPolygonOffset? offset) =>
        new(null, (GlBlend.Zero, GlBlend.OneMinusSrcAlpha), DepthTest: true, CullMode.None, stencil, offset, Matrix.Identity, IsLine: false);
}
