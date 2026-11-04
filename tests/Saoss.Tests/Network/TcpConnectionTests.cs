using System.Net.Sockets;
using Saoss.Network.Protocol;
using Saoss.Network.Transport;

namespace Saoss.Tests.Network;

[TestClass]
public sealed class TcpConnectionTests
{
    private static readonly ConnectionOptions FastTimeout = new() { HeartbeatInterval = TimeSpan.FromMilliseconds(50), Timeout = TimeSpan.FromMilliseconds(300) };

    [TestMethod]
    public void Messages_FlowBothWaysInOrder()
    {
        using var host = new NetHost(port: 0);
        using var client = NetClient.ConnectAsync("127.0.0.1", host.Port, Poll.DefaultTimeout).GetAwaiter().GetResult();
        var server = Accept(host);

        for (var i = 1; i <= 3; i++)
        {
            client.Send(new WelcomeMessage(i));
        }

        server.Send(new ChatMessage("x", "y"));
        var received = new List<Message>();
        Poll.Until(() => received.Count == 3, () => Drain(server, received), "three messages");
        Message? reply = null;
        Poll.Until(() => reply != null, () => client.TryReceive(out reply), "the reply");

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, received.Cast<WelcomeMessage>().Select(m => m.ClientId).ToArray());
        Assert.AreEqual(new ChatMessage("x", "y"), reply);
    }

    [TestMethod]
    public void OversizedFrame_ClosesWithProtocolViolation()
    {
        using var host = new NetHost(port: 0);
        using var raw = new TcpClient("127.0.0.1", host.Port);
        var server = Accept(host);

        raw.GetStream().Write(BitConverter.GetBytes(MessageCodec.MaxFrameSize + 1));

        Poll.Until(() => server.IsClosed, server.Update, "the connection to close");
        Assert.AreEqual(CloseReason.ProtocolViolation, server.CloseReason);
    }

    [TestMethod]
    public void MalformedPayload_ClosesWithProtocolViolation()
    {
        using var host = new NetHost(port: 0);
        using var raw = new TcpClient("127.0.0.1", host.Port);
        var server = Accept(host);

        raw.GetStream().Write([1, 0, 0, 0, 200]);

        Poll.Until(() => server.IsClosed, server.Update, "the connection to close");
        Assert.AreEqual(CloseReason.ProtocolViolation, server.CloseReason);
    }

    [TestMethod]
    public void SilentPeer_TimesOut_WhileHeartbeatsKeepAnIdleConnectionAlive()
    {
        using var host = new NetHost(port: 0, FastTimeout);
        using var silent = new TcpClient("127.0.0.1", host.Port);
        var server = Accept(host);
        using var idle = NetClient.ConnectAsync("127.0.0.1", host.Port, Poll.DefaultTimeout, FastTimeout).GetAwaiter().GetResult();
        var idleServer = Accept(host);

        Poll.Until(() => server.IsClosed, () => { server.Update(); idle.Update(); idleServer.Update(); }, "the silent peer to time out");

        Assert.AreEqual(CloseReason.TimedOut, server.CloseReason);
        Assert.IsFalse(idleServer.IsClosed);
        Assert.IsFalse(idle.IsClosed);
    }

    [TestMethod]
    public void CloseAfterSending_DeliversTheLastMessage()
    {
        using var host = new NetHost(port: 0);
        using var client = NetClient.ConnectAsync("localhost", host.Port, Poll.DefaultTimeout).GetAwaiter().GetResult();
        var server = Accept(host);

        server.Send(new RejectMessage(RejectReason.LobbyFull));
        server.CloseAfterSending();

        Message? message = null;
        Poll.Until(() => message != null, () => client.TryReceive(out message), "the rejection");
        Poll.Until(() => client.IsClosed, client.Update, "the client to see the close");
        Assert.AreEqual(new RejectMessage(RejectReason.LobbyFull), message);
        Assert.AreEqual(CloseReason.RemoteClosed, client.CloseReason);
    }

    private static TcpConnection Accept(NetHost host)
    {
        TcpConnection? connection = null;
        Poll.Until(() => connection != null, () => host.TryAccept(out connection), "a connection");
        return connection!;
    }

    private static void Drain(TcpConnection connection, List<Message> into)
    {
        while (connection.TryReceive(out var message))
        {
            into.Add(message);
        }
    }
}
