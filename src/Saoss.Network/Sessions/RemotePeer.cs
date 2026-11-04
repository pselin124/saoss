using Saoss.Network.Transport;

namespace Saoss.Network.Sessions;

/// <summary>A client connection on the host.</summary>
/// <param name="id">Client id; also the owner id of its lobby slots.</param>
/// <param name="connection">The connection.</param>
internal sealed class RemotePeer(int id, TcpConnection connection)
{
    /// <summary>Client id.</summary>
    public int Id { get; } = id;

    /// <summary>The connection.</summary>
    public TcpConnection Connection { get; } = connection;

    /// <summary>Nicknames of the client's players; empty until it has joined.</summary>
    public IReadOnlyList<string> Nicknames { get; set; } = [];

    /// <summary>Whether the client's players are in the lobby.</summary>
    public bool HasJoined => Nicknames.Count > 0;

    /// <summary>Tick of the newest input used, so older input arriving late is ignored.</summary>
    public int LastInputTick { get; set; } = -1;
}
