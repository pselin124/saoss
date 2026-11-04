namespace Saoss.Game.Rendering.Legacy;

/// <summary>Comparison functions for <see cref="LegacyGl.StencilFunc"/> (GL_NEVER ... GL_ALWAYS).</summary>
public enum GlCompare
{
    /// <summary>GL_NEVER.</summary>
    Never,

    /// <summary>GL_LESS.</summary>
    Less,

    /// <summary>GL_EQUAL.</summary>
    Equal,

    /// <summary>GL_LEQUAL.</summary>
    LessEqual,

    /// <summary>GL_GREATER.</summary>
    Greater,

    /// <summary>GL_NOTEQUAL.</summary>
    NotEqual,

    /// <summary>GL_GEQUAL.</summary>
    GreaterEqual,

    /// <summary>GL_ALWAYS.</summary>
    Always,
}
