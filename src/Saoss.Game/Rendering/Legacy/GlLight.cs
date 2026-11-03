using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// One OpenGL fixed-function light. Positions are stored in eye space, transformed when set, like glLightfv.
/// </summary>
public sealed class GlLight
{
    /// <summary>Whether the light contributes (GL_LIGHTi enabled).</summary>
    public bool Enabled { get; set; }

    /// <summary>Eye-space position; W = 0 means a directional light.</summary>
    public Vector4 Position { get; set; } = new(0, 0, 1, 0);

    /// <summary>Ambient intensity.</summary>
    public Vector4 Ambient { get; set; } = new(0, 0, 0, 1);

    /// <summary>Diffuse intensity.</summary>
    public Vector4 Diffuse { get; set; }

    /// <summary>Specular intensity.</summary>
    public Vector4 Specular { get; set; }

    /// <summary>Distance attenuation terms.</summary>
    public float ConstantAttenuation { get; set; } = 1f;

    /// <inheritdoc cref="ConstantAttenuation"/>
    public float LinearAttenuation { get; set; }

    /// <inheritdoc cref="ConstantAttenuation"/>
    public float QuadraticAttenuation { get; set; }
}
