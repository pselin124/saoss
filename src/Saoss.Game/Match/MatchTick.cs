using Saoss.Core.Simulation;

namespace Saoss.Game.Match;

/// <summary>One simulation tick to present: its sounds and effects come from <paramref name="Events"/>.</summary>
/// <param name="Events">Events of the tick, each to be played exactly once.</param>
public sealed record MatchTick(IReadOnlyList<GameEvent> Events);
