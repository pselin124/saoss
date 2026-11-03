namespace Saoss.Core.Simulation;

/// <summary>
/// A pooled moving object: a shot, a special-weapon object or a slime blob [Pati].
/// A slot is in use while <see cref="Lifetime"/> is positive.
/// </summary>
public sealed class Projectile
{
    /// <summary>Position in world units.</summary>
    public float X { get; set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; set; }

    /// <summary>Velocity per tick.</summary>
    public float VelocityX { get; set; }

    /// <inheritdoc cref="VelocityX"/>
    public float VelocityY { get; set; }

    /// <summary>Heading in degrees.</summary>
    public float Angle { get; set; }

    /// <summary>Remaining hit points when shot at.</summary>
    public float Energy { get; set; }

    /// <summary>Weapon-specific color or extra values [r, g, b].</summary>
    public float Red { get; set; }

    /// <inheritdoc cref="Red"/>
    public float Green { get; set; }

    /// <inheritdoc cref="Red"/>
    public float Blue { get; set; }

    /// <summary>Weapon-specific start value, e.g. animation phase [alku].</summary>
    public float Start { get; set; }

    /// <summary>Weapon-specific size [koko].</summary>
    public float Size { get; set; }

    /// <summary>Remaining ticks; zero or less means the slot is free [aika].</summary>
    public int Lifetime { get; set; }

    /// <summary>Weapon-specific delay counter [ampumisviive].</summary>
    public int Delay { get; set; }

    /// <summary>Whether the slot is in use.</summary>
    public bool IsActive => Lifetime > 0;
}
