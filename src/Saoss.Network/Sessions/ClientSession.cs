using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Snapshots;
using Saoss.Network.Lobby;
using Saoss.Network.Protocol;
using Saoss.Network.Transport;

namespace Saoss.Network.Sessions;

/// <summary>
/// The joining side of a network game (net.cpp joinMenu, netpeli.cpp netJoinPeli). Connects, sends the local
/// players, follows the lobby and mirrors the host's match: <see cref="World"/> is overwritten by every snapshot
/// and never stepped here. Call <see cref="Update"/> every frame from the game thread; all methods are
/// non-blocking.
/// </summary>
public sealed class ClientSession : IDisposable
{
    private readonly List<ReceivedTick> _receivedTicks = [];
    private readonly ChatLog _chat = new();
    private readonly IReadOnlyList<string> _nicknames;
    private Task<TcpConnection>? _connecting;
    private TcpConnection? _connection;

    private ClientSession(Task<TcpConnection> connecting, IReadOnlyList<string> nicknames)
    {
        _connecting = connecting;
        _nicknames = nicknames;
    }

    /// <summary>Where the session is.</summary>
    public ClientState State { get; private set; } = ClientState.Connecting;

    /// <summary>Why the session ended, once <see cref="State"/> is <see cref="ClientState.Disconnected"/>.</summary>
    public DisconnectReason? DisconnectReason { get; private set; }

    /// <summary>The host's reason when <see cref="DisconnectReason"/> is <see cref="Sessions.DisconnectReason.Rejected"/>.</summary>
    public RejectReason? RejectReason { get; private set; }

    /// <summary>Id the host gave this client; its lobby slots carry it as owner.</summary>
    public int ClientId { get; private set; } = LobbyState.NoOwner;

    /// <summary>Latest lobby from the host, or null before the first one.</summary>
    public LobbyState? Lobby { get; private set; }

    /// <summary>The mirrored match, or the last one after returning to the lobby; null before the first match.</summary>
    public World? World { get; private set; }

    /// <summary>Lobby slot of every ship of the current match.</summary>
    public IReadOnlyList<int> ShipSlots { get; private set; } = [];

    /// <summary>Ships of the current match played on this machine, in ship order.</summary>
    public IReadOnlyList<int> LocalShips { get; private set; } = [];

    /// <summary>Recent chat lines, oldest first.</summary>
    public IReadOnlyList<ChatLine> ChatLog => _chat.Lines;

    /// <summary>
    /// Host ticks applied during the last <see cref="Update"/>, in order, each with the events to play for it.
    /// <see cref="World"/> already shows the state after the last of them.
    /// </summary>
    public IReadOnlyList<ReceivedTick> ReceivedTicks => _receivedTicks;

    /// <summary>Starts connecting to a host with one to four local players.</summary>
    public static ClientSession Connect(string host, int port, IReadOnlyList<string> nicknames, ConnectionOptions? options = null, TimeSpan? connectTimeout = null)
    {
        if (nicknames.Count is 0 or > LobbyState.MaxLocalPlayers)
        {
            throw new ArgumentOutOfRangeException(nameof(nicknames), nicknames.Count, "One to four local players are needed.");
        }

        var connecting = NetClient.ConnectAsync(host, port, connectTimeout ?? NetClient.DefaultConnectTimeout, options);
        return new ClientSession(connecting, [.. nicknames.Select(Nickname.Sanitize)]);
    }

    /// <summary>Finishes connecting, handles everything the host sent and notices a lost connection.</summary>
    public void Update()
    {
        _receivedTicks.Clear();
        if (State == ClientState.Connecting)
        {
            FinishConnecting();
        }

        if (_connection == null || State == ClientState.Disconnected)
        {
            return;
        }

        _connection.Update();
        while (State != ClientState.Disconnected && _connection.TryReceive(out var message))
        {
            Handle(message);
        }

        if (State != ClientState.Disconnected && _connection.IsClosed)
        {
            Disconnect(_connection.CloseReason == CloseReason.ProtocolViolation ? Sessions.DisconnectReason.ProtocolError : Sessions.DisconnectReason.ConnectionLost);
        }
    }

    /// <summary>Sends a chat line; control characters are removed and the text is cut to 500 characters.</summary>
    public void SendChat(string text)
    {
        var clean = ChatText.Sanitize(text);
        if (clean.Length > 0)
        {
            Send(new ChatMessage(string.Empty, clean));
        }
    }

    /// <summary>Asks the host to put one of this client's slots on a team.</summary>
    public void RequestTeam(int slot, int team) => Send(new TeamChangeRequestMessage(slot, team));

    /// <summary>Sends the buttons of the local ships for the current tick.</summary>
    public void SendInputs(IReadOnlyList<ShipInput> inputs)
    {
        if (State == ClientState.InMatch && World != null)
        {
            Send(new PlayerInputMessage(World.Tick, inputs));
        }
    }

    /// <summary>Buys a ship for a local pending choice; null buys a random affordable one.</summary>
    public void ChooseShip(int shipIndex, ShipType? type) => Send(new ShipChoiceRequestMessage(shipIndex, type));

    /// <summary>Says goodbye and closes the connection.</summary>
    public void Leave()
    {
        if (State == ClientState.Disconnected)
        {
            return;
        }

        Send(new GoodbyeMessage());
        _connection?.CloseAfterSending();
        Disconnect(Sessions.DisconnectReason.Left);
    }

    /// <inheritdoc/>
    public void Dispose() => Leave();

    private void FinishConnecting()
    {
        if (_connecting is not { IsCompleted: true } connecting)
        {
            return;
        }

        _connecting = null;
        if (!connecting.IsCompletedSuccessfully)
        {
            _ = connecting.Exception;
            Disconnect(Sessions.DisconnectReason.ConnectFailed);
            return;
        }

        _connection = connecting.Result;
        _connection.Send(new HelloMessage(ProtocolVersion.Current, _nicknames));
        State = ClientState.Joining;
    }

    private void Handle(Message message)
    {
        switch (message)
        {
            case WelcomeMessage welcome when State == ClientState.Joining:
                ClientId = welcome.ClientId;
                State = ClientState.Lobby;
                break;
            case RejectMessage reject:
                RejectReason = reject.Reason;
                Disconnect(Sessions.DisconnectReason.Rejected);
                break;
            case LobbyUpdateMessage lobby:
                Lobby = lobby.Lobby;
                break;
            case ChatMessage chat:
                _chat.Add(new ChatLine(chat.Sender, chat.Text));
                break;
            case MatchStartMessage start:
                StartMatch(start);
                break;
            case WorldUpdateMessage update when State == ClientState.InMatch:
                ApplyUpdate(update);
                break;
            case ReturnToLobbyMessage when State == ClientState.InMatch:
                State = ClientState.Lobby;
                break;
            case GoodbyeMessage:
                Disconnect(Sessions.DisconnectReason.HostLeft);
                break;
        }
    }

    private void StartMatch(MatchStartMessage start)
    {
        if (start.ShipSlots.Count != start.Settings.Players.Count)
        {
            Disconnect(Sessions.DisconnectReason.ProtocolError);
            return;
        }

        World = new World(start.Settings, start.Seed);
        ShipSlots = start.ShipSlots;
        LocalShips = [.. Enumerable.Range(0, ShipSlots.Count).Where(i => Lobby?.Slots[ShipSlots[i]] is { Status: SlotStatus.Human } slot && slot.OwnerId == ClientId)];
        State = ClientState.InMatch;
    }

    private void ApplyUpdate(WorldUpdateMessage update)
    {
        try
        {
            WorldSnapshot.Apply(World!, update.Snapshot);
        }
        catch (SnapshotFormatException)
        {
            Disconnect(Sessions.DisconnectReason.ProtocolError);
            return;
        }

        var events = World!.Events;
        var newEvents = update.FirstNewEvent < events.Count ? events.Skip(update.FirstNewEvent).ToList() : [];
        _receivedTicks.Add(new ReceivedTick(World.Tick, newEvents));
    }

    private void Send(Message message)
    {
        if (State != ClientState.Disconnected)
        {
            _connection?.Send(message);
        }
    }

    private void Disconnect(DisconnectReason reason)
    {
        if (State == ClientState.Disconnected)
        {
            return;
        }

        State = ClientState.Disconnected;
        DisconnectReason = reason;
        _connecting?.ContinueWith(task => task.Result.Close(), TaskContinuationOptions.OnlyOnRanToCompletion);
        _connecting = null;
        if (reason != Sessions.DisconnectReason.Left)
        {
            _connection?.Close();
        }
    }
}
