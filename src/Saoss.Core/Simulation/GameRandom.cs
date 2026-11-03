namespace Saoss.Core.Simulation;

/// <summary>
/// Deterministic random source for the simulation, so a match can be replayed from its seed.
/// </summary>
public sealed class GameRandom(int seed)
{
    private readonly Random _random = new(seed);

    /// <summary>Uniform value in [0, <paramref name="max"/>); the original <c>random(x)</c>.</summary>
    public float Next(float max) => (float)(_random.NextDouble() * max);

    /// <summary>Uniform value in [<paramref name="min"/>, <paramref name="max"/>).</summary>
    public float Range(float min, float max) => min + Next(max - min);

    /// <summary>Uniform integer in [0, <paramref name="maxExclusive"/>).</summary>
    public int NextInt(int maxExclusive) => _random.Next(maxExclusive);
}
