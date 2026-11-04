using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Match;

/// <summary>
/// A match played on this computer only: every human is local and the match pauses while one buys a ship.
/// </summary>
public sealed class LocalMatchDriver : IMatchDriver
{
    /// <summary>Starts a match; its humans get the keyboard layouts in ship order.</summary>
    public LocalMatchDriver(MatchSettings settings, int seed)
    {
        World = new World(settings, seed);
        LocalShips = [.. World.Ships.Where(s => s.IsHuman).Select(s => s.Index)];
    }

    /// <inheritdoc/>
    public World World { get; }

    /// <inheritdoc/>
    public IReadOnlyList<int> LocalShips { get; }

    /// <inheritdoc/>
    public bool LatchesSpecialLocally => true;

    /// <inheritdoc/>
    public int? LocalChoice => World.PendingChoice?.ShipIndex;

    /// <inheritdoc/>
    public IReadOnlyList<MatchTick> Advance(IReadOnlyDictionary<int, ShipControls> localControls)
    {
        if (World.PendingChoice != null)
        {
            return [];
        }

        foreach (var (index, controls) in localControls)
        {
            World.Ships[index].Controls = controls;
        }

        World.Step();

        // Events of ship purchases made since the previous step are still in the list and play now.
        return [new MatchTick([.. World.Events])];
    }

    /// <inheritdoc/>
    public bool ChooseShip(int shipIndex, ShipType? type) => World.ChooseShip(shipIndex, type);
}
