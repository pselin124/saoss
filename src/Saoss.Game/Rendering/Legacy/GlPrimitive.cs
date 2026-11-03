namespace Saoss.Game.Rendering.Legacy;

/// <summary>Primitive types for <see cref="LegacyGl.Begin"/> (glBegin modes).</summary>
public enum GlPrimitive
{
    /// <summary>GL_POINTS (drawn as nothing).</summary>
    Points,

    /// <summary>GL_LINES.</summary>
    Lines,

    /// <summary>GL_LINE_STRIP.</summary>
    LineStrip,

    /// <summary>GL_LINE_LOOP.</summary>
    LineLoop,

    /// <summary>GL_TRIANGLES.</summary>
    Triangles,

    /// <summary>GL_TRIANGLE_STRIP.</summary>
    TriangleStrip,

    /// <summary>GL_TRIANGLE_FAN.</summary>
    TriangleFan,

    /// <summary>GL_QUADS.</summary>
    Quads,

    /// <summary>GL_QUAD_STRIP.</summary>
    QuadStrip,

    /// <summary>GL_POLYGON (convex, drawn as a fan).</summary>
    Polygon,
}
