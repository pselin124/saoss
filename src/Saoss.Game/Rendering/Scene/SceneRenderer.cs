using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws one frame of a match, a port of the original <c>drawFrame</c>: background galaxies and stars,
/// ships, pickups, asteroids, planet, the flower-and-star arena border, particles and the HUD.
/// </summary>
public sealed class SceneRenderer
{
    private const float FieldOfView = 45f;
    private const float NearPlane = 1f;
    private const float FarPlane = 1024f;
    private const float OriginalPi = 3.14f;
    private const float BackgroundDepth = 200f;
    private const float HudDepth = -30f;
    private const int BorderStarsPerSide = 20;
    private const float BorderStarSize = 6f;
    private const float PickupSize = 0.4f;

    private readonly LegacyGl _gl;
    private readonly SceneTextures _textures;
    private readonly SharedModels _models;
    private readonly ShipRenderer _ships;
    private readonly int _detail;

    /// <summary>Creates the renderer and applies the original global GL setup (setupOpengl).</summary>
    public SceneRenderer(LegacyGl gl, SceneTextures textures, SharedModels models, ShipRenderer ships, int detail)
    {
        _gl = gl;
        _textures = textures;
        _models = models;
        _ships = ships;
        _detail = detail;
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.ColorMaterial);
        gl.LightModelLocalViewer(true);
        gl.LightModelAmbient(0.35f, 0.35f, 0.35f, 1f);
        gl.Enable(GlCapability.Light0);
        gl.LightDiffuse(0, 1, 1, 1, 1);
        gl.LightSpecular(0, 1, 1, 1, 1);

        // The original main menu set this and it stayed in effect during matches.
        gl.MaterialShininess(120);
    }

    /// <summary>Draws the match as seen by <paramref name="state"/>'s camera.</summary>
    public void Draw(World world, SceneState state)
    {
        var gl = _gl;
        var camera = state.Camera;
        gl.MatrixMode(GlMatrixMode.Projection);
        gl.LoadIdentity();
        gl.Perspective(FieldOfView, (float)gl.ViewportWidth / gl.ViewportHeight, NearPlane, FarPlane);
        gl.MatrixMode(GlMatrixMode.ModelView);
        gl.LoadIdentity();
        gl.Clear();

        // State left by the end of the previous original frame.
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);

        var alpha2 = camera.SmoothedAlpha / 180f * OriginalPi;
        var beta = camera.Beta / 180f * OriginalPi;
        gl.LookAt(
            camera.X / 4f, camera.Y / 4f, camera.Z,
            camera.X, camera.Y, GameConstants.ShipPlaneZ,
            MathF.Cos(alpha2) * MathF.Cos(beta), MathF.Sin(alpha2) * MathF.Cos(beta), -MathF.Sin(beta));

        gl.MaterialSpecular(1, 1, 1, 1);
        gl.LightDiffuse(0, 0.8f, 0.8f, 0.8f, 0.8f);
        gl.LightPosition(0, 0, 0, -25, 1);

        DrawBackground(state);

        foreach (var ship in world.Ships)
        {
            if (ship.IsInGame && ship.IsAlive)
            {
                _ships.DrawShip(ship, world, state);
            }
        }

        foreach (var ship in world.Ships)
        {
            if (ship.IsInGame && ship.DeathTimer > 0 && ship.DeathTimer < GameConstants.SpawnTicks)
            {
                _ships.DrawSpawnSparks(ship, state);
            }
        }

        DrawPickups(world);
        DrawAsteroids(world, state);
        DrawPlanet(state);
        DrawBorder(camera);

        state.Particles.Draw(gl, _textures, camera);

        gl.BlendFunc(GlBlend.SrcColor, GlBlend.OneMinusSrcColor);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
        gl.LoadIdentity();
        gl.Translate(0, 0, HudDepth);
        foreach (var ship in world.Ships)
        {
            _ships.DrawHud(ship, world, state);
        }

        gl.Flush();
    }

    private void DrawBackground(SceneState state)
    {
        var gl = _gl;
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        if (_detail > 1)
        {
            DrawGalaxy(_textures.Galaxy, 1.7f * GameConstants.ArenaMaxX, -GameConstants.ArenaMaxY / 2f, 12f);
        }

        if (_detail > 0)
        {
            DrawGalaxy(_textures.Galaxy2, -0.7f * GameConstants.ArenaMaxX, -GameConstants.ArenaMaxY / 3f, 8f);
            gl.BindTexture(_textures.Flare);
            foreach (var star in state.Stars.Stars)
            {
                gl.PushMatrix();
                gl.Translate(star.X, star.Y, GameConstants.ShipPlaneZ + star.Z);
                gl.Color(star.Red, star.Green, star.Blue, 1f);
                SceneDrawing.Quad(gl, star.Size);
                gl.PopMatrix();
            }
        }
    }

    private void DrawGalaxy(Microsoft.Xna.Framework.Graphics.Texture2D texture, float x, float y, float size)
    {
        _gl.Color(1, 1, 1, 1);
        _gl.BindTexture(texture);
        _gl.PushMatrix();
        _gl.Translate(x, y, GameConstants.ShipPlaneZ - BackgroundDepth);
        SceneDrawing.Quad(_gl, size);
        _gl.PopMatrix();
    }

    private void DrawPickups(World world)
    {
        var gl = _gl;
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        foreach (var pickup in world.Pickups)
        {
            if (!pickup.IsActive)
            {
                continue;
            }

            gl.PushMatrix();
            gl.Translate(pickup.X, pickup.Y, GameConstants.ShipPlaneZ);
            gl.Rotate(-90, 0, 0, 1);
            gl.BindTexture(_textures.LittleMan);
            gl.Color(1, 1, 1, 1);
            SceneDrawing.Quad(gl, PickupSize, z: 1f);
            gl.PopMatrix();
        }
    }

    private void DrawAsteroids(World world, SceneState state)
    {
        var gl = _gl;
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);
        gl.BindTexture(_textures.Meteor);
        gl.Color(1, 1, 1, 1);
        foreach (var asteroid in world.Asteroids.Asteroids)
        {
            gl.PushMatrix();
            gl.Translate(asteroid.X, asteroid.Y, GameConstants.ShipPlaneZ);
            gl.Rotate((state.RotationX * asteroid.SpinAxisX) + asteroid.SpinPhase, 1, 0, 0);
            gl.Rotate((state.RotationY * asteroid.SpinAxisY) + asteroid.SpinPhase, 0, 1, 0);
            gl.Rotate((state.RotationZ * asteroid.SpinAxisZ) + asteroid.SpinPhase, 0, 0, 1);
            gl.Scale(asteroid.Size, asteroid.Size, asteroid.Size);
            gl.CallList(_models.Asteroid);
            gl.PopMatrix();
        }

        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Lighting);
    }

    /// <summary>
    /// The planet: a lit sphere covered with 60 cone spikes spread evenly over it (drawPlanet).
    /// Steps: spin the planet, draw the core sphere, then place spikes ring by ring using the original's
    /// equal-area point distribution.
    /// </summary>
    private void DrawPlanet(SceneState state)
    {
        const int spikeCount = 60;
        const int ringCount = 8;
        var gl = _gl;
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Blend);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.CullFace);
        gl.PushMatrix();
        gl.Translate(0, 0, GameConstants.ShipPlaneZ);
        gl.Scale(2.2f, 2.2f, 2.2f);
        gl.Rotate(state.RotationX, 1, 0, 0);
        gl.Rotate(state.RotationY, 0, 1, 0);
        gl.Rotate(state.RotationZ, 0, 0, 1);
        gl.Color(1, 1, 1);
        gl.Sphere(3, 8 + (_detail * 4), 8 + (_detail * 4));

        var beta = 4.0 / Math.Sqrt(spikeCount);
        var theta = (Math.PI - beta) / (ringCount - 2);
        var idealCounts = new double[ringCount];
        idealCounts[0] = 1;
        idealCounts[ringCount - 1] = 1;
        for (var i = 1; i < ringCount - 1; i++)
        {
            idealCounts[i] = spikeCount * (Math.Cos((theta * (i - 1)) + (beta / 2)) - Math.Cos((theta * i) + (beta / 2))) / 2;
        }

        var carry = 0.0;
        for (var i = 0; i < ringCount; i++)
        {
            var count = idealCounts[i] - (int)(idealCounts[i] + carry) < 0.5 ? (int)(idealCounts[i] + carry) : (int)(idealCounts[i] + carry) + 1;
            carry += idealCounts[i] - count;
            var ringAngle = ((i - 1 + 0.5) * theta) + (beta / 2);
            var offset = ((i - 1) & 1) / 2.0;
            for (var j = 0; j < count; j++)
            {
                var around = 2 * Math.PI * (j + offset) / count;
                gl.PushMatrix();
                gl.Rotate((float)(around * 360 / (2 * Math.PI)), 0, 1, 0);
                gl.Rotate((float)((ringAngle * 360 / (2 * Math.PI)) + 90), 1, 0, 0);
                gl.Translate(0, 0, 2.8f);
                gl.Cylinder(0.5f, 0, 1, 8 + (_detail * 4), 1 + (_detail * 2));
                gl.PopMatrix();
            }
        }

        gl.PopMatrix();
        gl.Disable(GlCapability.CullFace);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
    }

    /// <summary>
    /// The arena border: four rows of 20 glowing stars along the edges with a big flower in each corner.
    /// Each row's color fades between two colors.
    /// </summary>
    private void DrawBorder(GameCamera camera)
    {
        var gl = _gl;
        gl.PushMatrix();
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        gl.BindTexture(_textures.Flare);
        gl.Translate(GameConstants.ArenaMaxX, GameConstants.ArenaMaxY, GameConstants.ShipPlaneZ);
        var stepX = GameConstants.ArenaMaxX / 10f;
        var stepY = GameConstants.ArenaMaxY / 10f;

        DrawBorderSide(camera, _models.Flower1, -stepX, 0, f => (1 - f, 0, f));
        gl.Translate(-stepX, 0, 0);
        DrawBorderSide(camera, _models.Flower2, 0, -stepY, f => (f, 0, 1 - f));
        gl.Translate(0, -stepY, 0);
        DrawBorderSide(camera, _models.Flower1, stepX, 0, f => (1 - f, f, 0));
        gl.Translate(stepX, 0, 0);
        DrawBorderSide(camera, _models.Flower3, 0, stepY, f => (f, 1 - f, 0));

        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcColor);
        gl.PopMatrix();
    }

    private void DrawBorderSide(GameCamera camera, GlDisplayList cornerFlower, float stepX, float stepY, Func<float, (float R, float G, float B)> color)
    {
        var gl = _gl;
        for (var i = 0; i < BorderStarsPerSide; i++)
        {
            var (r, g, b) = color(i / (float)BorderStarsPerSide);
            gl.Color(r, g, b, 0.8f);
            if (i != 0)
            {
                gl.Translate(stepX, stepY, 0);
                gl.PushMatrix();
                SceneDrawing.FaceCamera(gl, camera);
                SceneDrawing.Quad(gl, BorderStarSize);
                gl.PopMatrix();
                continue;
            }

            gl.Enable(GlCapability.DepthTest);
            gl.Enable(GlCapability.Lighting);
            gl.Disable(GlCapability.Blend);
            gl.Disable(GlCapability.Texture2D);
            gl.PushMatrix();
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 1, 0, 0);
            gl.CallList(cornerFlower);
            gl.PopMatrix();
            gl.Enable(GlCapability.Blend);
            gl.Enable(GlCapability.Texture2D);
            gl.Disable(GlCapability.Lighting);
            gl.Disable(GlCapability.DepthTest);
        }
    }
}
