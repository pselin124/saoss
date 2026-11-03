using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws what a living ship has out in the arena: primary and special projectiles, slime stuck on it and
/// weapon beams (Player::piirraPadit, piirraePadit, piirraLimat, piirraViivat).
/// </summary>
public sealed class ProjectileRenderer(LegacyGl gl, SceneTextures textures)
{
    private const float ShotDepth = 1f;
    private const float SlimeSize = 0.5f;
    private const float BeamAlpha = 0.9f;

    /// <summary>
    /// Draws the primary projectiles [piirraPadit]. Power Vacuum, Rollu and Wiiii have their own look;
    /// all others are camera-facing textured quads turned to the shot's heading.
    /// </summary>
    public void DrawPrimary(Ship ship, GameCamera camera)
    {
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);

        // The original first set (One, One) and immediately replaced it.
        gl.BlendFunc(ship.Type == ShipType.Tringu ? GlBlend.SrcAlpha : GlBlend.SrcColor, GlBlend.OneMinusSrcColor);
        switch (ship.Type)
        {
            case ShipType.PowerVacuum:
                DrawVacuumOrbs(ship);
                break;
            case ShipType.Rollu:
                DrawRolluShots(ship, camera);
                break;
            case ShipType.Wiiii:
                DrawWiiiiBubbles(ship);
                break;
            default:
                DrawPlainShots(ship, camera);
                break;
        }
    }

    /// <summary>
    /// Draws the special projectiles additively [piirraePadit]. Unlike primary shots they lie flat on the
    /// ship plane. Relies on the texture state left by <see cref="DrawPrimary"/>, like the original.
    /// </summary>
    public void DrawSpecial(Ship ship)
    {
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        var radius = ship.Definition.Special.Radius;
        if (ship.Type != ShipType.Elephant)
        {
            // Elephant binds nothing and reuses the primary shot texture (the flare).
            gl.BindTexture(ProjectileTextures.Special(ship.Type, textures));
        }

        foreach (var projectile in Active(ship.SpecialProjectiles, ship.Definition.Special.MaxProjectiles))
        {
            gl.PushMatrix();
            gl.Translate(projectile.X, projectile.Y, GameConstants.ShipPlaneZ);
            var size = radius;
            switch (ship.Type)
            {
                case ShipType.Elephant:
                    gl.Color(projectile.Red, projectile.Green, projectile.Blue, 1f);
                    break;
                case ShipType.StickInsect when projectile.Red == 1:
                    gl.Color(1, 0.7f, 0.7f, 1f);
                    size = radius * 2f;
                    break;
                case ShipType.StickInsect:
                    gl.Color(0.7f, 1, 0.7f, 1f);
                    break;
                default:
                    gl.Rotate(projectile.Angle - 90f, 0, 0, 1);
                    if (ship.Type == ShipType.PowerVacuum)
                    {
                        gl.Color(0.1f, 0.7f, 0.1f, 1f);
                    }
                    else
                    {
                        gl.Color(1, 1, 1, 1f);
                    }

                    break;
            }

            SceneDrawing.Quad(gl, size, ShotDepth);
            gl.PopMatrix();
        }
    }

    /// <summary>Draws the green slime blobs stuck on the ship [piirraLimat].</summary>
    public void DrawSlimes(Ship ship)
    {
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        foreach (var slime in ship.Slimes)
        {
            if (!slime.IsActive)
            {
                continue;
            }

            gl.PushMatrix();
            gl.Translate(ship.X, ship.Y, GameConstants.ShipPlaneZ);
            gl.Rotate(ship.Angle - 90f, 0, 0, 1);
            gl.Translate(slime.X, slime.Y, 0);
            gl.BindTexture(textures.Flare);
            gl.Color(0.1f, 0.7f, 0.1f, 1f);
            SceneDrawing.Quad(gl, SlimeSize, ShotDepth);
            gl.PopMatrix();
        }

        gl.BlendFunc(GlBlend.SrcColor, GlBlend.OneMinusSrcColor);
    }

    /// <summary>Draws the ship's weapon beams as lines [piirraViivat].</summary>
    public void DrawBeams(Ship ship)
    {
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.LineSmooth);

        // glLineWidth (1, 1.5 or 2 pixels depending on the weapon) is not emulated; lines are 1 pixel wide.
        foreach (var beam in ship.Beams)
        {
            if (!beam.IsActive)
            {
                continue;
            }

            gl.PushMatrix();
            gl.Translate(beam.X1, beam.Y1, GameConstants.ShipPlaneZ);
            gl.Color(beam.Red, beam.Green, beam.Blue, BeamAlpha);
            gl.Begin(GlPrimitive.Lines);
            gl.Vertex(0, 0, 0);
            gl.Vertex(beam.X2 - beam.X1, beam.Y2 - beam.Y1, 0);
            gl.End();
            gl.PopMatrix();
        }

        gl.Enable(GlCapability.Texture2D);
    }

    private static IEnumerable<(int Index, Projectile Projectile)> ActiveWithIndex(Projectile[] pool, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (pool[i].IsActive)
            {
                yield return (i, pool[i]);
            }
        }
    }

    private static IEnumerable<Projectile> Active(Projectile[] pool, int count) => ActiveWithIndex(pool, count).Select(p => p.Projectile);

    // Lit nested spheres spinning at different rates; they appear as the charge (energy) grows.
    private void DrawVacuumOrbs(Ship ship)
    {
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Lighting);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.One);
        foreach (var projectile in Active(ship.PrimaryProjectiles, ship.Definition.Primary.MaxProjectiles))
        {
            gl.PushMatrix();
            gl.Translate(projectile.X, projectile.Y, GameConstants.ShipPlaneZ);
            gl.Scale(2, 2, 2);
            var charge = (int)projectile.Energy;
            gl.Color(projectile.Red, projectile.Green, projectile.Blue, 1f - MathF.Exp(-charge * 0.002f));
            for (var j = 1; j <= 10; j += 2)
            {
                gl.Rotate(charge * (j + 5), 0, 10 - j, j);
                gl.Sphere(j * 0.1f, 10, 10);
            }

            gl.PopMatrix();
        }

        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Texture2D);
    }

    // The first slot is the big leech-catching shot drawn with the ship's detail texture at the special's size.
    private void DrawRolluShots(Ship ship, GameCamera camera)
    {
        var definition = ship.Definition;
        foreach (var (index, projectile) in ActiveWithIndex(ship.PrimaryProjectiles, definition.Primary.MaxProjectiles))
        {
            gl.PushMatrix();
            gl.Translate(projectile.X, projectile.Y, GameConstants.ShipPlaneZ);
            SceneDrawing.FaceCamera(gl, camera);
            gl.Color(1, 1, 1, 1);
            if (index == 0)
            {
                gl.BindTexture(textures.TrekDetail);
                SceneDrawing.Quad(gl, definition.Special.Radius, ShotDepth);
            }
            else
            {
                gl.BindTexture(ProjectileTextures.Primary(ship.Type, textures));
                SceneDrawing.Quad(gl, definition.Primary.Radius, ShotDepth);
            }

            gl.PopMatrix();
        }
    }

    // Translucent lit blue spheres; leaves lighting and depth testing on and texturing off, like the original.
    private void DrawWiiiiBubbles(Ship ship)
    {
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);
        gl.Color(0.2f, 0.2f, 0.8f, 0.5f);
        foreach (var projectile in Active(ship.PrimaryProjectiles, ship.Definition.Primary.MaxProjectiles))
        {
            gl.PushMatrix();
            gl.Translate(projectile.X, projectile.Y, GameConstants.ShipPlaneZ);
            gl.Sphere(1.3f, 10, 5);
            gl.PopMatrix();
        }
    }

    // Tilted toward the camera but keeping the shot's heading in the ship plane's direction.
    private void DrawPlainShots(Ship ship, GameCamera camera)
    {
        var radius = ship.Definition.Primary.Radius;
        gl.BindTexture(ProjectileTextures.Primary(ship.Type, textures));
        foreach (var projectile in Active(ship.PrimaryProjectiles, ship.Definition.Primary.MaxProjectiles))
        {
            gl.PushMatrix();
            gl.Translate(projectile.X, projectile.Y, GameConstants.ShipPlaneZ);
            SceneDrawing.FaceCamera(gl, camera);
            gl.Rotate(-camera.Alpha, 0, 0, 1);
            gl.Rotate(projectile.Angle - 90f, 0, 0, 1);
            switch (ship.Type)
            {
                case ShipType.Tongu:
                    gl.Color(0.9f, 0.9f, 0.9f, 2f);
                    break;
                case ShipType.StickInsect:
                    gl.Color(0.5f, 0.5f, 1f, 5f);
                    break;
                default:
                    gl.Color(1, 1, 1, 1);
                    break;
            }

            SceneDrawing.Quad(gl, radius, ShotDepth);
            gl.PopMatrix();
        }
    }
}
