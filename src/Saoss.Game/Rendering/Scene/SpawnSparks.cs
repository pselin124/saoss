using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// The respawn animation: a shell of sparks in the new ship's colors swirls in toward the spawn point
/// and bursts into loose particles when the ship appears (laitaAlkuSirpaleet, laskeAlkuSirpaleet,
/// piirraAlkuSirpaleet).
/// </summary>
public static class SpawnSparks
{
    private const float MinRadiusSquared = 1000f;
    private const float MaxRadiusSquared = 1600f;
    private const float SwirlDegreesPerTick = 4f;
    private const float Shrink = 1.0165f;
    private const float BurstMaxSpeedSquared = 0.25f;

    /// <summary>Places the sparks on a random shell around the ship with the ship type's colors [laitaAlkuSirpaleet].</summary>
    public static void Place(Ship ship, ShipVisualState visual, GameRandom random)
    {
        foreach (var spark in visual.SpawnSparks)
        {
            spark.X = 100;
            spark.Y = 100;
            spark.Z = 100;
            var radiusSquared = (spark.X * spark.X) + (spark.Y * spark.Y) + (spark.Z * spark.Z);
            while (radiusSquared < MinRadiusSquared || radiusSquared > MaxRadiusSquared)
            {
                spark.X = (random.Next(16000) / 200f) - 40f;
                spark.Y = (random.Next(16000) / 200f) - 40f;
                spark.Z = (random.Next(16000) / 200f) - 40f;
                radiusSquared = (spark.X * spark.X) + (spark.Y * spark.Y) + (spark.Z * spark.Z);
            }

            spark.Size = (random.Next(250) / 100f) + 0.8f;
            spark.IsSquare = false;
            (spark.Red, spark.Green, spark.Blue) = SpawnSparkPalette.Pick(ship.Type, random);
            spark.Life = GameConstants.SpawnTicks;
            spark.MaxLife = GameConstants.SpawnTicks;
        }
    }

    /// <summary>Swirls the sparks 4 degrees around the ship and pulls them closer [laskeAlkuSirpaleet].</summary>
    public static void Advance(ShipVisualState visual)
    {
        foreach (var spark in visual.SpawnSparks)
        {
            if (!spark.IsActive)
            {
                continue;
            }

            spark.Life--;
            var angle = MathF.Atan2(spark.Y, spark.X) + (MathF.PI / 180f * SwirlDegreesPerTick);
            var distance = MathF.Sqrt((spark.X * spark.X) + (spark.Y * spark.Y));
            spark.X = MathF.Cos(angle) * distance / Shrink;
            spark.Y = MathF.Sin(angle) * distance / Shrink;
            spark.Z /= Shrink;
        }
    }

    /// <summary>
    /// When the ship appears, throws one particle per spark out from the ship in that spark's color
    /// (the <c>kuolema == 1</c> part of laskeAlkuSirpaleet).
    /// </summary>
    public static void Burst(Ship ship, ShipVisualState visual, ParticleSystem particles, GameRandom random)
    {
        var remaining = visual.SpawnSparks.Length;
        particles.Spawn(remaining, p =>
        {
            remaining--;
            p.X = ship.X;
            p.Y = ship.Y;
            p.Z = 0;
            p.VelocityX = 1;
            p.VelocityY = 1;
            p.VelocityZ = 1;
            while ((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY) + (p.VelocityZ * p.VelocityZ) > BurstMaxSpeedSquared)
            {
                p.VelocityX = (random.Next(10000) / 10000f) - 0.5f;
                p.VelocityY = (random.Next(10000) / 10000f) - 0.5f;
                p.VelocityZ = (random.Next(10000) / 10000f) - 0.5f;
            }

            p.MaxLife = 200;
            p.Life = (int)(random.Next(150) + 50);
            p.Size = (random.Next(180) / 100f) + 0.35f;
            var source = visual.SpawnSparks[remaining];
            p.Red = source.Red;
            p.Green = source.Green;
            p.Blue = source.Blue;
        });
    }

    /// <summary>Draws the sparks around the ship, fading out as they close in [piirraAlkuSirpaleet].</summary>
    public static void Draw(LegacyGl gl, SceneTextures textures, Ship ship, ShipVisualState visual)
    {
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.One);
        foreach (var spark in visual.SpawnSparks)
        {
            if (!spark.IsActive)
            {
                continue;
            }

            var progress = 1f - (spark.Life / spark.MaxLife);
            var alpha = 1f - (progress * progress);
            gl.PushMatrix();
            gl.Translate(ship.X, ship.Y, GameConstants.ShipPlaneZ);
            gl.Translate(spark.X, spark.Y, spark.Z);
            gl.BindTexture(textures.Flare);
            gl.Color(spark.Red, spark.Green, spark.Blue, alpha);
            SceneDrawing.Quad(gl, spark.Size, z: 1f);
            gl.PopMatrix();
        }

        gl.BlendFunc(GlBlend.SrcColor, GlBlend.OneMinusSrcColor);
    }
}
