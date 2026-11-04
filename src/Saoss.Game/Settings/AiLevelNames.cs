using Saoss.Core.Simulation.Ai;

namespace Saoss.Game.Settings;

/// <summary>Names of the AI levels as the menus show them.</summary>
public static class AiLevelNames
{
    /// <summary>Display name of <paramref name="difficulty"/>.</summary>
    public static string Of(AiDifficulty difficulty) => difficulty switch
    {
        AiDifficulty.SpaceCadet => "Space Cadet",
        AiDifficulty.RocketAce => "Rocket Ace",
        _ => "Galactic Terror",
    };
}
