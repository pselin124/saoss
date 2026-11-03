using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Visual-only state of a match that advances with the simulation: camera, stars, particles and the
/// global spin counters used by the planet and asteroids [xrot, yrot, zrot].
/// </summary>
public sealed class SceneState
{
    private const float SpinX = 0.6f;
    private const float SpinY = 0.9f;
    private const float SpinZ = 0.12f;

    /// <summary>Creates the visual state for a match.</summary>
    public SceneState(int detail, int seed)
    {
        Detail = detail;
        Random = new GameRandom(seed);
        Particles = new ParticleSystem(detail);
        Stars = new Starfield(Random);
    }

    /// <summary>Detail level 0-2 [sets->detail].</summary>
    public int Detail { get; }

    /// <summary>Random source for visual effects only; never used by the simulation.</summary>
    public GameRandom Random { get; }

    /// <summary>The chase camera.</summary>
    public GameCamera Camera { get; } = new();

    /// <summary>Sparks and debris.</summary>
    public ParticleSystem Particles { get; }

    /// <summary>Background stars.</summary>
    public Starfield Stars { get; }

    /// <summary>Spin angles in degrees.</summary>
    public float RotationX { get; private set; }

    /// <inheritdoc cref="RotationX"/>
    public float RotationY { get; private set; }

    /// <inheritdoc cref="RotationX"/>
    public float RotationZ { get; private set; }

    /// <summary>Advances the visual state by one simulation tick.</summary>
    public void Update(World world)
    {
        Camera.Update(world);
        Stars.Update(world.Tick);
        Particles.Update();
        RotationX += SpinX;
        RotationY += SpinY;
        RotationZ += SpinZ;
    }
}
