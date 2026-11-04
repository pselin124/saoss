using Saoss.Core.Simulation.Ai;

namespace Saoss.Core.Simulation;

/// <summary>
/// One player slot at match start.
/// </summary>
/// <param name="Team">Team number; players on the same team do not hurt each other with area weapons.</param>
/// <param name="IsHuman">Controlled by a person rather than the AI.</param>
public sealed record PlayerSetup(int Team, bool IsHuman);

/// <summary>
/// Everything needed to start a match.
/// </summary>
public sealed record MatchSettings
{
    /// <summary>Default <see cref="Credits"/>, the "credits" value of the original's default settings file.</summary>
    public const int DefaultCredits = 400;

    /// <summary>Player slots; the list index is the ship index.</summary>
    public required IReadOnlyList<PlayerSetup> Players { get; init; }

    /// <summary>Game mode.</summary>
    public GameMode Mode { get; init; } = GameMode.Deathmatch;

    /// <summary>
    /// When false, an AI pilot with no enemy left attacks the nearest other ship of any team [aikohteet].
    /// </summary>
    public bool AiTargetsOnlyEnemies { get; init; }

    /// <summary>Skill of every computer pilot; the default is the original pilot.</summary>
    public AiDifficulty AiDifficulty { get; init; } = AiDifficulty.GalacticTerror;

    /// <summary>
    /// Credits game: every player starts with <see cref="Credits"/>, pays for each ship and is out when it can no
    /// longer afford one; the last team with players left wins [pistepeli].
    /// </summary>
    public bool UsesCredits { get; init; }

    /// <summary>In a credits game, players pick each ship from the purchase menu instead of buying a random one [saavalita].</summary>
    public bool PlayersChooseShips { get; init; }

    /// <summary>Starting credits of a credits game, and the target score of the point modes [fullcredits].</summary>
    public int Credits { get; init; } = DefaultCredits;

    /// <summary>
    /// A human buying a ship pauses the match until the purchase (local games). When false the match goes on and
    /// the buyer stays out until the purchase, as in network games, where netValitseAlus kept the frames running.
    /// </summary>
    public bool ChoicesPauseMatch { get; init; } = true;

    /// <summary>
    /// Settings of a new game started from the local game menu (localgame.cpp onePlayerGameMenu).
    /// </summary>
    public static MatchSettings ForLocalMenu(LocalGameKind kind, IReadOnlyList<PlayerSetup> players, int credits = DefaultCredits) =>
        new()
        {
            Players = players,
            Credits = credits,
            Mode = kind switch
            {
                LocalGameKind.RandomShips or LocalGameKind.CreditsRandomShips => GameMode.RandomShipsDeathmatch,
                LocalGameKind.CreditsChooseShips => GameMode.Deathmatch,
                LocalGameKind.FlowerDomination => GameMode.FlowerDomination,
                LocalGameKind.CaptureTheFlower => GameMode.CaptureTheFlower,
                LocalGameKind.EatTheMushroom => GameMode.EatTheMushroom,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown local game kind."),
            },
            UsesCredits = kind is LocalGameKind.CreditsChooseShips or LocalGameKind.CreditsRandomShips,
            PlayersChooseShips = kind == LocalGameKind.CreditsChooseShips,
        };
}
