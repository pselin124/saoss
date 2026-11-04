namespace Saoss.Network.Protocol;

/// <summary>A chat line [xät]. The host replaces the sender of lines from clients with the client's own nickname.</summary>
/// <param name="Sender">Nickname of the writer; empty in lines sent by clients.</param>
/// <param name="Text">The line, at most <see cref="ChatText.MaxLength"/> characters.</param>
public sealed record ChatMessage(string Sender, string Text) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.Chat;
}
