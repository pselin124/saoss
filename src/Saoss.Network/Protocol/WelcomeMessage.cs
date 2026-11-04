namespace Saoss.Network.Protocol;

/// <summary>The host accepted the client [you have joined the game].</summary>
/// <param name="ClientId">The client's id; lobby slots owned by the client carry it.</param>
public sealed record WelcomeMessage(int ClientId) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.Welcome;
}
