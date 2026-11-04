using Saoss.Core.Simulation;

namespace Saoss.Network.Sessions;

/// <summary>A host tick mirrored by a client: the events to play for it.</summary>
/// <param name="Tick">The world tick after the update.</param>
/// <param name="Events">Events new in this update, in order; play each exactly once.</param>
public sealed record ReceivedTick(int Tick, IReadOnlyList<GameEvent> Events);
