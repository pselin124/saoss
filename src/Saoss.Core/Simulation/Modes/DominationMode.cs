namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// Flower Domination (domination.cpp): two control points in opposite corners. A point goes to the team that is
/// alone within its radius and stays theirs until another team takes it; every tick each player of the owning
/// team scores a little per point held.
/// </summary>
public sealed class DominationMode : GameModeRules
{
    /// <summary>Capture radius of a control point [DSADE].</summary>
    public const float CaptureRadius = 10f;

    /// <summary>Points per tick for every player of the owning team, per point held [pisteetfreimissa].</summary>
    public const float PointsPerTick = 0.005f;

    private const float CornerInset = 20f;

    private readonly ControlPoint[] _points =
    [
        new(GameConstants.ArenaMaxX - CornerInset, GameConstants.ArenaMaxY - CornerInset),
        new(GameConstants.ArenaMinX + CornerInset, GameConstants.ArenaMinY + CornerInset),
    ];

    /// <summary>The control points [dpiste].</summary>
    public IReadOnlyList<ControlPoint> Points => _points;

    /// <summary>
    /// Takes and scores the points (laskeDPisteet). Steps per point: the first ship in range proposes its team,
    /// any ship of another team in range cancels it, an uncontested proposal takes the point, then the owner scores.
    /// Destroyed ships still count where they died, as in the original.
    /// </summary>
    public override void Update(World world)
    {
        foreach (var point in _points)
        {
            var claimant = world.Ships.FirstOrDefault(s => InRange(point, s))?.Team ?? -1;
            if (claimant >= 0 && world.Ships.Any(s => InRange(point, s) && s.Team != claimant))
            {
                claimant = -1;
            }

            if (claimant >= 0)
            {
                point.OwnerTeam = claimant;
            }

            if (point.OwnerTeam >= 0)
            {
                AddTeamScore(world, point.OwnerTeam, PointsPerTick);
            }
        }
    }

    private static bool InRange(ControlPoint point, Ship ship) => Distance(point.X, point.Y, ship.X, ship.Y) < CaptureRadius;
}
