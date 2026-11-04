using Saoss.Game.Rendering.Scene;
using Saoss.Network.Protocol;

namespace Saoss.Game.Settings;

/// <summary>
/// The player's options, a port of the original <c>Sets</c> class (sets.txt). Defaults are the values of the
/// original's shipped sets.txt, except the window: 1280x720 windowed instead of 1920x1080 fullscreen, so the
/// first start opens a window. The original's 256-color texture option is not ported.
/// </summary>
public sealed record GameSettings
{
    /// <summary>Fewest players in a local match.</summary>
    public const int MinPlayers = 2;

    /// <summary>Most players in a match [maxplayers].</summary>
    public const int MaxPlayers = 16;

    /// <summary>Local humans share one keyboard, which has four layouts; only the first four slots can be human.</summary>
    public const int MaxLocalHumans = 4;

    /// <summary>Highest team number [MAXNETPLAYERS - 1].</summary>
    public const int MaxTeam = MaxPlayers - 1;

    /// <summary>Lowest starting credits accepted from the settings file.</summary>
    public const int MinCredits = 20;

    /// <summary>Highest starting credits.</summary>
    public const int MaxCredits = 1000;

    /// <summary>Slowest game speed in simulation ticks per second.</summary>
    public const int MinTicksPerSecond = 50;

    /// <summary>Fastest game speed in simulation ticks per second.</summary>
    public const int MaxTicksPerSecond = 200;

    /// <summary>Loudest volume; volumes run from 0 to this.</summary>
    public const int MaxVolume = 100;

    /// <summary>Highest detail level [maxdetail].</summary>
    public const int MaxDetail = 3;

    /// <summary>Longest nickname; the network protocol sends ten characters.</summary>
    public const int MaxNicknameLength = Nickname.MaxLength;

    /// <summary>Most players one computer brings to a network game.</summary>
    public const int MaxNetworkPlayers = 4;

    private const int MinWidth = 320;
    private const int MinHeight = 240;
    private const int MaxWindowSize = 16384;

    /// <summary>Window or fullscreen width in pixels [width].</summary>
    public int Width { get; init; } = 1280;

    /// <summary>Window or fullscreen height in pixels [height].</summary>
    public int Height { get; init; } = 720;

    /// <summary>Borderless fullscreen instead of a window [fullscreen].</summary>
    public bool Fullscreen { get; init; }

    /// <summary>Players in a local match, <see cref="MinPlayers"/> to <see cref="MaxPlayers"/> [playermaara].</summary>
    public int PlayerCount { get; init; } = 3;

    /// <summary>Background music on [musics].</summary>
    public bool MusicEnabled { get; init; } = true;

    /// <summary>Sound effects on [efects].</summary>
    public bool SoundEffectsEnabled { get; init; } = true;

    /// <summary>Starting credits of a credits game and target score of the point modes [credits].</summary>
    public int Credits { get; init; } = 400;

    /// <summary>Game speed: simulation ticks per second, shown as "Fps" [fps].</summary>
    public int TicksPerSecond { get; init; } = 100;

    /// <summary>Music volume, 0 to <see cref="MaxVolume"/> [musicvolume].</summary>
    public int MusicVolume { get; init; } = 80;

    /// <summary>Sound effect volume, 0 to <see cref="MaxVolume"/> [soundvolume].</summary>
    public int SoundVolume { get; init; } = 80;

    /// <summary>All <see cref="MaxPlayers"/> player slots; a match uses the first <see cref="PlayerCount"/>.</summary>
    public IReadOnlyList<PlayerSlotSettings> Players { get; init; } = DefaultPlayers;

    /// <summary>
    /// An AI pilot with no enemy left does not turn on its teammates [aikohteet, "ai vs ai"].
    /// </summary>
    public bool AiTargetsOnlyEnemies { get; init; }

    /// <summary>Match camera [cammode].</summary>
    public CameraMode CameraMode { get; init; } = CameraMode.Tilted;

    /// <summary>Detail level 0 to <see cref="MaxDetail"/>: model smoothness and particle counts [detail].</summary>
    public int Detail { get; init; } = 2;

    /// <summary>Nicknames of up to four players on this computer, kept for network play [nick].</summary>
    public IReadOnlyList<string> Nicknames { get; init; } = DefaultNicknames;

    /// <summary>Host name or address joined last; replaces the original's lasthost.txt.</summary>
    public string LastHost { get; init; } = DefaultHost;

    /// <summary>Players from this computer in a network game, 1 to <see cref="MaxNetworkPlayers"/> [hosteja, joinaavia].</summary>
    public int NetworkPlayerCount { get; init; } = 1;

    private static string DefaultHost => "localhost";

    // Teams of the original's sets.txt (stored there one higher); only player 1 is human.
    private static IReadOnlyList<PlayerSlotSettings> DefaultPlayers { get; } =
        [.. new[] { 0, 2, 2, 2, 4, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0 }.Select((team, i) => new PlayerSlotSettings(IsHuman: i == 0, team))];

    private static IReadOnlyList<string> DefaultNicknames { get; } = ["Player 1", "Player 2", "Player 3", "Player 4"];

    /// <summary>
    /// Returns a copy with every value brought into its valid range, for settings read from a file.
    /// </summary>
    public GameSettings Validated() => this with
    {
        Width = Math.Clamp(Width, MinWidth, MaxWindowSize),
        Height = Math.Clamp(Height, MinHeight, MaxWindowSize),
        PlayerCount = Math.Clamp(PlayerCount, MinPlayers, MaxPlayers),
        Credits = Math.Clamp(Credits, MinCredits, MaxCredits),
        TicksPerSecond = Math.Clamp(TicksPerSecond, MinTicksPerSecond, MaxTicksPerSecond),
        MusicVolume = Math.Clamp(MusicVolume, 0, MaxVolume),
        SoundVolume = Math.Clamp(SoundVolume, 0, MaxVolume),
        Players = ValidatedPlayers(Players),
        CameraMode = Enum.IsDefined(CameraMode) ? CameraMode : CameraMode.Tilted,
        Detail = Math.Clamp(Detail, 0, MaxDetail),
        Nicknames = ValidatedNicknames(Nicknames),
        LastHost = LastHost is { } host && HostAddress.IsValid(host) ? host : DefaultHost,
        NetworkPlayerCount = Math.Clamp(NetworkPlayerCount, 1, MaxNetworkPlayers),
    };

    // Exactly MaxPlayers slots; only the first MaxLocalHumans may be human, like the original player setup.
    private static PlayerSlotSettings[] ValidatedPlayers(IReadOnlyList<PlayerSlotSettings?>? players) =>
    [
        .. Enumerable.Range(0, MaxPlayers).Select(i =>
        {
            var slot = players?.ElementAtOrDefault(i) ?? DefaultPlayers[i];
            return new PlayerSlotSettings(slot.IsHuman && i < MaxLocalHumans, Math.Clamp(slot.Team, 0, MaxTeam));
        }),
    ];

    // Characters the network refuses are dropped and long names cut; a name left blank falls back to the default.
    private static string[] ValidatedNicknames(IReadOnlyList<string?>? nicknames) =>
    [
        .. DefaultNicknames.Select((fallback, i) =>
        {
            var kept = new string([.. (nicknames?.ElementAtOrDefault(i) ?? string.Empty).Where(Nickname.IsAllowed).Take(Nickname.MaxLength)]);
            return string.IsNullOrWhiteSpace(kept) ? fallback : kept;
        }),
    ];
}
