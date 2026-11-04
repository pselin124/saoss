namespace Saoss.Network.Protocol;

/// <summary>The host's world after a tick [pal ... plo].</summary>
/// <param name="FirstNewEvent">
/// Index of the first event in the snapshot's event list that no earlier update carried; earlier ones were already
/// played.
/// </param>
/// <param name="Snapshot">A <see cref="Core.Simulation.Snapshots.WorldSnapshot"/>.</param>
public sealed record WorldUpdateMessage(int FirstNewEvent, byte[] Snapshot) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.WorldUpdate;
}
