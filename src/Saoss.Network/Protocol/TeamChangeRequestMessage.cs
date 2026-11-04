namespace Saoss.Network.Protocol;

/// <summary>A client asks to put one of its own slots on another team [team].</summary>
/// <param name="Slot">Lobby slot.</param>
/// <param name="Team">New team, 0-15.</param>
public sealed record TeamChangeRequestMessage(int Slot, int Team) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.TeamChangeRequest;
}
