namespace Saoss.Core.Simulation.Modes;

/// <summary>A Flower Domination control point [DPiste].</summary>
/// <param name="x">Position; whole numbers in the original.</param>
/// <param name="y">Position; whole numbers in the original.</param>
public sealed class ControlPoint(float x, float y)
{
    /// <summary>Position [x].</summary>
    public float X { get; } = x;

    /// <summary>Position [y].</summary>
    public float Y { get; } = y;

    /// <summary>Team that holds the point, or -1 while nobody has taken it yet [tiimi].</summary>
    public int OwnerTeam { get; set; } = -1;
}
