using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Network.Sessions;

namespace Saoss.Game.Match;

/// <summary>
/// The host's side of a network match (netpeli.cpp netHostPeli): local ships from the keyboard, remote ships from
/// their clients, one step per update and the result sent to every client. Nobody waits for a buyer. The session
/// itself is updated by its owner before the screen.
/// </summary>
public sealed class HostMatchDriver : IMatchDriver
{
    private readonly HostSession _session;

    /// <summary>Drives the match <paramref name="session"/> has started.</summary>
    public HostMatchDriver(HostSession session)
    {
        _session = session;
        World = session.World ?? throw new InvalidOperationException("The host has no running match.");
        LocalShips = [.. Enumerable.Range(0, World.Ships.Count).Where(session.IsLocalShip)];
    }

    /// <inheritdoc/>
    public World World { get; }

    /// <inheritdoc/>
    public IReadOnlyList<int> LocalShips { get; }

    /// <inheritdoc/>
    public bool LatchesSpecialLocally => true;

    /// <inheritdoc/>
    public int? LocalChoice => LocalShips.Cast<int?>().FirstOrDefault(i => World.IsChoosingShip(i!.Value));

    /// <inheritdoc/>
    public IReadOnlyList<MatchTick> Advance(IReadOnlyDictionary<int, ShipControls> localControls)
    {
        foreach (var (index, controls) in localControls)
        {
            World.Ships[index].Controls = controls;
        }

        _session.ApplyClientInputs(World);
        World.Step();
        _session.BroadcastWorld(World);
        return [new MatchTick([.. World.Events])];
    }

    /// <inheritdoc/>
    public bool ChooseShip(int shipIndex, ShipType? type) => World.ChooseShip(shipIndex, type);
}
