namespace Saoss.Network.Discovery;

/// <summary>
/// Constants of LAN game discovery: joiners broadcast a small request on UDP <see cref="Port"/> and every host in a
/// lobby or match answers it directly. The original game had no discovery; the joiner typed the host's address.
/// </summary>
public static class DiscoveryProtocol
{
    /// <summary>UDP port hosts listen on for discovery requests, next to the game's TCP port 9999.</summary>
    public const int Port = 9998;

    /// <summary>Version of the discovery packets; packets of another version are ignored.</summary>
    public const int Version = 1;

    /// <summary>
    /// Requests are padded to this size and smaller ones are ignored, so a reply is never larger than the request
    /// that caused it and spoofed requests cannot amplify traffic.
    /// </summary>
    public const int MinRequestSize = 64;

    /// <summary>Largest packet accepted; anything bigger is dropped unread.</summary>
    public const int MaxPacketSize = 512;

    /// <summary>Bytes that open every discovery packet.</summary>
    public static ReadOnlySpan<byte> Magic => "SAOSSLAN"u8;
}
