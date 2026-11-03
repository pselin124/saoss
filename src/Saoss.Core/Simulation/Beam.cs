namespace Saoss.Core.Simulation;

/// <summary>
/// A line drawn by a weapon, e.g. a laser [Viivakasa]. It moves with its velocity until it expires.
/// </summary>
public sealed class Beam
{
    /// <summary>Start point.</summary>
    public float X1 { get; set; }

    /// <inheritdoc cref="X1"/>
    public float Y1 { get; set; }

    /// <summary>End point.</summary>
    public float X2 { get; set; }

    /// <inheritdoc cref="X2"/>
    public float Y2 { get; set; }

    /// <summary>Color components.</summary>
    public float Red { get; set; }

    /// <inheritdoc cref="Red"/>
    public float Green { get; set; }

    /// <inheritdoc cref="Red"/>
    public float Blue { get; set; }

    /// <summary>Remaining ticks.</summary>
    public int Lifetime { get; set; }

    /// <summary>Velocity of both end points per tick.</summary>
    public float VelocityX { get; set; }

    /// <inheritdoc cref="VelocityX"/>
    public float VelocityY { get; set; }

    /// <summary>Whether the beam is visible.</summary>
    public bool IsActive => Lifetime > 0;

    /// <summary>Shows the beam between two points; the velocity is left unchanged.</summary>
    public void Set(float x1, float y1, float x2, float y2, float red, float green, float blue, int lifetime)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Red = red;
        Green = green;
        Blue = blue;
        Lifetime = lifetime;
    }

    /// <summary>Moves the beam and counts down its lifetime [laskeViivat].</summary>
    public void Update()
    {
        if (Lifetime <= 0)
        {
            return;
        }

        Lifetime--;
        X1 += VelocityX;
        X2 += VelocityX;
        Y1 += VelocityY;
        Y2 += VelocityY;
    }
}
