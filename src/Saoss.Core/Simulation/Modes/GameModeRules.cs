namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// The rules that differ between game modes, called by <see cref="World"/> at the same points as the original
/// <c>calculateFrame</c>: when a destroyed ship reaches the respawn point, at the end of every tick
/// (the laskeXPisteet functions), and after every tick to see whether the match is over.
/// Mode state such as control points or flowers lives in the subclass and is read by the renderer.
/// </summary>
public abstract class GameModeRules
{
    /// <summary>Creates the rules for the settings' mode.</summary>
    public static GameModeRules Create(MatchSettings settings) => settings.Mode switch
    {
        GameMode.FlowerDomination => new DominationMode(),
        GameMode.CaptureTheFlower => new CaptureTheFlowerMode(settings.Players.Select(p => p.Team)),
        GameMode.EatTheMushroom => new MushroomMode(),
        _ => new DeathmatchMode(),
    };

    /// <summary>
    /// A destroyed ship's timer reached the respawn point, before it gets its new ship. Applies death penalties.
    /// </summary>
    public virtual void OnRespawnPoint(World world, Ship ship)
    {
    }

    /// <summary>Scores the tick; runs once at the end of every step.</summary>
    public virtual void Update(World world)
    {
    }

    /// <summary>
    /// Whether the match has ended. The point modes end when any player has reached the target score
    /// (localgame.cpp, the "lapi" checks).
    /// </summary>
    public virtual bool IsOver(World world) => world.Ships.Any(s => s.Score >= world.Settings.Credits);

    /// <summary>Adds <paramref name="points"/> to every player of <paramref name="team"/>; team scores are kept per player.</summary>
    protected static void AddTeamScore(World world, int team, float points)
    {
        foreach (var ship in world.Ships)
        {
            if (ship.Team == team)
            {
                ship.Score += points;
            }
        }
    }

    /// <summary>Distance between two points.</summary>
    protected static float Distance(float x1, float y1, float x2, float y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }
}
