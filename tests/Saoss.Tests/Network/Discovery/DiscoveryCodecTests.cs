using System.Text;
using Saoss.Core;
using Saoss.Network.Discovery;
using Saoss.Network.Protocol;

namespace Saoss.Tests.Network.Discovery;

[TestClass]
public sealed class DiscoveryCodecTests
{
    private const int Token = -123456789;
    private const int InstanceId = 987654321;

    private static readonly LanGameInfo Game = new(ProtocolVersion.Current, 9999, "Pekka-Åö 2", 3, 5, 8, GameMode.CaptureTheFlower, IsInGame: true);

    [TestMethod]
    public void Request_RoundTrips_AndIsPaddedToTheMinimumSize()
    {
        var packet = DiscoveryCodec.EncodeRequest(Token);

        Assert.HasCount(DiscoveryProtocol.MinRequestSize, packet);
        Assert.IsTrue(DiscoveryCodec.TryDecodeRequest(packet, out var token));
        Assert.AreEqual(Token, token);
    }

    [TestMethod]
    public void Reply_RoundTrips_AndIsNoLargerThanARequest()
    {
        var longest = Game with { HostNickname = "ÅÅÅÅÅÅÅÅÅÅ" };
        var packet = DiscoveryCodec.EncodeReply(Token, InstanceId, longest);

        Assert.IsLessThanOrEqualTo(DiscoveryProtocol.MinRequestSize, packet.Length);
        Assert.IsTrue(DiscoveryCodec.TryDecodeReply(packet, out var reply));
        Assert.AreEqual(new DiscoveryReply(Token, InstanceId, longest), reply);
    }

    [TestMethod]
    public void Reply_WithAnUnknownMode_KeepsTheRestOfTheGame()
    {
        var packet = DiscoveryCodec.EncodeReply(Token, InstanceId, Game);
        packet[^2] = 200;

        Assert.IsTrue(DiscoveryCodec.TryDecodeReply(packet, out var reply));
        Assert.AreEqual(Game with { Mode = null }, reply.Game);
    }

    [TestMethod]
    public void MalformedRequests_AreIgnored()
    {
        var valid = DiscoveryCodec.EncodeRequest(Token);

        Assert.IsFalse(DiscoveryCodec.TryDecodeRequest(valid.AsSpan(0, DiscoveryProtocol.MinRequestSize - 1), out _), "too small to answer");
        Assert.IsFalse(DiscoveryCodec.TryDecodeRequest(new byte[DiscoveryProtocol.MaxPacketSize + 1], out _), "too large");
        Assert.IsFalse(DiscoveryCodec.TryDecodeRequest(Changed(valid, 0, (byte)'X'), out _), "magic");
        Assert.IsFalse(DiscoveryCodec.TryDecodeRequest(Changed(valid, 8, DiscoveryProtocol.Version + 1), out _), "version");
        Assert.IsFalse(DiscoveryCodec.TryDecodeRequest(Changed(valid, 9, (byte)DiscoveryPacketKind.Reply), out _), "kind");
        Assert.IsFalse(DiscoveryCodec.TryDecodeRequest(DiscoveryCodec.EncodeReply(Token, InstanceId, Game), out _), "a reply");
    }

    [TestMethod]
    public void MalformedReplies_AreIgnored()
    {
        var valid = DiscoveryCodec.EncodeReply(Token, InstanceId, Game);
        var nickname = Encoding.UTF8.GetByteCount(Game.HostNickname);
        var nicknameStart = 8 + 1 + 1 + 4 + 4 + 2 + 2 + 2;
        var countsStart = nicknameStart + nickname;

        Assert.IsTrue(DiscoveryCodec.TryDecodeReply(valid, out _));
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(valid.AsSpan(0, valid.Length - 1), out _), "truncated");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply([.. valid, 0], out _), "trailing byte");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, 9, (byte)DiscoveryPacketKind.Request), out _), "kind");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, nicknameStart - 2, 200), out _), "nickname length");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, nicknameStart, (byte)'!'), out _), "nickname character");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, nicknameStart, 0xFF), out _), "invalid UTF-8");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, countsStart, 17), out _), "humans");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, countsStart + 2, 1), out _), "maximum players");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(Changed(valid, valid.Length - 1, 2), out _), "flags");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply(new byte[DiscoveryProtocol.MaxPacketSize + 1], out _), "too large");
        Assert.IsFalse(DiscoveryCodec.TryDecodeReply([], out _), "empty");
    }

    [TestMethod]
    public void RandomBytes_NeverThrow()
    {
        var random = new Random(5);
        var valid = DiscoveryCodec.EncodeReply(Token, InstanceId, Game);
        for (var i = 0; i < 2000; i++)
        {
            var packet = (byte[])valid.Clone();
            packet[random.Next(packet.Length)] = (byte)random.Next(256);
            var cut = packet.AsSpan(0, random.Next(packet.Length + 1));
            DiscoveryCodec.TryDecodeReply(cut, out _);
            DiscoveryCodec.TryDecodeRequest(cut, out _);
        }
    }

    private static byte[] Changed(byte[] packet, int index, int value)
    {
        var copy = (byte[])packet.Clone();
        copy[index] = (byte)value;
        return copy;
    }
}
