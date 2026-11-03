using Saoss.Core.Ships;
using Saoss.Core.Simulation.Weapons;

namespace Saoss.Core.Simulation;

/// <summary>
/// The whole match state and the fixed-step update [calculateFrame].
/// Set each ship's <see cref="Ship.Controls"/>, call <see cref="Step"/>, then read <see cref="Events"/>.
/// </summary>
public sealed class World
{
    private const float PlanetGravityExponent = 2.3f;
    private const float MinPlanetBounceSpeed = 0.1f;
    private const float PlanetEscapeDistance = 5f;
    private const float MothRevivalChance = 50f;

    private readonly List<Ship> _ships;
    private readonly List<GameEvent> _events = [];

    /// <summary>Creates a match and gives every player a random ship.</summary>
    public World(MatchSettings settings, int seed)
    {
        Settings = settings;
        Random = new GameRandom(seed);
        _ships = [.. settings.Players.Select((setup, index) => new Ship(index, setup.Team, setup.IsHuman))];
        Asteroids = new AsteroidField(Random);
        Pickups = [.. Enumerable.Range(0, GameConstants.MaxPickups).Select(_ => new Pickup())];
        foreach (var ship in _ships)
        {
            ship.Respawn(RandomShip(), Random);
        }
    }

    /// <summary>Settings the match was started with.</summary>
    public MatchSettings Settings { get; }

    /// <summary>Random source shared by everything in the match.</summary>
    public GameRandom Random { get; }

    /// <summary>All player ships, indexed by <see cref="Ship.Index"/>.</summary>
    public IReadOnlyList<Ship> Ships => _ships;

    /// <summary>The asteroids.</summary>
    public AsteroidField Asteroids { get; }

    /// <summary>Health pickup pool.</summary>
    public IReadOnlyList<Pickup> Pickups { get; }

    /// <summary>Number of completed steps.</summary>
    public int Tick { get; private set; }

    /// <summary>Events produced by the last <see cref="Step"/>.</summary>
    public IReadOnlyList<GameEvent> Events => _events;

    /// <summary>
    /// Advances the match by one tick. Steps: ship movement and weapons (in ship order), pickups,
    /// asteroids, planet gravity and impacts, death timers, explosions, and respawns.
    /// </summary>
    public void Step()
    {
        _events.Clear();
        Tick++;

        foreach (var ship in _ships)
        {
            if (ship.IsInGame && ship.IsAlive)
            {
                var behavior = ShipBehaviors.For(ship.Type);
                ShipMovement.Update(ship, this);
                behavior.UpdatePrimaryProjectiles(ship, this);
                behavior.UpdateSpecialProjectiles(ship, this);
                behavior.UpdateProjectileClashes(ship, this);
            }
        }

        UpdatePickups();
        Asteroids.Update(this);

        foreach (var ship in _ships)
        {
            if (ship.IsInGame)
            {
                ApplyPlanet(ship);
            }
        }

        foreach (var ship in _ships)
        {
            if (ship.IsInGame)
            {
                UpdateDeath(ship);
            }
        }
    }

    /// <summary>Queues an event for the presentation layer.</summary>
    public void Emit(GameEvent gameEvent) => _events.Add(gameEvent);

    /// <summary>Picks a uniformly random ship type.</summary>
    public ShipDefinition RandomShip() => ShipCatalog.All[Random.NextInt(ShipCatalog.All.Count)];

    private void UpdatePickups()
    {
        foreach (var pickup in Pickups)
        {
            if (!pickup.IsActive)
            {
                continue;
            }

            pickup.Lifetime--;
            if (pickup.Delay > 0)
            {
                pickup.Delay--;
                continue;
            }

            foreach (var ship in _ships)
            {
                if (ship.IsInGame && ship.IsAlive && Distance(pickup.X - ship.X, pickup.Y - ship.Y) < ship.Definition.Radius)
                {
                    ship.Energy++;
                    pickup.Lifetime = 0;
                    break;
                }
            }
        }
    }

    // Gravity pulls ships toward the planet; touching it costs energy and bounces the ship off.
    private static void ApplyPlanet(Ship ship)
    {
        var definition = ship.Definition;
        var distance = Distance(ship.X, ship.Y);
        if (definition.IsInertiaFree)
        {
            if (distance < GameConstants.PlanetRadius + definition.Radius)
            {
                ship.Energy -= GameConstants.PlanetDamage;
                ship.X += ship.X * definition.Thrust * 2f;
                ship.Y += ship.Y * definition.Thrust * 2f;
            }

            return;
        }

        var positionAngle = MathF.Atan2(ship.Y, ship.X);
        var velocityAngle = MathF.Atan2(ship.VelocityY, ship.VelocityX);
        var speed = Distance(ship.VelocityX, ship.VelocityY);
        if (distance > 0)
        {
            var pull = GameConstants.Gravity * (GameConstants.PlanetMass / definition.Mass) / MathF.Pow(distance, PlanetGravityExponent);
            ship.VelocityX -= ship.X * pull;
            ship.VelocityY -= ship.Y * pull;
        }

        if (distance < GameConstants.PlanetRadius + definition.Radius)
        {
            ship.Energy -= GameConstants.PlanetDamage;
            var bounceAngle = positionAngle + (positionAngle - (velocityAngle + MathF.PI));
            speed = MathF.Max(speed, MinPlanetBounceSpeed);
            ship.VelocityX = MathF.Cos(bounceAngle) * speed;
            ship.VelocityY = MathF.Sin(bounceAngle) * speed;
            if (Distance(ship.X + ship.VelocityX, ship.Y + ship.VelocityY) < PlanetEscapeDistance)
            {
                ship.VelocityX = -ship.VelocityX;
                ship.VelocityY = -ship.VelocityY;
            }
        }
    }

    // Order matters: the timer counts down first, then a newly destroyed ship starts its timer,
    // then a ship whose timer reached the respawn point gets its new ship.
    private void UpdateDeath(Ship ship)
    {
        if (ship.DeathTimer > 0)
        {
            ship.DeathTimer--;
        }

        if (ship.Energy <= 0 && ship.IsAlive)
        {
            Explode(ship);
        }

        if (ship.DeathTimer == GameConstants.SpawnTicks)
        {
            ship.Respawn(RandomShip(), Random);
            Emit(new ShipRespawning(ship.Index));
        }

        if (ship.DeathTimer == 1)
        {
            ship.Energy = ship.Definition.MaxEnergy;
            ship.VelocityX = 0;
            ship.VelocityY = 0;
        }
    }

    private void Explode(Ship ship)
    {
        Emit(new ShipExploded(ship.Index, ship.X, ship.Y));
        ship.IsShieldActive = false;
        if (ship.Type == ShipType.Moth && Random.Next(100f) >= MothRevivalChance)
        {
            ship.Respawn(ShipCatalog.Get(ShipType.Moth), Random);
            Emit(new ShipSoundPlayed(ship.Index, ShipType.Moth, ShipSound.SpecialHit));
            Emit(new ShipRevived(ship.Index));
            return;
        }

        if (ship.IsFireLoopPlaying)
        {
            ship.IsFireLoopPlaying = false;
            Emit(new ShipLoopStopped(ship.Index));
        }

        // The Wasp's special sound doubles as the explosion sound for every ship.
        Emit(new ShipSoundPlayed(ship.Index, ShipType.Wasp, ShipSound.SpecialFire));
        ship.DeathTimer = GameConstants.SpawnTicks + GameConstants.DeathTicks;
    }

    private static float Distance(float x, float y) => MathF.Sqrt((x * x) + (y * y));
}
