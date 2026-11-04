using Saoss.Core.Simulation;

namespace Saoss.Network.Protocol;

/// <summary>The buttons one ship holds.</summary>
/// <param name="ShipIndex">The ship.</param>
/// <param name="Controls">Its buttons.</param>
public readonly record struct ShipInput(int ShipIndex, ShipControls Controls);
