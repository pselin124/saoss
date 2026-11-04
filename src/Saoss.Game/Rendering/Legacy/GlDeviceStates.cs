using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Translates emulated GL state that MonoGame has no direct equivalent for into MonoGame state objects and textures.
/// </summary>
public static class GlDeviceStates
{
    // MonoGame DesktopGL multiplies DepthBias by 1 << 23 for 24-bit depth buffers before calling glPolygonOffset.
    private const float DepthBiasPerGlUnit = 1f / (1 << 23);

    /// <summary>A depth-stencil state with the stencil test enabled; depth is tested and written when <paramref name="depthTest"/> is set.</summary>
    public static DepthStencilState CreateDepthStencil(bool depthTest, GlStencil stencil) => new()
    {
        DepthBufferEnable = depthTest,
        DepthBufferWriteEnable = depthTest,
        DepthBufferFunction = CompareFunction.LessEqual,
        StencilEnable = true,
        StencilFunction = ToXna(stencil.Function),
        ReferenceStencil = stencil.Reference,
        StencilMask = stencil.Mask,
        StencilFail = ToXna(stencil.Fail),
        StencilDepthBufferFail = ToXna(stencil.DepthFail),
        StencilPass = ToXna(stencil.Pass),
    };

    /// <summary>A rasterizer state that culls <paramref name="cull"/> and offsets depth like glPolygonOffset.</summary>
    public static RasterizerState CreateRasterizer(CullMode cull, GlPolygonOffset offset) => new()
    {
        CullMode = cull,
        DepthBias = offset.Units * DepthBiasPerGlUnit,
        SlopeScaleDepthBias = offset.Factor,
    };

    /// <summary>
    /// Replaces every color with (1 - color), keeping alpha, so GL_MODULATE with the result equals GL_BLEND with a black
    /// environment color: C = Cf * (1 - Ct), A = Af * At.
    /// </summary>
    public static void InvertRgb(XnaColor[] pixels)
    {
        for (var i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            pixels[i] = new XnaColor((byte)(255 - p.R), (byte)(255 - p.G), (byte)(255 - p.B), p.A);
        }
    }

    private static CompareFunction ToXna(GlCompare function) => function switch
    {
        GlCompare.Never => CompareFunction.Never,
        GlCompare.Less => CompareFunction.Less,
        GlCompare.Equal => CompareFunction.Equal,
        GlCompare.LessEqual => CompareFunction.LessEqual,
        GlCompare.Greater => CompareFunction.Greater,
        GlCompare.NotEqual => CompareFunction.NotEqual,
        GlCompare.GreaterEqual => CompareFunction.GreaterEqual,
        _ => CompareFunction.Always,
    };

    private static StencilOperation ToXna(GlStencilOp operation) => operation switch
    {
        GlStencilOp.Zero => StencilOperation.Zero,
        GlStencilOp.Replace => StencilOperation.Replace,
        GlStencilOp.Increment => StencilOperation.IncrementSaturation,
        GlStencilOp.Decrement => StencilOperation.DecrementSaturation,
        GlStencilOp.Invert => StencilOperation.Invert,
        _ => StencilOperation.Keep,
    };
}
