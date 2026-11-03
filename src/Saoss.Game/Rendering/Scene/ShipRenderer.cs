using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Ships;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws ships, their projectiles and effects, and the per-player HUD.
/// Ships use the ported 3D models; projectiles are still placeholder glowing dots.
/// </summary>
public sealed class ShipRenderer(LegacyGl gl, SceneTextures textures, SharedModels models, int detail)
{
    private readonly ShipModelRenderer _models = new(gl, textures, new ShipModels(gl, models, detail));
    /// <summary>Updates per-ship visual state and spawns particles for this tick's events.</summary>
    public void Update(World world, SceneState state)
    {
        foreach (var gameEvent in world.Events)
        {
            if (gameEvent is ShipExploded exploded)
            {
                state.Particles.Spawn(200 + (detail * 100), p =>
                {
                    p.X = exploded.X;
                    p.Y = exploded.Y;
                    p.VelocityX = state.Random.Range(-0.75f, 0.75f) * 0.8f;
                    p.VelocityY = state.Random.Range(-0.75f, 0.75f) * 0.8f;
                    p.VelocityZ = state.Random.Range(-0.75f, 0.75f) * 0.8f;
                    p.MaxLife = 400;
                    p.Life = (int)state.Random.Next(200) + 200;
                    p.Size = state.Random.Next(2f) + 0.6f;
                    p.Red = state.Random.Next(0.5f) + 0.5f;
                    p.Green = state.Random.Next(1f);
                    p.Blue = state.Random.Next(1f);
                });
            }
        }
    }

    /// <summary>Draws a living ship with its projectiles.</summary>
    public void DrawShip(Ship ship, World world, SceneState state)
    {
        _models.Draw(ship, state);

        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        gl.BindTexture(textures.Flare);
        DrawDots(ship.PrimaryProjectiles, ship.Definition.Primary.MaxProjectiles, ship.Definition.Primary.Radius, state);
        DrawDots(ship.SpecialProjectiles, ship.Definition.Special.MaxProjectiles, ship.Definition.Special.Radius, state);
    }

    /// <summary>Draws the respawn animation of a ship that is about to reappear.</summary>
    public void DrawSpawnSparks(Ship ship, SceneState state)
    {
    }

    /// <summary>Draws the ship's corner of the HUD.</summary>
    public void DrawHud(Ship ship, World world, SceneState state)
    {
    }

    private void DrawDots(Projectile[] pool, int count, float radius, SceneState state)
    {
        gl.Color(1, 1, 0.6f, 1);
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (!projectile.IsActive)
            {
                continue;
            }

            gl.PushMatrix();
            gl.Translate(projectile.X, projectile.Y, GameConstants.ShipPlaneZ);
            SceneDrawing.FaceCamera(gl, state.Camera);
            SceneDrawing.Quad(gl, MathF.Max(radius, 0.4f));
            gl.PopMatrix();
        }
    }
}
