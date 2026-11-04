namespace Saoss.Game.Rendering.Legacy;

/// <summary>Stencil buffer actions for <see cref="LegacyGl.StencilOp"/> (glStencilOp values).</summary>
public enum GlStencilOp
{
    /// <summary>GL_KEEP.</summary>
    Keep,

    /// <summary>GL_ZERO.</summary>
    Zero,

    /// <summary>GL_REPLACE: writes the reference value.</summary>
    Replace,

    /// <summary>GL_INCR (saturating).</summary>
    Increment,

    /// <summary>GL_DECR (saturating).</summary>
    Decrement,

    /// <summary>GL_INVERT.</summary>
    Invert,
}
