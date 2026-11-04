using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Ai;
using Saoss.Core.Simulation.Snapshots;
using Saoss.Network.Lobby;
using Saoss.Network.Protocol;
using Saoss.Tests.Simulation.Snapshots;

namespace Saoss.Tests.Network;

[TestClass]
public sealed class MessageCodecTests
{
    private const int FuzzRounds = 500;

    [TestMethod]
    public void EveryMessage_RoundTrips()
    {
        foreach (var message in SampleMessages())
        {
            var decoded = MessageCodec.Decode(MessageCodec.Encode(message));
            if (message is LobbyUpdateMessage { Lobby: var sent })
            {
                // Client copies carry the settings and slots, not the host's member list or version counter.
                var received = ((LobbyUpdateMessage)decoded).Lobby;
                Assert.AreEqual(
                    (sent.MaxPlayers, sent.Mode, sent.Credits, sent.AiTargetsOnlyEnemies, sent.AiDifficulty),
                    (received.MaxPlayers, received.Mode, received.Credits, received.AiTargetsOnlyEnemies, received.AiDifficulty));
                DeepComparer.AssertEqual(sent.Slots, received.Slots, "lobby slots");
                continue;
            }

            DeepComparer.AssertEqual(message, decoded, message.Type.ToString());
        }
    }

    [TestMethod]
    public void EncodeFrame_PrefixesLittleEndianLength()
    {
        var frame = MessageCodec.EncodeFrame(new WelcomeMessage(3));

        CollectionAssert.AreEqual(new byte[] { 2, 0, 0, 0, (byte)MessageType.Welcome, 3 }, frame);
    }

    [TestMethod]
    public void Hello_WithOtherVersion_DecodesWithoutNicknames()
    {
        var decoded = (HelloMessage)MessageCodec.Decode(MessageCodec.Encode(new HelloMessage(ProtocolVersion.Current + 1, ["a"])));

        Assert.AreEqual(ProtocolVersion.Current + 1, decoded.Version);
        Assert.IsEmpty(decoded.Nicknames);
    }

    [TestMethod]
    public void Decode_RejectsMalformedPayloads()
    {
        byte[][] bad =
        [
            [],
            [0],
            [99],
            [(byte)MessageType.Welcome],
            [(byte)MessageType.Welcome, 0],
            [(byte)MessageType.Welcome, 3, 0],
            [(byte)MessageType.Reject, 9],
            [(byte)MessageType.Hello, (byte)'X', (byte)'A', (byte)'O', (byte)'S', (byte)'S', 1, 0, 0, 0],
            [(byte)MessageType.TeamChangeRequest, 1, 16],
            [(byte)MessageType.PlayerInput, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [(byte)MessageType.PlayerInput, 0, 0, 0, 0, 1, 0, 32],
            [(byte)MessageType.ShipChoiceRequest, 0, 24],
        ];

        foreach (var payload in bad)
        {
            Assert.ThrowsExactly<ProtocolException>(() => MessageCodec.Decode(payload), $"[{string.Join(",", payload)}]");
        }
    }

    [TestMethod]
    public void Decode_RejectsBadStrings()
    {
        Assert.ThrowsExactly<ProtocolException>(() => MessageCodec.Decode(MessageCodec.Encode(new HelloMessage(ProtocolVersion.Current, ["bad<nick>"]))));
        Assert.ThrowsExactly<ProtocolException>(() => MessageCodec.Decode(MessageCodec.Encode(new ChatMessage("x", "ok") with { Sender = "a\nb" })));

        var tooLong = ChatPayload(new string('a', ChatText.MaxLength + 1));
        Assert.ThrowsExactly<ProtocolException>(() => MessageCodec.Decode(tooLong));

        byte[] invalidUtf8 = [(byte)MessageType.Chat, 0, 0, 2, 0, 0xC3, 0x28];
        Assert.ThrowsExactly<ProtocolException>(() => MessageCodec.Decode(invalidUtf8));
    }

    [TestMethod]
    public void Decode_CorruptedMessages_ThrowOnlyProtocolException()
    {
        var random = new Random(5);
        var samples = SampleMessages().Select(MessageCodec.Encode).ToList();
        for (var round = 0; round < FuzzRounds; round++)
        {
            var bytes = (byte[])samples[round % samples.Count].Clone();
            if (bytes.Length > 0)
            {
                bytes[random.Next(bytes.Length)] = (byte)random.Next(256);
            }

            var length = random.Next(3) == 0 ? random.Next(bytes.Length + 1) : bytes.Length;
            try
            {
                MessageCodec.Decode(bytes.AsSpan(0, length));
            }
            catch (ProtocolException)
            {
                // Expected for most corruptions.
            }
        }
    }

    private static byte[] ChatPayload(string text)
    {
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        return [(byte)MessageType.Chat, 0, 0, (byte)textBytes.Length, (byte)(textBytes.Length >> 8), .. textBytes];
    }

    private static IEnumerable<Message> SampleMessages()
    {
        var lobby = new LobbyState(["host", "Äijä-2"]);
        lobby.TryJoin(1, ["a.b c"]);
        lobby.CycleStatus(5);
        lobby.SetTeam(1, 7);
        lobby.SetMode(GameMode.CaptureTheFlower);
        lobby.SetCredits(1300);
        lobby.SetAiTargetsOnlyEnemies(true);
        lobby.SetAiDifficulty(AiDifficulty.RocketAce);
        var settings = lobby.ToMatchSettings();
        var world = new World(settings, 4);
        ScriptedMatch.Run(world, 200, 4);

        return
        [
            new HelloMessage(ProtocolVersion.Current, ["one", "Två", "3", "four four"]),
            new WelcomeMessage(15),
            new RejectMessage(RejectReason.LobbyFull),
            new LobbyUpdateMessage(lobby),
            new TeamChangeRequestMessage(15, 15),
            new ChatMessage("host", "Hei kaikki! Ääkköset ja emoji \U0001F680 toimivat."),
            new ChatMessage(string.Empty, new string('x', ChatText.MaxLength)),
            new MatchStartMessage(settings, -12345, lobby.PlayerSlots()),
            new PlayerInputMessage(4321, [new ShipInput(0, new ShipControls { Fire = true, Left = true }), new ShipInput(15, new ShipControls { Thrust = true, Special = true, Right = true })]),
            new ShipChoiceRequestMessage(3, ShipType.Popper),
            new ShipChoiceRequestMessage(0, null),
            new WorldUpdateMessage(2, WorldSnapshot.ToBytes(world)),
            new ReturnToLobbyMessage(),
            new GoodbyeMessage(),
            new HeartbeatMessage(),
        ];
    }
}
