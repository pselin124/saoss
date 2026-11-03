namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// One glowing puff of an exhaust trail [Kaasujalki]. Purely visual.
/// </summary>
public sealed class ExhaustPuff
{
    /// <summary>Position in world units.</summary>
    public float X { get; set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; set; }

    /// <summary>Visibility; the puff is drawn while above zero.</summary>
    public float Alpha { get; set; }
}
