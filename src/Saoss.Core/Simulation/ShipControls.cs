namespace Saoss.Core.Simulation;

/// <summary>
/// The buttons a pilot holds during one tick. Humans and the AI both drive ships through this.
/// </summary>
public record struct ShipControls
{
    /// <summary>Thrust forward [pylos].</summary>
    public bool Thrust { get; set; }

    /// <summary>Turn counter-clockwise [pleft].</summary>
    public bool Left { get; set; }

    /// <summary>Turn clockwise [pright].</summary>
    public bool Right { get; set; }

    /// <summary>Primary weapon [pfire].</summary>
    public bool Fire { get; set; }

    /// <summary>Special weapon [pease].</summary>
    public bool Special { get; set; }
}
