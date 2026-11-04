using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Network.Protocol;

namespace Saoss.Network.Lobby;

/// <summary>
/// The network lobby (net.cpp hostMenu, hostSettingsMenu, paivitaPelaajat): match settings and 16 slots that are
/// open, closed, AI or human. Pure logic; the host owns the authoritative copy and clients get copies without the
/// member list. Every change increments <see cref="Version"/>.
/// Human players are always packed into the first slots that are open or human, the host's players first and then
/// each client's in client id order (paivitaPelaajat), so slots can move when somebody leaves; teams stay with
/// the slot.
/// </summary>
public sealed class LobbyState
{
    /// <summary>Number of slots [MAXNETPLAYERS].</summary>
    public const int SlotCount = GameConstants.MaxPlayers;

    /// <summary>Fewest slots the host can set [playerslots].</summary>
    public const int MinPlayers = 2;

    /// <summary>Most players on one machine.</summary>
    public const int MaxLocalPlayers = 4;

    /// <summary>Owner id of the host's own players.</summary>
    public const int HostId = 0;

    /// <summary>Owner id of slots without a human.</summary>
    public const int NoOwner = -1;

    /// <summary>Lowest credits setting [mpcredits].</summary>
    public const int MinCredits = 25;

    /// <summary>Highest credits setting [MAX_MP_CREDITS].</summary>
    public const int MaxCredits = 9000;

    /// <summary>Credits of a new lobby [mpcredits = 250].</summary>
    public const int DefaultCredits = 250;

    private const int HostTeam = 0;
    private const int OtherTeam = 1;
    private const int SmallCreditStep = 25;
    private const int LargeCreditStep = 100;
    private const int LargeStepFrom = 1000;

    private readonly LobbySlot[] _slots;
    private readonly SortedDictionary<int, IReadOnlyList<string>> _members = [];

    /// <summary>
    /// Creates the host's lobby: the host's players in the first slots on team 1, every other slot open on team 2
    /// (hostMenu). Nicknames are sanitized.
    /// </summary>
    public LobbyState(IReadOnlyList<string> hostNicknames)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hostNicknames.Count, MaxLocalPlayers, nameof(hostNicknames));
        _slots = [.. Enumerable.Range(0, SlotCount).Select(i => new LobbySlot(SlotStatus.Open, i < hostNicknames.Count ? HostTeam : OtherTeam, string.Empty, NoOwner))];
        _members[HostId] = [.. hostNicknames.Select(Nickname.Sanitize)];
        Repack(SlotCount);
    }

    /// <summary>Creates a copy received from the host; it has no member list.</summary>
    internal LobbyState(int maxPlayers, GameMode mode, int credits, bool aiTargetsOnlyEnemies, LobbySlot[] slots)
    {
        MaxPlayers = maxPlayers;
        Mode = mode;
        Credits = credits;
        AiTargetsOnlyEnemies = aiTargetsOnlyEnemies;
        _slots = slots;
    }

    /// <summary>Slots in use, 2-16 [playerslots, "maximum players"].</summary>
    public int MaxPlayers { get; private set; } = SlotCount;

    /// <summary>Game mode; the original's default was deathmatch with random ships [gamemode].</summary>
    public GameMode Mode { get; private set; } = GameMode.RandomShipsDeathmatch;

    /// <summary>Starting credits of the deathmatches, target score of the point modes ["credits/limit"].</summary>
    public int Credits { get; private set; } = DefaultCredits;

    /// <summary>See <see cref="MatchSettings.AiTargetsOnlyEnemies"/>; the host's own setting [aikohteet].</summary>
    public bool AiTargetsOnlyEnemies { get; private set; }

    /// <summary>All 16 slots; only the first <see cref="MaxPlayers"/> are used.</summary>
    public IReadOnlyList<LobbySlot> Slots => _slots;

    /// <summary>Incremented on every change.</summary>
    public int Version { get; private set; }

    /// <summary>Open slots among the used ones.</summary>
    public int OpenSlotCount => _slots.Take(MaxPlayers).Count(s => s.Status == SlotStatus.Open);

    /// <summary>
    /// Adds a client's players to the open slots. Fails without changes when there are fewer open slots than
    /// players; the original then answered "pelaajia liikaa!" and disconnected.
    /// </summary>
    public bool TryJoin(int clientId, IReadOnlyList<string> nicknames)
    {
        if (clientId <= HostId || _members.ContainsKey(clientId) || nicknames.Count is 0 or > MaxLocalPlayers || nicknames.Count > OpenSlotCount)
        {
            return false;
        }

        _members[clientId] = [.. nicknames.Select(Nickname.Sanitize)];
        Repack(MaxPlayers);
        Changed();
        return true;
    }

    /// <summary>A client left the lobby: its slots open up and later players move up (paivitaPelaajat).</summary>
    public void RemoveClient(int clientId)
    {
        if (clientId != HostId && _members.Remove(clientId))
        {
            Repack(MaxPlayers);
            Changed();
        }
    }

    /// <summary>
    /// A client left during a match: its slots become AI so its ships fly on (disconnect sets mpstatus 2 and
    /// clears ihmispelaaja). Returns the slots that changed.
    /// </summary>
    public IReadOnlyList<int> ConvertClientToAi(int clientId)
    {
        var slots = SlotsOf(clientId);
        if (clientId == HostId || !_members.Remove(clientId))
        {
            return [];
        }

        foreach (var slot in slots)
        {
            _slots[slot] = _slots[slot] with { Status = SlotStatus.Ai, Nickname = string.Empty, OwnerId = NoOwner };
        }

        Changed();
        return slots;
    }

    /// <summary>Slots of the given owner.</summary>
    public IReadOnlyList<int> SlotsOf(int ownerId) =>
        [.. Enumerable.Range(0, SlotCount).Where(i => _slots[i].Status == SlotStatus.Human && _slots[i].OwnerId == ownerId)];

    /// <summary>
    /// Host: cycles a slot open, closed, AI, open. Human slots cannot be changed, so a player can only be removed by
    /// leaving.
    /// </summary>
    public void CycleStatus(int slot)
    {
        if (slot < 0 || slot >= MaxPlayers || _slots[slot].Status == SlotStatus.Human)
        {
            return;
        }

        var next = _slots[slot].Status switch
        {
            SlotStatus.Open => SlotStatus.Closed,
            SlotStatus.Closed => SlotStatus.Ai,
            _ => SlotStatus.Open,
        };
        _slots[slot] = _slots[slot] with { Status = next };
        Repack(MaxPlayers);
        Changed();
    }

    /// <summary>Host: puts any slot on a team (the original cycled 1-16 with the arrow keys).</summary>
    public void SetTeam(int slot, int team)
    {
        if (slot is >= 0 and < SlotCount && IsTeam(team) && _slots[slot].Team != team)
        {
            _slots[slot] = _slots[slot] with { Team = team };
            Changed();
        }
    }

    /// <summary>Client: puts one of its own slots on a team; returns false for anybody else's slot.</summary>
    public bool TrySetTeam(int clientId, int slot, int team)
    {
        if (slot is < 0 or >= SlotCount || !IsTeam(team) || _slots[slot].Status != SlotStatus.Human || _slots[slot].OwnerId != clientId)
        {
            return false;
        }

        SetTeam(slot, team);
        return true;
    }

    /// <summary>Host: changes the number of slots in use; refused when the human players would no longer fit.</summary>
    public bool TrySetMaxPlayers(int maxPlayers)
    {
        if (maxPlayers is < MinPlayers or > SlotCount)
        {
            return false;
        }

        var before = (LobbySlot[])_slots.Clone();
        if (!Repack(maxPlayers))
        {
            before.CopyTo(_slots, 0);
            return false;
        }

        MaxPlayers = maxPlayers;
        Changed();
        return true;
    }

    /// <summary>Host: sets the game mode.</summary>
    public void SetMode(GameMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown game mode.");
        }

        Mode = mode;
        Changed();
    }

    /// <summary>Host: sets the credits, clamped to <see cref="MinCredits"/>-<see cref="MaxCredits"/>.</summary>
    public void SetCredits(int credits)
    {
        Credits = Math.Clamp(credits, MinCredits, MaxCredits);
        Changed();
    }

    /// <summary>
    /// Host: the original's credits arrows: steps of 25 below 1000 and 100 above, wrapping around at the ends
    /// (hostSettingsMenu).
    /// </summary>
    public void StepCredits(bool up)
    {
        int next;
        if (up)
        {
            next = Credits < LargeStepFrom ? Credits + SmallCreditStep : Credits < MaxCredits ? Credits + LargeCreditStep : MinCredits;
        }
        else
        {
            next = Credits > LargeStepFrom ? Credits - LargeCreditStep : Credits > MinCredits ? Credits - SmallCreditStep : MaxCredits;
        }

        SetCredits(next);
    }

    /// <summary>Host: sets <see cref="AiTargetsOnlyEnemies"/>.</summary>
    public void SetAiTargetsOnlyEnemies(bool value)
    {
        AiTargetsOnlyEnemies = value;
        Changed();
    }

    /// <summary>Slot of every ship: the used slots with a human or AI, in slot order (hostStart).</summary>
    public IReadOnlyList<int> PlayerSlots() => [.. Enumerable.Range(0, MaxPlayers).Where(i => _slots[i].IsPlayer)];

    /// <summary>
    /// Settings of a match from this lobby. Both deathmatches were credits games in network play: random ships
    /// buy random ships, plain deathmatch lets players choose (netpeli.cpp netHostPeli); the point modes use the
    /// credits as the target score.
    /// </summary>
    public MatchSettings ToMatchSettings()
    {
        var players = PlayerSlots().Select(i => new PlayerSetup(_slots[i].Team, _slots[i].Status == SlotStatus.Human)).ToList();
        if (players.Count == 0)
        {
            throw new InvalidOperationException("The lobby has no players.");
        }

        var deathmatch = Mode is GameMode.Deathmatch or GameMode.RandomShipsDeathmatch;
        return new MatchSettings
        {
            Players = players,
            Mode = Mode,
            Credits = Credits,
            UsesCredits = deathmatch,
            PlayersChooseShips = Mode == GameMode.Deathmatch,
            AiTargetsOnlyEnemies = AiTargetsOnlyEnemies,
        };
    }

    private static bool IsTeam(int team) => team is >= 0 and < SlotCount;

    // paivitaPelaajat: humans fill the slots that are open or human in member order, skipping closed and AI
    // slots; human slots left over become open. Returns false if some player found no slot.
    private bool Repack(int maxPlayers)
    {
        var next = 0;
        foreach (var (owner, nicknames) in _members)
        {
            foreach (var nickname in nicknames)
            {
                while (next < maxPlayers && _slots[next].Status is SlotStatus.Closed or SlotStatus.Ai)
                {
                    next++;
                }

                if (next >= maxPlayers)
                {
                    return false;
                }

                _slots[next] = _slots[next] with { Status = SlotStatus.Human, Nickname = nickname, OwnerId = owner };
                next++;
            }
        }

        for (var i = next; i < SlotCount; i++)
        {
            if (_slots[i].Status == SlotStatus.Human)
            {
                _slots[i] = _slots[i] with { Status = SlotStatus.Open, Nickname = string.Empty, OwnerId = NoOwner };
            }
        }

        return true;
    }

    private void Changed() => Version++;
}
