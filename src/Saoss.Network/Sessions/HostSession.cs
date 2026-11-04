using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Snapshots;
using Saoss.Network.Lobby;
using Saoss.Network.Protocol;
using Saoss.Network.Transport;

namespace Saoss.Network.Sessions;

/// <summary>
/// The hosting side of a network game (net.cpp hostMenu/hostStart, netpeli.cpp netHostPeli). Owns the listener and
/// the authoritative <see cref="LobbyState"/>; the host's UI changes the lobby directly and calls
/// <see cref="Update"/> every frame. During a match the UI calls <see cref="ApplyClientInputs"/> before each
/// <see cref="World.Step"/> and <see cref="BroadcastWorld"/> after it. All methods are non-blocking and must be
/// called from the game thread.
/// </summary>
public sealed class HostSession : IDisposable
{
    private const string HostSenderFallback = "host";

    private readonly NetHost _net;
    private readonly Dictionary<int, RemotePeer> _peers = [];
    private readonly Dictionary<int, ShipControls> _inputs = [];
    private readonly Dictionary<int, ShipControls> _appliedInputs = [];
    private readonly HashSet<int> _specialConsumed = [];
    private readonly ChatLog _chat = new();
    private readonly string _chatSender;
    private HashSet<GameEvent> _sentEvents = new(ReferenceEqualityComparer.Instance);
    private int[] _shipOwners = [];
    private int _sentLobbyVersion = -1;
    private bool _isShutDown;

    /// <summary>Starts listening and creates the lobby with the host's own players (0-4 nicknames).</summary>
    public HostSession(IReadOnlyList<string> localNicknames, int port = ProtocolVersion.DefaultPort, ConnectionOptions? options = null)
    {
        Lobby = new LobbyState(localNicknames);
        _chatSender = localNicknames.Count > 0 ? Nickname.Sanitize(localNicknames[0]) : HostSenderFallback;
        _net = new NetHost(port, options);
    }

    /// <summary>The authoritative lobby; the host's UI edits it directly between matches.</summary>
    public LobbyState Lobby { get; }

    /// <summary>The port listened on.</summary>
    public int Port => _net.Port;

    /// <summary>Nickname of the host's first player, as it signs chat lines.</summary>
    public string HostNickname => _chatSender;

    /// <summary>The running match, or null while in the lobby.</summary>
    public World? World { get; private set; }

    /// <summary>Lobby slot of every ship of the running match.</summary>
    public IReadOnlyList<int> ShipSlots { get; private set; } = [];

    /// <summary>Recent chat lines, oldest first, including the host's own.</summary>
    public IReadOnlyList<ChatLine> ChatLog => _chat.Lines;

    /// <summary>Number of clients whose players are in the lobby or match.</summary>
    public int JoinedClientCount => _peers.Values.Count(p => p.HasJoined);

    /// <summary>Whether the ship of the running match is played on this machine.</summary>
    public bool IsLocalShip(int shipIndex) => shipIndex >= 0 && shipIndex < _shipOwners.Length && _shipOwners[shipIndex] == LobbyState.HostId;

    /// <summary>Accepts connections, handles client messages and departures, and sends lobby changes.</summary>
    public void Update()
    {
        if (_isShutDown)
        {
            return;
        }

        AcceptConnections();
        foreach (var peer in _peers.Values.ToList())
        {
            peer.Connection.Update();
            while (peer.Connection.TryReceive(out var message) && !peer.Connection.IsClosed)
            {
                Handle(peer, message);
            }

            if (peer.Connection.IsClosed)
            {
                RemovePeer(peer);
            }
        }

        BroadcastLobbyIfChanged();
    }

    /// <summary>Sends a chat line from the host's first player to everybody.</summary>
    public void SendChat(string text) => PostChat(_chatSender, text);

    /// <summary>
    /// Starts a match from the lobby: creates the world and tells every client the settings, seed and ship slots.
    /// </summary>
    public World StartMatch(int seed)
    {
        if (World != null)
        {
            throw new InvalidOperationException("A match is already running.");
        }

        BroadcastLobbyIfChanged();
        var settings = Lobby.ToMatchSettings();
        ShipSlots = Lobby.PlayerSlots();
        _shipOwners = [.. ShipSlots.Select(slot => Lobby.Slots[slot].OwnerId)];
        _inputs.Clear();
        _appliedInputs.Clear();
        _specialConsumed.Clear();
        _sentEvents = new HashSet<GameEvent>(ReferenceEqualityComparer.Instance);
        foreach (var peer in _peers.Values)
        {
            peer.LastInputTick = -1;
        }

        World = new World(settings, seed);
        Broadcast(new MatchStartMessage(settings, seed, ShipSlots));
        return World;
    }

    /// <summary>
    /// Gives every remote human ship the buttons its client sent last. The special button is latched like a local
    /// player's: once the simulation clears it, it stays off until the client releases it.
    /// </summary>
    public void ApplyClientInputs(World world)
    {
        for (var i = 0; i < _shipOwners.Length; i++)
        {
            if (_shipOwners[i] > LobbyState.HostId && world.Ships[i].IsHuman)
            {
                var controls = _inputs.GetValueOrDefault(i);
                if (!controls.Special)
                {
                    _specialConsumed.Remove(i);
                }
                else if (_specialConsumed.Contains(i))
                {
                    controls.Special = false;
                }

                world.Ships[i].Controls = controls;
                _appliedInputs[i] = controls;
            }
        }
    }

    /// <summary>Sends the world and the events no earlier update carried to every client; call after the step.</summary>
    public void BroadcastWorld(World world)
    {
        foreach (var (ship, applied) in _appliedInputs)
        {
            if (applied.Special && !world.Ships[ship].Controls.Special)
            {
                _specialConsumed.Add(ship);
            }
        }

        _appliedInputs.Clear();
        var firstNew = 0;
        while (firstNew < world.Events.Count && _sentEvents.Contains(world.Events[firstNew]))
        {
            firstNew++;
        }

        _sentEvents = new HashSet<GameEvent>(world.Events, ReferenceEqualityComparer.Instance);
        var frame = MessageCodec.EncodeFrame(new WorldUpdateMessage(firstNew, WorldSnapshot.ToBytes(world)));
        foreach (var peer in _peers.Values.Where(p => p.HasJoined))
        {
            peer.Connection.SendFrame(frame);
        }
    }

    /// <summary>Ends the match and brings everybody back to the lobby; clients who left stay AI.</summary>
    public void ReturnToLobby()
    {
        World = null;
        ShipSlots = [];
        _shipOwners = [];
        _inputs.Clear();
        Broadcast(new ReturnToLobbyMessage());
    }

    /// <summary>Says goodbye to every client and stops listening.</summary>
    public void Shutdown()
    {
        if (_isShutDown)
        {
            return;
        }

        _isShutDown = true;
        foreach (var peer in _peers.Values)
        {
            peer.Connection.Send(new GoodbyeMessage());
            peer.Connection.CloseAfterSending();
        }

        _peers.Clear();
        _net.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose() => Shutdown();

    private void AcceptConnections()
    {
        while (_net.TryAccept(out var connection))
        {
            var id = Enumerable.Range(1, LobbyState.SlotCount - 1).FirstOrDefault(i => !_peers.ContainsKey(i));
            if (World != null || id == 0)
            {
                Reject(connection, World != null ? RejectReason.GameInProgress : RejectReason.LobbyFull);
                continue;
            }

            _peers[id] = new RemotePeer(id, connection);
        }
    }

    private static void Reject(TcpConnection connection, RejectReason reason)
    {
        connection.Send(new RejectMessage(reason));
        connection.CloseAfterSending();
    }

    private void Handle(RemotePeer peer, Message message)
    {
        if (!peer.HasJoined)
        {
            HandleJoin(peer, message);
            return;
        }

        switch (message)
        {
            case TeamChangeRequestMessage team when World == null:
                Lobby.TrySetTeam(peer.Id, team.Slot, team.Team);
                break;
            case ChatMessage chat:
                PostChat(peer.Nicknames[0], chat.Text);
                break;
            case PlayerInputMessage input:
                HandleInput(peer, input);
                break;
            case ShipChoiceRequestMessage choice:
                HandleShipChoice(peer, choice);
                break;
            case GoodbyeMessage:
                peer.Connection.Close();
                break;
            case HeartbeatMessage or TeamChangeRequestMessage:
                break;
            default:
                peer.Connection.Close();
                break;
        }
    }

    // The first message must be a Hello; then the players take open slots or the client is turned away.
    private void HandleJoin(RemotePeer peer, Message message)
    {
        if (message is HeartbeatMessage)
        {
            return;
        }

        RejectReason? reason = message switch
        {
            HelloMessage { Version: not ProtocolVersion.Current } => RejectReason.VersionMismatch,
            HelloMessage when World != null => RejectReason.GameInProgress,
            HelloMessage hello when !Lobby.TryJoin(peer.Id, hello.Nicknames) => RejectReason.LobbyFull,
            HelloMessage => null,
            _ => RejectReason.VersionMismatch,
        };
        if (reason is { } rejected)
        {
            _peers.Remove(peer.Id);
            Reject(peer.Connection, rejected);
            return;
        }

        peer.Nicknames = [.. Lobby.SlotsOf(peer.Id).Select(slot => Lobby.Slots[slot].Nickname)];
        peer.Connection.Send(new WelcomeMessage(peer.Id));
    }

    private void HandleInput(RemotePeer peer, PlayerInputMessage input)
    {
        if (World == null || input.Tick < peer.LastInputTick)
        {
            return;
        }

        peer.LastInputTick = input.Tick;
        foreach (var ship in input.Inputs)
        {
            if (ship.ShipIndex < _shipOwners.Length && _shipOwners[ship.ShipIndex] == peer.Id)
            {
                _inputs[ship.ShipIndex] = ship.Controls;
            }
        }
    }

    private void HandleShipChoice(RemotePeer peer, ShipChoiceRequestMessage choice)
    {
        if (World?.IsChoosingShip(choice.ShipIndex) == true && _shipOwners[choice.ShipIndex] == peer.Id)
        {
            World.ChooseShip(choice.ShipIndex, choice.ShipType);
        }
    }

    private void PostChat(string sender, string text)
    {
        var clean = ChatText.Sanitize(text);
        if (clean.Length == 0)
        {
            return;
        }

        _chat.Add(new ChatLine(sender, clean));
        Broadcast(new ChatMessage(sender, clean));
    }

    // In the lobby the client's slots open up; during a match its ships fly on under AI control (disconnect()).
    private void RemovePeer(RemotePeer peer)
    {
        _peers.Remove(peer.Id);
        if (!peer.HasJoined)
        {
            return;
        }

        if (World == null)
        {
            Lobby.RemoveClient(peer.Id);
            return;
        }

        Lobby.ConvertClientToAi(peer.Id);
        for (var i = 0; i < _shipOwners.Length; i++)
        {
            if (_shipOwners[i] == peer.Id)
            {
                _shipOwners[i] = LobbyState.NoOwner;
                World.Ships[i].IsHuman = false;
                _inputs.Remove(i);
            }
        }
    }

    private void BroadcastLobbyIfChanged()
    {
        if (Lobby.Version != _sentLobbyVersion)
        {
            _sentLobbyVersion = Lobby.Version;
            Broadcast(new LobbyUpdateMessage(Lobby));
        }
    }

    private void Broadcast(Message message)
    {
        var frame = MessageCodec.EncodeFrame(message);
        foreach (var peer in _peers.Values.Where(p => p.HasJoined))
        {
            peer.Connection.SendFrame(frame);
        }
    }
}
