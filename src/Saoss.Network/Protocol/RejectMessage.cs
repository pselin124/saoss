namespace Saoss.Network.Protocol;

/// <summary>The host refuses the client and closes the connection.</summary>
/// <param name="Reason">Why.</param>
public sealed record RejectMessage(RejectReason Reason) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.Reject;
}
