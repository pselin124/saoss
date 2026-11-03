using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships;

/// <summary>
/// The two GL state blocks the original repeated at the start and end of nearly every ship list and branch.
/// </summary>
public static class ShipGlState
{
    /// <summary>Lit, untextured, opaque and depth tested: the state for solid ship geometry.</summary>
    public static void Solid(LegacyGl gl)
    {
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);
    }

    /// <summary>Textured, blended, unlit and without depth test: the state the rest of the scene expects.</summary>
    public static void Overlay(LegacyGl gl)
    {
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
    }
}
