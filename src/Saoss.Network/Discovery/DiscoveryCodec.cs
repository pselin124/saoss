using System.Diagnostics.CodeAnalysis;
using Saoss.Core;
using Saoss.Network.Lobby;
using Saoss.Network.Protocol;

namespace Saoss.Network.Discovery;

/// <summary>
/// Wire format of discovery packets, little-endian: the magic "SAOSSLAN", the discovery version byte, the
/// <see cref="DiscoveryPacketKind"/> byte and the request token (int). A request is then zero-padded to
/// <see cref="DiscoveryProtocol.MinRequestSize"/>. A reply goes on with the host's instance id (int), game protocol
/// version (ushort), TCP port (ushort), nickname (ushort byte length + UTF-8), humans, open slots, maximum players,
/// game mode and flags (one byte each; flag 1 = in game). Decoding never throws: malformed packets give false.
/// </summary>
public static class DiscoveryCodec
{
    private const int InGameFlag = 1;
    private const int AllFlags = InGameFlag;
    private const int MaxPort = ushort.MaxValue;

    /// <summary>Encodes a request carrying <paramref name="token"/>.</summary>
    public static byte[] EncodeRequest(int token)
    {
        var packet = new byte[DiscoveryProtocol.MinRequestSize];
        using var writer = new BinaryWriter(new MemoryStream(packet));
        WriteHeader(writer, DiscoveryPacketKind.Request, token);
        return packet;
    }

    /// <summary>Encodes the reply of host <paramref name="instanceId"/> to the request with <paramref name="token"/>.</summary>
    public static byte[] EncodeReply(int token, int instanceId, LanGameInfo game)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        WriteHeader(writer, DiscoveryPacketKind.Reply, token);
        writer.Write(instanceId);
        writer.Write((ushort)game.ProtocolVersion);
        writer.Write((ushort)game.Port);
        SettingsCodec.WriteString(writer, game.HostNickname, Nickname.MaxLength);
        writer.Write((byte)game.HumanPlayers);
        writer.Write((byte)game.OpenSlots);
        writer.Write((byte)game.MaxPlayers);
        writer.Write((byte)(game.Mode ?? 0));
        writer.Write((byte)(game.IsInGame ? InGameFlag : 0));
        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>Reads a request; false for anything else, including requests below the minimum size.</summary>
    public static bool TryDecodeRequest(ReadOnlySpan<byte> packet, out int token)
    {
        token = 0;
        if (packet.Length is < DiscoveryProtocol.MinRequestSize or > DiscoveryProtocol.MaxPacketSize)
        {
            return false;
        }

        try
        {
            token = ReadHeader(new WireReader(packet), DiscoveryPacketKind.Request);
            return true;
        }
        catch (ProtocolException)
        {
            return false;
        }
    }

    /// <summary>Reads a reply; false for anything malformed.</summary>
    public static bool TryDecodeReply(ReadOnlySpan<byte> packet, [NotNullWhen(true)] out DiscoveryReply? reply)
    {
        reply = null;
        if (packet.Length > DiscoveryProtocol.MaxPacketSize)
        {
            return false;
        }

        try
        {
            reply = ReadReply(new WireReader(packet));
            return true;
        }
        catch (ProtocolException)
        {
            return false;
        }
    }

    private static void WriteHeader(BinaryWriter writer, DiscoveryPacketKind kind, int token)
    {
        writer.Write(DiscoveryProtocol.Magic);
        writer.Write((byte)DiscoveryProtocol.Version);
        writer.Write((byte)kind);
        writer.Write(token);
    }

    private static int ReadHeader(WireReader reader, DiscoveryPacketKind kind)
    {
        foreach (var expected in DiscoveryProtocol.Magic)
        {
            if (reader.ReadByte(byte.MaxValue, "magic") != expected)
            {
                throw new ProtocolException("Not a discovery packet.");
            }
        }

        if (reader.ReadByte(byte.MaxValue, "discovery version") != DiscoveryProtocol.Version
            || reader.ReadByte(byte.MaxValue, "packet kind") != (int)kind)
        {
            throw new ProtocolException("Unexpected discovery version or packet kind.");
        }

        return reader.ReadInt(int.MinValue, int.MaxValue, "token");
    }

    private static DiscoveryReply ReadReply(WireReader reader)
    {
        var token = ReadHeader(reader, DiscoveryPacketKind.Reply);
        var instanceId = reader.ReadInt(int.MinValue, int.MaxValue, "instance id");
        var version = reader.ReadUShort(ushort.MaxValue, "protocol version");
        var port = reader.ReadUShort(MaxPort, "port");
        var nickname = SettingsCodec.ReadNickname(reader, allowEmpty: false);
        var humans = reader.ReadByte(LobbyState.SlotCount, "humans");
        var open = reader.ReadByte(LobbyState.SlotCount, "open slots");
        var maxPlayers = reader.ReadByte(LobbyState.SlotCount, "maximum players");
        var rawMode = (GameMode)reader.ReadByte(byte.MaxValue, "game mode");
        var flags = reader.ReadByte(AllFlags, "flags");
        reader.EnsureEnd();
        if (port == 0 || maxPlayers < LobbyState.MinPlayers || humans > maxPlayers || open > maxPlayers)
        {
            throw new ProtocolException("Inconsistent discovery reply.");
        }

        GameMode? mode = Enum.IsDefined(rawMode) ? rawMode : null;
        var game = new LanGameInfo(version, port, nickname, humans, open, maxPlayers, mode, (flags & InGameFlag) != 0);
        return new DiscoveryReply(token, instanceId, game);
    }
}
