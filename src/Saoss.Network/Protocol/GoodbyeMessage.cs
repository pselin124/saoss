namespace Saoss.Network.Protocol;

/// <summary>The sender is leaving and closes the connection [quiting..].</summary>
public sealed record GoodbyeMessage : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.Goodbye;
}
