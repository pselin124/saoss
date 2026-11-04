namespace Saoss.Core.Simulation.Ai;

/// <summary>
/// Handicaps of one <see cref="AiDifficulty"/> level; all tuning of the easier levels is here. Periods are
/// averages: each re-roll waits a random time between half and one and a half of them, so ships drift apart.
/// </summary>
public sealed record AiSkill
{
    /// <summary>The easiest level.</summary>
    public static AiSkill SpaceCadet { get; } = new()
    {
        HoldBackChance = 0.5,
        MoodTicks = 100,
        WildFireChance = 0.02,
        MaxAimErrorDegrees = 30,
        AimTicks = 80,
        DistanceJitter = 0.4,
        DistanceTicks = 300,
        LeadFactor = 0.3,
        VelocityTracking = 0.03,
    };

    /// <summary>The middle level.</summary>
    public static AiSkill RocketAce { get; } = new()
    {
        HoldBackChance = 0.3,
        MoodTicks = 100,
        WildFireChance = 0.008,
        MaxAimErrorDegrees = 15,
        AimTicks = 80,
        DistanceJitter = 0.25,
        DistanceTicks = 300,
        LeadFactor = 0.6,
        VelocityTracking = 0.1,
    };

    /// <summary>The original pilot: no handicaps and no extra random numbers.</summary>
    public static AiSkill GalacticTerror { get; } = new() { IsFlawless = true, LeadFactor = 1, VelocityTracking = 1 };

    /// <summary>
    /// True for the original pilot; the handicaps below are then skipped entirely, so the match plays exactly as
    /// before levels existed.
    /// </summary>
    public bool IsFlawless { get; init; }

    /// <summary>
    /// Chance that a mood period is passive: no attacking thrust, no fire and no special (except a boost away from
    /// the planet). Rolled every <see cref="MoodTicks"/>.
    /// </summary>
    public double HoldBackChance { get; init; }

    /// <summary>Average ticks between mood rolls.</summary>
    public int MoodTicks { get; init; }

    /// <summary>Chance per tick to pull the trigger although no shot was planned.</summary>
    public double WildFireChance { get; init; }

    /// <summary>
    /// Largest error in degrees added to every aim direction (lead and straight at the target), so the pilot also
    /// fires when "aimed" the wrong way. Rolled uniformly every <see cref="AimTicks"/>.
    /// </summary>
    public double MaxAimErrorDegrees { get; init; }

    /// <summary>Average ticks between aim error rolls.</summary>
    public int AimTicks { get; init; }

    /// <summary>
    /// Relative spread of every distance threshold (weapon range, chase and keep-away distances): each ship scales
    /// them all by a factor in [1 - jitter, 1 + jitter], rolled every <see cref="DistanceTicks"/>.
    /// </summary>
    public double DistanceJitter { get; init; }

    /// <summary>Average ticks between distance threshold rolls.</summary>
    public int DistanceTicks { get; init; }

    /// <summary>Share of the estimated target velocity used to lead shots; 1 leads fully.</summary>
    public double LeadFactor { get; init; }

    /// <summary>
    /// How fast the velocity estimate follows the target's real velocity, per tick (1 = at once), so the lead
    /// lags behind turns and target switches.
    /// </summary>
    public double VelocityTracking { get; init; }

    /// <summary>The handicaps of <paramref name="difficulty"/>.</summary>
    public static AiSkill For(AiDifficulty difficulty) => difficulty switch
    {
        AiDifficulty.SpaceCadet => SpaceCadet,
        AiDifficulty.RocketAce => RocketAce,
        _ => GalacticTerror,
    };
}
