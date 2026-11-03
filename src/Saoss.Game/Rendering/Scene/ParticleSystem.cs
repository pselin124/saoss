using Saoss.Core;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Pool of sparks and debris shared by all effects [sirpale array]. Spawning fills free slots from the
/// start of the pool and silently gives up when it is full, like the original.
/// </summary>
public sealed class ParticleSystem
{
    private readonly Particle[] _particles;

    /// <summary>Creates a pool sized like the original: 1000 + 500 per detail level.</summary>
    public ParticleSystem(int detail)
    {
        _particles = [.. Enumerable.Range(0, 1000 + (detail * 500)).Select(_ => new Particle())];
    }

    /// <summary>
    /// Fills up to <paramref name="count"/> free slots, calling <paramref name="initialize"/> for each one.
    /// </summary>
    public void Spawn(int count, Action<Particle> initialize)
    {
        for (var i = 0; i < _particles.Length && count > 0; i++)
        {
            var particle = _particles[i];
            if (particle.IsActive)
            {
                continue;
            }

            count--;
            particle.Z = 0;
            particle.VelocityZ = 0;
            particle.IsSquare = false;
            initialize(particle);
        }
    }

    /// <summary>Moves all particles one tick.</summary>
    public void Update()
    {
        foreach (var particle in _particles)
        {
            if (!particle.IsActive)
            {
                continue;
            }

            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Z += particle.VelocityZ;
            particle.Life--;
        }
    }

    /// <summary>Draws all particles as additive billboards facing the camera.</summary>
    public void Draw(LegacyGl gl, SceneTextures textures, GameCamera camera)
    {
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Lighting);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        foreach (var particle in _particles)
        {
            if (!particle.IsActive)
            {
                continue;
            }

            gl.BindTexture(particle.IsSquare ? textures.WhiteSquare : textures.Flare);
            var fade = particle.Life / particle.MaxLife;
            gl.Color(particle.Red * fade, particle.Green * fade, particle.Blue * fade, 1f);
            gl.PushMatrix();
            gl.Translate(particle.X, particle.Y, GameConstants.ShipPlaneZ + particle.Z);
            SceneDrawing.FaceCamera(gl, camera);
            SceneDrawing.Quad(gl, particle.Size);
            gl.PopMatrix();
        }
    }
}
