namespace Saoss.Network.Protocol;

/// <summary>
/// Identity of the wire protocol. Peers with a different <see cref="Current"/> version are turned away; there is
/// no compatibility with the original game's protocol.
/// </summary>
public static class ProtocolVersion
{
    /// <summary>Version sent in <see cref="HelloMessage"/>; bump on any change to messages or the snapshot format.</summary>
    public const int Current = 2;

    /// <summary>Bytes that open every <see cref="HelloMessage"/>, so stray connections are recognized at once.</summary>
    public const string Magic = "SAOSS";

    /// <summary>TCP port of the original game, used by default [9999].</summary>
    public const int DefaultPort = 9999;
}
