namespace Saoss.Network.Protocol;

/// <summary>The match is over; everybody is back in the lobby.</summary>
public sealed record ReturnToLobbyMessage : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.ReturnToLobby;
}
