using Saoss.Network.Lobby;

namespace Saoss.Network.Protocol;

/// <summary>The host's whole lobby, sent whenever it changes [lahetaAlkuRuutu].</summary>
/// <param name="Lobby">Settings and slots; client-side copies have no member list.</param>
public sealed record LobbyUpdateMessage(LobbyState Lobby) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.LobbyUpdate;
}
