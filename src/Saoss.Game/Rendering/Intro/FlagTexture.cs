using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// Paints the intro flag: a white field with a blue Nordic cross in the Finnish flag's proportions (the early
/// development versions flew the Finnish flag) and a small red ship flying in the upper fly. Only large, bold shapes,
/// so the waving cloth stays calm to look at; the texture is mipmapped so it does not shimmer when the flag is small.
/// </summary>
public static class FlagTexture
{
    /// <summary>Width and height of the texture in pixels.</summary>
    public const int Size = 512;

    // The picture is laid out in flag units: 4 across and 3 down, the cloth's proportions (6 by 4.5).
    private const float Width = 4f;
    private const float Height = 3f;
    private const int Supersampling = 4;

    // The Finnish flag's cross divides the length 5:3:10 and the height 4:3:4.
    private const float CrossLeft = Width * 5 / 18;
    private const float CrossRight = Width * 8 / 18;
    private const float CrossTop = Height * 4 / 11;
    private const float CrossBottom = Height * 7 / 11;

    private static readonly Color Field = new(246, 246, 250);
    private static readonly Color Cross = new(16, 74, 170);
    private static readonly Color Hull = new(212, 44, 40);
    private static readonly Color Fin = Cross;
    private static readonly Color Window = new(150, 210, 255);
    private static readonly Color Flame = new(255, 136, 28);
    private static readonly Color FlameCore = new(255, 214, 60);

    private static readonly Vector2 ShipCenter = new(2.95f, 0.55f);
    private static readonly float ShipAngle = MathHelper.ToRadians(-14f);
    private static readonly Vector2[] OuterFlame = [new(-0.8f, 0), new(-0.4f, -0.12f), new(-0.4f, 0.12f)];
    private static readonly Vector2[] InnerFlame = [new(-0.64f, 0), new(-0.4f, -0.065f), new(-0.4f, 0.065f)];
    private static readonly Vector2[] UpperFin = [new(-0.46f, -0.08f), new(-0.12f, -0.1f), new(-0.52f, -0.36f)];
    private static readonly Vector2[] LowerFin = [new(-0.46f, 0.08f), new(-0.12f, 0.1f), new(-0.52f, 0.36f)];
    private static readonly Vector2 BodyRadii = new(0.5f, 0.17f);
    private static readonly Vector2 WindowCenter = new(0.14f, 0);
    private const float WindowRadius = 0.075f;

    /// <summary>Creates the mipmapped flag texture.</summary>
    public static Texture2D Create(GraphicsDevice device)
    {
        var texture = new Texture2D(device, Size, Size, mipmap: true, SurfaceFormat.Color);
        var level = Paint();
        var size = Size;
        for (var mip = 0; mip < texture.LevelCount; mip++)
        {
            texture.SetData(mip, null, level, 0, level.Length);
            if (size > 1)
            {
                level = Halve(level, size);
                size /= 2;
            }
        }

        return texture;
    }

    /// <summary>
    /// The full-size picture, row by row from the top-left (the hoist's top corner), each pixel averaged over a grid
    /// of samples for smooth edges.
    /// </summary>
    public static Color[] Paint()
    {
        var pixels = new Color[Size * Size];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var sum = Vector3.Zero;
                for (var sy = 0; sy < Supersampling; sy++)
                {
                    for (var sx = 0; sx < Supersampling; sx++)
                    {
                        var point = new Vector2(
                            (x + ((sx + 0.5f) / Supersampling)) / Size * Width,
                            (y + ((sy + 0.5f) / Supersampling)) / Size * Height);
                        sum += ColorAt(point).ToVector3();
                    }
                }

                pixels[(y * Size) + x] = new Color(sum / (Supersampling * Supersampling));
            }
        }

        return pixels;
    }

    /// <summary>The flag's color at <paramref name="point"/> in flag units (x right from the pole, y down).</summary>
    public static Color ColorAt(Vector2 point)
    {
        if ((point.X >= CrossLeft && point.X < CrossRight) || (point.Y >= CrossTop && point.Y < CrossBottom))
        {
            return Cross;
        }

        return ShipColorAt(ToShip(point)) ?? Field;
    }

    private static Color? ShipColorAt(Vector2 local)
    {
        var body = local / BodyRadii;
        if (Vector2.Distance(local, WindowCenter) < WindowRadius)
        {
            return Window;
        }

        if (body.LengthSquared() < 1)
        {
            return Hull;
        }

        if (IsInside(local, UpperFin) || IsInside(local, LowerFin))
        {
            return Fin;
        }

        if (IsInside(local, InnerFlame))
        {
            return FlameCore;
        }

        return IsInside(local, OuterFlame) ? Flame : null;
    }

    // Into the ship's own frame: x along its axis toward the nose, y across.
    private static Vector2 ToShip(Vector2 point)
    {
        var offset = point - ShipCenter;
        var (sin, cos) = MathF.SinCos(-ShipAngle);
        return new Vector2((offset.X * cos) - (offset.Y * sin), (offset.X * sin) + (offset.Y * cos));
    }

    // Even-odd rule.
    private static bool IsInside(Vector2 point, Vector2[] polygon)
    {
        var inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            var a = polygon[i];
            var b = polygon[j];
            if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < ((b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y)) + a.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    // The next mipmap level: each pixel the average of a 2x2 block.
    private static Color[] Halve(Color[] pixels, int size)
    {
        var half = size / 2;
        var result = new Color[half * half];
        for (var y = 0; y < half; y++)
        {
            for (var x = 0; x < half; x++)
            {
                var i = (2 * y * size) + (2 * x);
                var sum = pixels[i].ToVector4() + pixels[i + 1].ToVector4() + pixels[i + size].ToVector4() + pixels[i + size + 1].ToVector4();
                result[(y * half) + x] = new Color(sum / 4);
            }
        }

        return result;
    }
}
