namespace Saoss.Network.Protocol;

/// <summary>A protocol message; encoded and decoded by <see cref="MessageCodec"/>.</summary>
public abstract record Message
{
    /// <summary>Kind written as the first payload byte.</summary>
    public abstract MessageType Type { get; }
}
