namespace Saoss.Game.Rendering.Legacy;

/// <summary>Which GLU quadric a <see cref="GlQuadric"/> describes.</summary>
internal enum GlQuadricKind
{
    /// <summary>gluSphere.</summary>
    Sphere,

    /// <summary>gluCylinder.</summary>
    Cylinder,

    /// <summary>gluPartialDisk (and gluDisk).</summary>
    PartialDisk,
}
