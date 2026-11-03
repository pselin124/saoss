using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Exhaust trails behind ships (laskeKaasujaljet, piirraKaasujaljet): a few red glows dropped at the engine
/// every 10 ticks while thrusting. Only visible at detail 0; higher detail levels use thrust sparks instead.
/// </summary>
public static class ExhaustTrails
{
    private const int UpdateInterval = 10;
    private const float FadePerUpdate = 0.01f;
    private const float StraightBack = -180f;

    /// <summary>
    /// Heading offsets in degrees of the ship's engines: Rollu and Popper have two, every other ship one
    /// straight back. Shared with the thrust sparks.
    /// </summary>
    public static (float First, float? Second) NozzleOffsets(ShipType type) => type switch
    {
        ShipType.Rollu => (-145f, -215f),
        ShipType.Popper => (-155f, -205f),
        _ => (StraightBack, null),
    };

    /// <summary>Point on the ship's radius at <paramref name="offsetDegrees"/> from its heading.</summary>
    public static (float X, float Y) NozzlePosition(Ship ship, float offsetDegrees)
    {
        var radians = (ship.Angle + offsetDegrees) / 180f * MathF.PI;
        return (ship.X + (ship.Definition.Radius * MathF.Cos(radians)), ship.Y + (ship.Definition.Radius * MathF.Sin(radians)));
    }

    /// <summary>Lights the newest puffs while the engine is on (done in liiku at detail 0 only).</summary>
    public static void MarkThrust(Ship ship, ShipVisualState visual)
    {
        visual.Trail[0].Alpha = 1f;
        if (HasTwinTrail(ship.Type))
        {
            visual.SecondTrail[0].Alpha = 1f;
        }
    }

    /// <summary>
    /// Advances the trails one tick [laskeKaasujaljet]. Every 10 ticks: a lit newest puff moves to the engine,
    /// an unlit one is cleared, older puffs take the previous puff's position with a fixed fading alpha,
    /// and the newest puff dims slightly so it goes out unless the engine relights it.
    /// </summary>
    public static void Update(Ship ship, ShipVisualState visual)
    {
        if (visual.TrailTimer >= 1)
        {
            visual.TrailTimer--;
            return;
        }

        visual.TrailTimer = UpdateInterval;
        var twin = HasTwinTrail(ship.Type);
        var (first, second) = NozzleOffsets(ship.Type);
        if (visual.Trail[0].Alpha >= 1f)
        {
            (visual.Trail[0].X, visual.Trail[0].Y) = NozzlePosition(ship, first);
            if (second is { } offset)
            {
                (visual.SecondTrail[0].X, visual.SecondTrail[0].Y) = NozzlePosition(ship, offset);
            }
        }
        else
        {
            visual.Trail[0].Alpha = 0;
            if (twin)
            {
                visual.SecondTrail[0].Alpha = 0;
            }
        }

        Shift(visual.Trail);
        visual.Trail[0].Alpha -= FadePerUpdate;
        if (twin)
        {
            Shift(visual.SecondTrail);
            visual.SecondTrail[0].Alpha -= FadePerUpdate;
        }
    }

    /// <summary>Draws the trails as additive red glows [piirraKaasujaljet].</summary>
    public static void Draw(LegacyGl gl, SceneTextures textures, Ship ship, ShipVisualState visual)
    {
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        if (ship.Type is not ShipType.Elephant and not ShipType.StarWorm)
        {
            DrawPuffs(gl, textures, ship, visual.Trail);
        }

        if (HasTwinTrail(ship.Type))
        {
            DrawPuffs(gl, textures, ship, visual.SecondTrail);
        }

        gl.BlendFunc(GlBlend.SrcColor, GlBlend.OneMinusSrcColor);
    }

    private static bool HasTwinTrail(ShipType type) => type is ShipType.Rollu or ShipType.Popper;

    private static void Shift(ExhaustPuff[] trail)
    {
        for (var i = trail.Length - 1; i > 0; i--)
        {
            trail[i].Alpha = trail[i - 1].Alpha > 0 ? 1f - ((i + 1f) / trail.Length) : 0f;
            trail[i].X = trail[i - 1].X;
            trail[i].Y = trail[i - 1].Y;
        }
    }

    // With additive blending the alpha has no effect, so every visible puff is full red.
    private static void DrawPuffs(LegacyGl gl, SceneTextures textures, Ship ship, ExhaustPuff[] trail)
    {
        var size = ship.Definition.TrailSize * 2f;
        foreach (var puff in trail)
        {
            if (puff.Alpha <= 0f)
            {
                continue;
            }

            gl.PushMatrix();
            gl.Translate(puff.X, puff.Y, GameConstants.ShipPlaneZ);
            gl.BindTexture(textures.Flare);
            gl.Color(1, 0, 0, puff.Alpha);
            SceneDrawing.Quad(gl, size, z: 1f);
            gl.PopMatrix();
        }
    }
}
