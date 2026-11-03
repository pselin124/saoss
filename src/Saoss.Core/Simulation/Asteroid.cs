namespace Saoss.Core.Simulation;

/// <summary>
/// A drifting rock that is pulled toward the planet, bounces off ships and other asteroids,
/// and breaks when shot or when it hits the planet.
/// </summary>
public sealed class Asteroid
{
    /// <summary>Position in world units.</summary>
    public float X { get; set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; set; }

    /// <summary>Velocity per tick.</summary>
    public float VelocityX { get; set; }

    /// <inheritdoc cref="VelocityX"/>
    public float VelocityY { get; set; }

    /// <summary>Size; also its mass factor and collision radius factor [koko].</summary>
    public float Size { get; set; }

    /// <summary>Rotation phase used when drawing [alku].</summary>
    public float SpinPhase { get; set; }

    /// <summary>Rotation axis used when drawing [r, g, b].</summary>
    public float SpinAxisX { get; set; }

    /// <inheritdoc cref="SpinAxisX"/>
    public float SpinAxisY { get; set; }

    /// <inheritdoc cref="SpinAxisX"/>
    public float SpinAxisZ { get; set; }

    /// <summary>Hit points.</summary>
    public float Energy { get; set; }

    /// <summary>Collision radius against an object of the given radius; the original's (a + b) / 4 * PI.</summary>
    public float CollisionDistance(float otherRadius) => (Size + otherRadius) / 4f * MathF.PI;
}
