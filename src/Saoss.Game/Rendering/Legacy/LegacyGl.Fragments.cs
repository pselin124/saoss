using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Per-fragment state beyond blending: stencil test, polygon offset and the texture environment, and the MonoGame
/// state objects and textures created for them.
/// </summary>
public sealed partial class LegacyGl
{
    private readonly Dictionary<(bool DepthTest, GlStencil Stencil), DepthStencilState> _stencilStates = [];
    private readonly Dictionary<(CullMode Cull, GlPolygonOffset Offset), RasterizerState> _offsetStates = [];
    private readonly Dictionary<Texture2D, Texture2D> _invertedTextures = [];

    private bool _stencilTest;
    private GlStencil _stencil = GlStencil.Default;
    private bool _polygonOffsetFill;
    private GlPolygonOffset _polygonOffset;
    private GlTexEnv _texEnv;

    /// <summary>glStencilFunc.</summary>
    public void StencilFunc(GlCompare function, int reference, int mask)
    {
        if (TryRecord((function, reference, mask), static (g, a) => g.StencilFunc(a.function, a.reference, a.mask)))
        {
            return;
        }

        _stencil = _stencil with { Function = function, Reference = reference, Mask = mask };
    }

    /// <summary>glStencilOp.</summary>
    public void StencilOp(GlStencilOp fail, GlStencilOp depthFail, GlStencilOp pass)
    {
        if (TryRecord((fail, depthFail, pass), static (g, a) => g.StencilOp(a.fail, a.depthFail, a.pass)))
        {
            return;
        }

        _stencil = _stencil with { Fail = fail, DepthFail = depthFail, Pass = pass };
    }

    /// <summary>glPolygonOffset; applies to filled polygons while <see cref="GlCapability.PolygonOffsetFill"/> is enabled.</summary>
    public void PolygonOffset(float factor, float units)
    {
        if (TryRecord((factor, units), static (g, a) => g.PolygonOffset(a.factor, a.units)))
        {
            return;
        }

        _polygonOffset = new GlPolygonOffset(factor, units);
    }

    /// <summary>glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, mode).</summary>
    public void TexEnv(GlTexEnv mode)
    {
        if (TryRecord(mode, static (g, m) => g.TexEnv(m)))
        {
            return;
        }

        _texEnv = mode;
    }

    // GL_BLEND is drawn as GL_MODULATE with a color-inverted copy of the texture.
    private Texture2D? ResolveTexture(Texture2D? texture)
    {
        if (texture == null || _texEnv == GlTexEnv.Modulate)
        {
            return texture;
        }

        if (!_invertedTextures.TryGetValue(texture, out var inverted))
        {
            var pixels = new XnaColor[texture.Width * texture.Height];
            texture.GetData(pixels);
            GlDeviceStates.InvertRgb(pixels);
            inverted = new Texture2D(_device, texture.Width, texture.Height);
            inverted.SetData(pixels);
            _invertedTextures[texture] = inverted;
        }

        return inverted;
    }

    private DepthStencilState GetDepthStencilState(bool depthTest, GlStencil? stencil)
    {
        if (stencil is not { } settings)
        {
            return depthTest ? _depthTestState : DepthStencilState.None;
        }

        if (!_stencilStates.TryGetValue((depthTest, settings), out var state))
        {
            state = GlDeviceStates.CreateDepthStencil(depthTest, settings);
            _stencilStates[(depthTest, settings)] = state;
        }

        return state;
    }

    private RasterizerState GetRasterizerState(CullMode cull, GlPolygonOffset? offset)
    {
        if (offset is not { } settings)
        {
            return cull switch
            {
                CullMode.CullClockwiseFace => RasterizerState.CullClockwise,
                CullMode.CullCounterClockwiseFace => RasterizerState.CullCounterClockwise,
                _ => RasterizerState.CullNone,
            };
        }

        if (!_offsetStates.TryGetValue((cull, settings), out var state))
        {
            state = GlDeviceStates.CreateRasterizer(cull, settings);
            _offsetStates[(cull, settings)] = state;
        }

        return state;
    }

    private void DisposeFragmentStates()
    {
        foreach (var state in _stencilStates.Values)
        {
            state.Dispose();
        }

        foreach (var state in _offsetStates.Values)
        {
            state.Dispose();
        }

        foreach (var texture in _invertedTextures.Values)
        {
            texture.Dispose();
        }
    }
}
