namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// Capture The Flower (ctf.cpp): every team has a flower on a base in a corner. An enemy picks it up by touching
/// it; a carrier who reaches its own base while its own flower is at home scores for the whole team and the
/// flower goes back. A destroyed carrier drops the flower; the next ship to touch it either picks it up (enemy)
/// or returns it (owner). Every death costs the dying player's team points.
/// </summary>
public sealed class CaptureTheFlowerMode : GameModeRules
{
    /// <summary>Pick-up and capture radius [CSADE].</summary>
    public const float TouchRadius = 7f;

    /// <summary>Points for every player of the team that brings a flower home [ctfpisteet].</summary>
    public const float CapturePoints = 25f;

    /// <summary>Points every player of a team loses when one of them dies [ckuolemapisteet].</summary>
    public const float DeathPenalty = 1f;

    private const float CornerInset = 20f;

    private readonly Flower[] _flowers;

    /// <summary>Creates a flower for every possible team and enables those of the given teams.</summary>
    public CaptureTheFlowerMode(IEnumerable<int> teams)
    {
        _flowers = [.. Enumerable.Range(0, GameConstants.MaxPlayers).Select(CreateFlower)];
        foreach (var team in teams)
        {
            _flowers[team].IsEnabled = true;
        }
    }

    /// <summary>One flower per team, indexed by team; only enabled ones are in play [cpiste].</summary>
    public IReadOnlyList<Flower> Flowers => _flowers;

    /// <summary>The flower <paramref name="shipIndex"/> is carrying, if any.</summary>
    public Flower? CarriedBy(int shipIndex) =>
        _flowers.FirstOrDefault(f => f.IsEnabled && f.State == FlowerState.Carried && f.CarrierIndex == shipIndex);

    /// <inheritdoc/>
    public override void OnRespawnPoint(World world, Ship ship)
    {
        // Local games subtracted the penalty twice: once in a local-only check and again in the general one.
        AddTeamScore(world, ship.Team, -DeathPenalty);
        AddTeamScore(world, ship.Team, -DeathPenalty);
    }

    /// <summary>Moves, captures and returns flowers (laskeCPisteet); destroyed ships can still touch flowers.</summary>
    public override void Update(World world)
    {
        foreach (var flower in _flowers)
        {
            if (!flower.IsEnabled)
            {
                continue;
            }

            switch (flower.State)
            {
                case FlowerState.Carried:
                    UpdateCarried(world, flower);
                    break;
                case FlowerState.AtHome:
                    var taker = world.Ships.FirstOrDefault(s => Touches(flower, s) && s.Team != flower.Team);
                    if (taker != null)
                    {
                        flower.PickUp(taker.Index);
                    }

                    break;
                default:
                    var toucher = world.Ships.FirstOrDefault(s => Touches(flower, s));
                    if (toucher != null && toucher.Team != flower.Team)
                    {
                        flower.PickUp(toucher.Index);
                    }
                    else if (toucher != null)
                    {
                        flower.ReturnHome();
                    }

                    break;
            }
        }
    }

    // Base positions: teams 1-4 in the corners, any other team on the planet in the middle.
    private static Flower CreateFlower(int team)
    {
        var (x, y) = team switch
        {
            0 => (GameConstants.ArenaMaxX - CornerInset, GameConstants.ArenaMaxY - CornerInset),
            1 => (GameConstants.ArenaMinX + CornerInset, GameConstants.ArenaMinY + CornerInset),
            2 => (GameConstants.ArenaMaxX - CornerInset, GameConstants.ArenaMinY + CornerInset),
            3 => (GameConstants.ArenaMinX + CornerInset, GameConstants.ArenaMaxY - CornerInset),
            _ => (0f, 0f),
        };
        return new Flower(team, x, y);
    }

    private static bool Touches(Flower flower, Ship ship) => Distance(flower.X, flower.Y, ship.X, ship.Y) < TouchRadius;

    // A living carrier scores at its own base if its own flower is home; a destroyed one drops the flower.
    private void UpdateCarried(World world, Flower flower)
    {
        var carrier = world.Ships[flower.CarrierIndex];
        if (carrier.Energy >= 0 && carrier.DeathTimer < 1)
        {
            var home = _flowers[carrier.Team];
            if (Distance(carrier.X, carrier.Y, home.X, home.Y) < TouchRadius && home.State == FlowerState.AtHome)
            {
                flower.ReturnHome();
                AddTeamScore(world, carrier.Team, CapturePoints);
            }
        }
        else
        {
            flower.Drop(carrier.X, carrier.Y);
        }
    }
}
