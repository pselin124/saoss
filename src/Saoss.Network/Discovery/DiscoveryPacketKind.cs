namespace Saoss.Network.Discovery;

/// <summary>Kind of a discovery packet, stored after the magic and version.</summary>
public enum DiscoveryPacketKind : byte
{
    /// <summary>A joiner asks which games are open.</summary>
    Request = 1,

    /// <summary>A host describes its game.</summary>
    Reply = 2,
}
