namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// The arguments of one GLU quadric call, the key of its cached tessellation. Unused arguments are zero.
/// </summary>
/// <param name="Kind">Sphere, cylinder or partial disk.</param>
/// <param name="Radius1">Sphere radius, cylinder base radius or disk inner radius.</param>
/// <param name="Radius2">Cylinder top radius or disk outer radius.</param>
/// <param name="Height">Cylinder height.</param>
/// <param name="Slices">Subdivisions around the axis.</param>
/// <param name="Stacks">Sphere and cylinder stacks or disk loops.</param>
/// <param name="StartDegrees">Disk start angle.</param>
/// <param name="SweepDegrees">Disk sweep angle.</param>
/// <param name="Textured">Whether texture coordinates are generated.</param>
internal readonly record struct GlQuadric(
    GlQuadricKind Kind,
    float Radius1,
    float Radius2,
    float Height,
    int Slices,
    int Stacks,
    float StartDegrees,
    float SweepDegrees,
    bool Textured);
