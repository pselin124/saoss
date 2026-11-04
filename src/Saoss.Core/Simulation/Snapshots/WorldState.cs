using Saoss.Core.Simulation.Modes;

namespace Saoss.Core.Simulation.Snapshots;

/// <summary>Match-level state of a <see cref="World"/> read from a snapshot.</summary>
/// <param name="Tick">Number of completed steps.</param>
/// <param name="Events">The world's event list.</param>
/// <param name="StepEventCount">How many of <paramref name="Events"/> the next step drops.</param>
/// <param name="ChoiceQueue">Players waiting to buy a ship, in order.</param>
/// <param name="PendingChoiceIndex">Ship of the pending choice, or null.</param>
/// <param name="Outcome">How the match ended, or null.</param>
internal sealed record WorldState(
    int Tick,
    IReadOnlyList<GameEvent> Events,
    int StepEventCount,
    IReadOnlyList<int> ChoiceQueue,
    int? PendingChoiceIndex,
    MatchOutcome? Outcome);
