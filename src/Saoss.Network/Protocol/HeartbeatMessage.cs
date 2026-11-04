namespace Saoss.Network.Protocol;

/// <summary>Sent on an otherwise idle connection so the peer does not time it out.</summary>
public sealed record HeartbeatMessage : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.Heartbeat;
}
