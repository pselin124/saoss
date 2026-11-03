namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// One spark or chunk of debris [Sirpale]. Purely visual.
/// </summary>
public sealed class Particle
{
    /// <summary>Position; Z is relative to the ship plane.</summary>
    public float X { get; set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; set; }

    /// <inheritdoc cref="X"/>
    public float Z { get; set; }

    /// <summary>Velocity per tick.</summary>
    public float VelocityX { get; set; }

    /// <inheritdoc cref="VelocityX"/>
    public float VelocityY { get; set; }

    /// <inheritdoc cref="VelocityX"/>
    public float VelocityZ { get; set; }

    /// <summary>Half size of the quad [koko].</summary>
    public float Size { get; set; }

    /// <summary>Color at full life.</summary>
    public float Red { get; set; }

    /// <inheritdoc cref="Red"/>
    public float Green { get; set; }

    /// <inheritdoc cref="Red"/>
    public float Blue { get; set; }

    /// <summary>Life used for fading: color is scaled by Life / MaxLife [maxaika].</summary>
    public float MaxLife { get; set; } = 1;

    /// <summary>Remaining ticks [aika].</summary>
    public int Life { get; set; }

    /// <summary>Square instead of round [muoto == 10].</summary>
    public bool IsSquare { get; set; }

    /// <summary>Whether the slot is in use.</summary>
    public bool IsActive => Life > 0;
}
