namespace Saoss.Network.Protocol;

/// <summary>
/// First message of a client: protocol version and the nicknames of the players on its machine
/// (the original's "Hello!" and "pelaajia" messages).
/// </summary>
/// <param name="Version">The client's <see cref="ProtocolVersion.Current"/>.</param>
/// <param name="Nicknames">One to four local players; empty when the version does not match.</param>
public sealed record HelloMessage(int Version, IReadOnlyList<string> Nicknames) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.Hello;
}
