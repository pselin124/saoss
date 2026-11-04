namespace Saoss.Core.Simulation.Ai;

/// <summary>
/// Per-ship memory of the handicapped computer pilot (<see cref="AiSkill"/>); not in the original. The original pilot
/// (<see cref="AiSkill.IsFlawless"/>) never touches it.
/// </summary>
public sealed class AiState
{
    /// <summary>Factor applied to every distance threshold this period.</summary>
    public float DistanceScale { get; set; } = 1;

    /// <summary>Ticks until <see cref="DistanceScale"/> is rolled again.</summary>
    public int DistanceTicksLeft { get; set; }

    /// <summary>Error in degrees added to the aim this period.</summary>
    public float AimErrorDegrees { get; set; }

    /// <summary>Ticks until <see cref="AimErrorDegrees"/> is rolled again.</summary>
    public int AimTicksLeft { get; set; }

    /// <summary>The pilot is in a passive mood: it neither attacks nor shoots on purpose.</summary>
    public bool IsHoldingBack { get; set; }

    /// <summary>Ticks until the mood is rolled again.</summary>
    public int MoodTicksLeft { get; set; }

    /// <summary>Lagging estimate of the target's velocity, used to lead shots.</summary>
    public float TargetVelocityX { get; set; }

    /// <inheritdoc cref="TargetVelocityX"/>
    public float TargetVelocityY { get; set; }
}
