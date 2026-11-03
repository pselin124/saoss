namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Visual-only state of one player slot that the original kept in <c>Player</c>: exhaust trails and the
/// respawn sparks. Survives respawns like the original.
/// </summary>
public sealed class ShipVisualState
{
    /// <summary>Puffs per exhaust trail [kaasumaara].</summary>
    public const int TrailLength = 5;

    /// <summary>Creates the state with room for <paramref name="spawnSparkCount"/> respawn sparks.</summary>
    public ShipVisualState(int spawnSparkCount)
    {
        SpawnSparks = [.. Enumerable.Range(0, spawnSparkCount).Select(_ => new Particle())];
    }

    /// <summary>Main exhaust trail, newest puff first [kaasujalki].</summary>
    public ExhaustPuff[] Trail { get; } = [.. Enumerable.Range(0, TrailLength).Select(_ => new ExhaustPuff())];

    /// <summary>Second exhaust trail of twin-engine ships [kaasujalki2].</summary>
    public ExhaustPuff[] SecondTrail { get; } = [.. Enumerable.Range(0, TrailLength).Select(_ => new ExhaustPuff())];

    /// <summary>Ticks until the trails advance [kaasujalkiaika].</summary>
    public int TrailTimer { get; set; }

    /// <summary>Sparks that swirl in toward the respawn point; positions are relative to the ship [alkusirpale].</summary>
    public Particle[] SpawnSparks { get; }

    /// <summary>The ship's death timer after the previous tick, i.e. the value the original saw at the start of a tick.</summary>
    public int PreviousDeathTimer { get; set; }
}
