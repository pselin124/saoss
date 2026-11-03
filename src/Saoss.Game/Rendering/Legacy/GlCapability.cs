namespace Saoss.Game.Rendering.Legacy;

/// <summary>Capabilities for <see cref="LegacyGl.Enable"/> and <see cref="LegacyGl.Disable"/> (glEnable values).</summary>
public enum GlCapability
{
    /// <summary>GL_BLEND.</summary>
    Blend,

    /// <summary>GL_DEPTH_TEST.</summary>
    DepthTest,

    /// <summary>GL_LIGHTING.</summary>
    Lighting,

    /// <summary>GL_TEXTURE_2D.</summary>
    Texture2D,

    /// <summary>GL_CULL_FACE.</summary>
    CullFace,

    /// <summary>GL_COLOR_MATERIAL.</summary>
    ColorMaterial,

    /// <summary>GL_LIGHT0.</summary>
    Light0,

    /// <summary>GL_LIGHT1.</summary>
    Light1,

    /// <summary>GL_LIGHT2.</summary>
    Light2,

    /// <summary>GL_NORMALIZE (normals are always normalized here).</summary>
    Normalize,

    /// <summary>GL_LINE_SMOOTH (ignored).</summary>
    LineSmooth,
}
