using System.Buffers.Binary;
using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Snapshots;
using Saoss.Network.Lobby;

namespace Saoss.Network.Protocol;

/// <summary>
/// Turns messages into payloads (a type byte followed by little-endian fields) and back, and frames payloads as
/// <c>[int32 length][payload]</c>. Decoding validates every length, count, range and string and throws
/// <see cref="ProtocolException"/> on anything wrong.
/// </summary>
public static class MessageCodec
{
    /// <summary>Largest payload accepted; a longer frame drops the connection.</summary>
    public const int MaxFrameSize = 1024 * 1024;

    /// <summary>Size of the length prefix of a frame.</summary>
    public const int FrameHeaderSize = sizeof(int);

    private const byte RandomShip = byte.MaxValue;

    /// <summary>Encodes a message payload.</summary>
    public static byte[] Encode(Message message)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((byte)message.Type);
            WriteBody(writer, message);
        }

        return stream.ToArray();
    }

    /// <summary>Encodes a message as a complete frame, length prefix included.</summary>
    public static byte[] EncodeFrame(Message message)
    {
        var payload = Encode(message);
        if (payload.Length > MaxFrameSize)
        {
            throw new InvalidOperationException($"{message.Type} message of {payload.Length} bytes exceeds the frame limit.");
        }

        var frame = new byte[FrameHeaderSize + payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(frame, payload.Length);
        payload.CopyTo(frame, FrameHeaderSize);
        return frame;
    }

    /// <summary>Decodes a payload produced by <see cref="Encode"/>.</summary>
    public static Message Decode(ReadOnlySpan<byte> payload)
    {
        var reader = new WireReader(payload);
        var type = reader.ReadEnum<MessageType>("message type");
        var message = ReadBody(reader, type);
        if (message is not HelloMessage { Nicknames.Count: 0 })
        {
            reader.EnsureEnd();
        }

        return message;
    }

    private static void WriteBody(BinaryWriter writer, Message message)
    {
        switch (message)
        {
            case HelloMessage hello:
                WriteHello(writer, hello);
                break;
            case WelcomeMessage welcome:
                writer.Write((byte)welcome.ClientId);
                break;
            case RejectMessage reject:
                writer.Write((byte)reject.Reason);
                break;
            case LobbyUpdateMessage lobby:
                SettingsCodec.WriteLobby(writer, lobby.Lobby);
                break;
            case TeamChangeRequestMessage team:
                writer.Write((byte)team.Slot);
                writer.Write((byte)team.Team);
                break;
            case ChatMessage chat:
                SettingsCodec.WriteString(writer, chat.Sender, Nickname.MaxLength);
                SettingsCodec.WriteString(writer, chat.Text, ChatText.MaxLength);
                break;
            default:
                WriteMatchBody(writer, message);
                break;
        }
    }

    private static void WriteMatchBody(BinaryWriter writer, Message message)
    {
        switch (message)
        {
            case MatchStartMessage start:
                SettingsCodec.WriteSettings(writer, start.Settings);
                writer.Write(start.Seed);
                writer.Write([.. start.ShipSlots.Select(slot => (byte)slot)]);
                break;
            case PlayerInputMessage input:
                writer.Write(input.Tick);
                writer.Write((byte)input.Inputs.Count);
                foreach (var ship in input.Inputs)
                {
                    writer.Write((byte)ship.ShipIndex);
                    writer.Write(ShipControlsBits.ToBits(ship.Controls));
                }

                break;
            case ShipChoiceRequestMessage choice:
                writer.Write((byte)choice.ShipIndex);
                writer.Write(choice.ShipType is { } type ? (byte)type : RandomShip);
                break;
            case WorldUpdateMessage update:
                writer.Write((ushort)update.FirstNewEvent);
                writer.Write(update.Snapshot);
                break;
            case ReturnToLobbyMessage or GoodbyeMessage or HeartbeatMessage:
                break;
            default:
                throw new ArgumentException($"Unknown message {message.GetType().Name}.", nameof(message));
        }
    }

    private static void WriteHello(BinaryWriter writer, HelloMessage hello)
    {
        writer.Write(ProtocolVersion.Magic.Select(c => (byte)c).ToArray());
        writer.Write(hello.Version);
        writer.Write((byte)hello.Nicknames.Count);
        foreach (var nickname in hello.Nicknames)
        {
            SettingsCodec.WriteString(writer, nickname, Nickname.MaxLength);
        }
    }

    private static Message ReadBody(WireReader reader, MessageType type) => type switch
    {
        MessageType.Hello => ReadHello(reader),
        MessageType.Welcome => new WelcomeMessage(ReadClientId(reader)),
        MessageType.Reject => new RejectMessage(reader.ReadEnum<RejectReason>("reject reason")),
        MessageType.LobbyUpdate => new LobbyUpdateMessage(SettingsCodec.ReadLobby(reader)),
        MessageType.TeamChangeRequest => new TeamChangeRequestMessage(ReadSlot(reader), reader.ReadByte(LobbyState.SlotCount - 1, "team")),
        MessageType.Chat => new ChatMessage(SettingsCodec.ReadNickname(reader, allowEmpty: true), reader.ReadString(ChatText.MaxLength, "chat text")),
        MessageType.MatchStart => ReadMatchStart(reader),
        MessageType.PlayerInput => ReadPlayerInput(reader),
        MessageType.ShipChoiceRequest => ReadShipChoice(reader),
        MessageType.WorldUpdate => new WorldUpdateMessage(reader.ReadUShort(WorldSnapshot.MaxEvents, "first new event"), reader.ReadRest()),
        MessageType.ReturnToLobby => new ReturnToLobbyMessage(),
        MessageType.Goodbye => new GoodbyeMessage(),
        _ => new HeartbeatMessage(),
    };

    private static HelloMessage ReadHello(WireReader reader)
    {
        foreach (var expected in ProtocolVersion.Magic)
        {
            if (reader.ReadByte(byte.MaxValue, "magic") != expected)
            {
                throw new ProtocolException("Not a Saoss client.");
            }
        }

        var version = reader.ReadInt(int.MinValue, int.MaxValue, "version");
        if (version != ProtocolVersion.Current)
        {
            return new HelloMessage(version, []);
        }

        var nicknames = new string[reader.ReadByte(LobbyState.MaxLocalPlayers, "nickname count")];
        if (nicknames.Length == 0)
        {
            throw WireReader.Invalid("nickname count", 0);
        }

        for (var i = 0; i < nicknames.Length; i++)
        {
            nicknames[i] = SettingsCodec.ReadNickname(reader, allowEmpty: false);
        }

        return new HelloMessage(version, nicknames);
    }

    private static int ReadClientId(WireReader reader)
    {
        var id = reader.ReadByte(LobbyState.SlotCount - 1, "client id");
        return id > LobbyState.HostId ? id : throw WireReader.Invalid("client id", id);
    }

    private static int ReadSlot(WireReader reader) => reader.ReadByte(LobbyState.SlotCount - 1, "slot");

    private static MatchStartMessage ReadMatchStart(WireReader reader)
    {
        var settings = SettingsCodec.ReadSettings(reader);
        var seed = reader.ReadInt(int.MinValue, int.MaxValue, "seed");
        var slots = new int[settings.Players.Count];
        for (var i = 0; i < slots.Length; i++)
        {
            slots[i] = ReadSlot(reader);
            if (i > 0 && slots[i] <= slots[i - 1])
            {
                throw WireReader.Invalid("ship slot order", slots[i]);
            }
        }

        return new MatchStartMessage(settings, seed, slots);
    }

    private static PlayerInputMessage ReadPlayerInput(WireReader reader)
    {
        var tick = reader.ReadInt(0, int.MaxValue, "tick");
        var inputs = new ShipInput[reader.ReadByte(LobbyState.MaxLocalPlayers, "input count")];
        for (var i = 0; i < inputs.Length; i++)
        {
            var ship = reader.ReadByte(GameConstants.MaxPlayers - 1, "ship index");
            inputs[i] = new ShipInput(ship, ShipControlsBits.FromBits((byte)reader.ReadByte(ShipControlsBits.AllBits, "buttons")));
        }

        return new PlayerInputMessage(tick, inputs);
    }

    private static ShipChoiceRequestMessage ReadShipChoice(WireReader reader)
    {
        var ship = reader.ReadByte(GameConstants.MaxPlayers - 1, "ship index");
        var type = reader.ReadByte(RandomShip, "ship type");
        if (type != RandomShip && type >= GameConstants.ShipTypeCount)
        {
            throw WireReader.Invalid("ship type", type);
        }

        return new ShipChoiceRequestMessage(ship, type == RandomShip ? null : (ShipType)type);
    }
}
