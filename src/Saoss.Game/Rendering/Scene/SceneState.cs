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
    public SceneState(int detail, CameraMode cameraMode, int seed)
    {
        Detail = detail;
        Random = new GameRandom(seed);
        Particles = new ParticleSystem(detail);
        Stars = new Starfield(Random);
        Camera = new GameCamera(cameraMode);
    }

    /// <summary>Detail level 0-3 [sets->detail].</summary>
    public int Detail { get; }

    /// <summary>Random source for visual effects only; never used by the simulation.</summary>
    public GameRandom Random { get; }

    /// <summary>The match camera.</summary>
    public GameCamera Camera { get; }

    /// <summary>Sparks and debris.</summary>
    public ParticleSystem Particles { get; }

    /// <summary>Background stars.</summary>
    public Starfield Stars { get; }

    /// <summary>
    /// Nickname of every ship in a network game, drawn around the ship and in the HUD; null in a local game.
    /// </summary>
    public IReadOnlyList<string>? ShipNames { get; init; }

    /// <summary>Spin angles in degrees.</summary>
    public float RotationX { get; private set; }

    /// <inheritdoc cref="RotationX"/>
    public float RotationY { get; private set; }

    /// <inheritdoc cref="RotationX"/>
    public float RotationZ { get; private set; }

    /// <summary>
    /// Advances the visual state by one simulation tick; the camera frames the match for a viewport of
    /// <paramref name="aspectRatio"/> (width / height).
    /// </summary>
    public void Update(World world, float aspectRatio)
    {
        Camera.Update(world, aspectRatio);
        Stars.Update(world.Tick);
        Particles.Update();
        RotationX += SpinX;
        RotationY += SpinY;
        RotationZ += SpinZ;
    }
}
