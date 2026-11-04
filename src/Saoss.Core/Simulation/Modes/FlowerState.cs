namespace Saoss.Core.Simulation.Modes;

/// <summary>Where a Capture The Flower flower is.</summary>
public enum FlowerState
{
    /// <summary>On its team's base [player == 100].</summary>
    AtHome,

    /// <summary>Lying where its carrier was destroyed [player == 101].</summary>
    Dropped,

    /// <summary>Carried by an enemy ship [player = carrier index].</summary>
    Carried,
}
