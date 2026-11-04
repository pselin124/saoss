namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// A human player must pick a ship from the purchase menu before the match can go on (valitseAlus).
/// </summary>
/// <param name="ShipIndex">The player choosing.</param>
public sealed record ShipChoice(int ShipIndex);
