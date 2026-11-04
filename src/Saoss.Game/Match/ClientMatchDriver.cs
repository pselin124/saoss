using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Network.Protocol;
using Saoss.Network.Sessions;

namespace Saoss.Game.Match;

/// <summary>
/// A client's side of a network match (netpeli.cpp netJoinPeli): sends the local ships' buttons every update and
/// presents the host ticks that arrived. The world is the session's mirror and is never stepped here; the session
/// itself is updated by its owner before the screen.
/// </summary>
public sealed class ClientMatchDriver : IMatchDriver
{
    /// <summary>Mirrored ticks after which a purchase the host has not taken is offered again.</summary>
    public const int ChoiceRetryTicks = 200;

    private readonly ClientSession _session;
    private readonly Dictionary<int, int> _sentChoices = [];

    /// <summary>Drives the match <paramref name="session"/> is mirroring.</summary>
    public ClientMatchDriver(ClientSession session)
    {
        _session = session;
        World = session.World ?? throw new InvalidOperationException("The client has no match.");
        LocalShips = session.LocalShips;
    }

    /// <inheritdoc/>
    public World World { get; }

    /// <inheritdoc/>
    public IReadOnlyList<int> LocalShips { get; }

    /// <inheritdoc/>
    public bool LatchesSpecialLocally => false;

    /// <inheritdoc/>
    public int? LocalChoice
    {
        get
        {
            foreach (var (ship, sentAt) in _sentChoices.ToList())
            {
                if (!World.IsChoosingShip(ship) || World.Tick - sentAt > ChoiceRetryTicks)
                {
                    _sentChoices.Remove(ship);
                }
            }

            return LocalShips.Cast<int?>().FirstOrDefault(i => World.IsChoosingShip(i!.Value) && !_sentChoices.ContainsKey(i.Value));
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<MatchTick> Advance(IReadOnlyDictionary<int, ShipControls> localControls)
    {
        _session.SendInputs([.. LocalShips.Select(i => new ShipInput(i, localControls.GetValueOrDefault(i)))]);
        return [.. _session.ReceivedTicks.Select(t => new MatchTick(t.Events))];
    }

    /// <summary>
    /// Asks the host for the ship. Affordability is checked on the mirror first, like the original menu did, so a
    /// refused purchase jumps to the fallback ship at once.
    /// </summary>
    public bool ChooseShip(int shipIndex, ShipType? type)
    {
        if (type is { } chosen && !ShipShop.CanAfford(World.Ships[shipIndex], ShipCatalog.Get(chosen)))
        {
            return false;
        }

        _session.ChooseShip(shipIndex, type);
        _sentChoices[shipIndex] = World.Tick;
        return true;
    }
}
