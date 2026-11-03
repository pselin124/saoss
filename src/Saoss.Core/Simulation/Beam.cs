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
