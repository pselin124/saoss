namespace Saoss.Core.Simulation.Ai;

/// <summary>
/// Keeps a handicapped pilot's <see cref="AiState"/> up to date once per tick: re-rolls the distance scale, the aim
/// error and the mood when their time is up, and moves the velocity estimate toward the target's real velocity.
/// All randomness comes from the world's generator, so matches stay reproducible.
/// </summary>
internal static class AiHandicap
{
    /// <summary>Advances the state of a pilot with <paramref name="skill"/> by one tick.</summary>
    public static void Update(AiState state, AiSkill skill, Ship? target, GameRandom random)
    {
        state.DistanceTicksLeft--;
        if (state.DistanceTicksLeft <= 0)
        {
            var jitter = (float)skill.DistanceJitter;
            state.DistanceScale = 1 + random.Range(-jitter, jitter);
            state.DistanceTicksLeft = NextPeriod(skill.DistanceTicks, random);
        }

        state.AimTicksLeft--;
        if (state.AimTicksLeft <= 0)
        {
            var maxError = (float)skill.MaxAimErrorDegrees;
            state.AimErrorDegrees = random.Range(-maxError, maxError);
            state.AimTicksLeft = NextPeriod(skill.AimTicks, random);
        }

        state.MoodTicksLeft--;
        if (state.MoodTicksLeft <= 0)
        {
            state.IsHoldingBack = random.Next(1f) < skill.HoldBackChance;
            state.MoodTicksLeft = NextPeriod(skill.MoodTicks, random);
        }

        if (target is not null)
        {
            state.TargetVelocityX += (float)(skill.VelocityTracking * (target.VelocityX - state.TargetVelocityX));
            state.TargetVelocityY += (float)(skill.VelocityTracking * (target.VelocityY - state.TargetVelocityY));
        }
    }

    /// <summary>Whether to pull the trigger this tick without a planned shot.</summary>
    public static bool FiresWildly(AiSkill skill, GameRandom random) => random.Next(1f) < skill.WildFireChance;

    // Between half and one and a half times the average.
    private static int NextPeriod(int averageTicks, GameRandom random) => (averageTicks / 2) + random.NextInt(averageTicks) + 1;
}
