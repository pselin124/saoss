namespace Saoss.Core.Simulation.Ai;

/// <summary>
/// How well every computer pilot of a match flies and shoots; not in the original, which only had
/// <see cref="GalacticTerror"/>. The handicaps of each level are in <see cref="AiSkill"/>.
/// </summary>
public enum AiDifficulty
{
    /// <summary>Easiest: passive, inaccurate and slow to read the target's movement.</summary>
    SpaceCadet,

    /// <summary>Between the two others.</summary>
    RocketAce,

    /// <summary>The original computer pilot, unchanged.</summary>
    GalacticTerror,
}
