using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Match;

/// <summary>
/// Runs a match for <see cref="Screens.GameScreen"/>: a local simulation, the host of a network game or a client
/// mirroring the host. The screen reads the keys of <see cref="LocalShips"/>, calls <see cref="Advance"/> once per
/// update and presents the returned ticks.
/// </summary>
public interface IMatchDriver
{
    /// <summary>The match as this machine sees it.</summary>
    World World { get; }

    /// <summary>Ships played on this machine; the n-th gets the n-th keyboard layout.</summary>
    IReadOnlyList<int> LocalShips { get; }

    /// <summary>
    /// Whether the special button is latched here after each step (<see cref="Input.LocalPlayerInput.AfterStep"/>);
    /// a client leaves that to the host.
    /// </summary>
    bool LatchesSpecialLocally { get; }

    /// <summary>The first local ship that must buy a ship now, or null.</summary>
    int? LocalChoice { get; }

    /// <summary>
    /// Gives the local ships their controls and moves the match on; returns the ticks to present, in order, each
    /// with its events.
    /// </summary>
    IReadOnlyList<MatchTick> Advance(IReadOnlyDictionary<int, ShipControls> localControls);

    /// <summary>
    /// Buys <paramref name="type"/> (null: a random affordable ship) for <see cref="LocalChoice"/>; false when the
    /// player cannot afford it.
    /// </summary>
    bool ChooseShip(int shipIndex, ShipType? type);
}
