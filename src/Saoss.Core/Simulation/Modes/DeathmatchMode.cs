namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// Deathmatch, with or without credits. Without credits the match never ends; with credits it ends when fewer
/// than two teams still have a player in the game (localgame.cpp, menu entries 2 and 3).
/// </summary>
public sealed class DeathmatchMode : GameModeRules
{
    /// <inheritdoc/>
    public override bool IsOver(World world) =>
        world.Settings.UsesCredits && world.Ships.Where(s => s.IsInGame).Select(s => s.Team).Distinct().Count() < 2;
}
