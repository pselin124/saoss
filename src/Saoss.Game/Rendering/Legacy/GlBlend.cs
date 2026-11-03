namespace Saoss.Game.Rendering.Legacy;

/// <summary>Blend factors for <see cref="LegacyGl.BlendFunc"/> (glBlendFunc values).</summary>
public enum GlBlend
{
    /// <summary>GL_ZERO.</summary>
    Zero,

    /// <summary>GL_ONE.</summary>
    One,

    /// <summary>GL_SRC_COLOR.</summary>
    SrcColor,

    /// <summary>GL_ONE_MINUS_SRC_COLOR.</summary>
    OneMinusSrcColor,

    /// <summary>GL_SRC_ALPHA.</summary>
    SrcAlpha,

    /// <summary>GL_ONE_MINUS_SRC_ALPHA.</summary>
    OneMinusSrcAlpha,

    /// <summary>GL_DST_COLOR.</summary>
    DstColor,

    /// <summary>GL_ONE_MINUS_DST_COLOR.</summary>
    OneMinusDstColor,

    /// <summary>GL_DST_ALPHA.</summary>
    DstAlpha,

    /// <summary>GL_ONE_MINUS_DST_ALPHA.</summary>
    OneMinusDstAlpha,
}
