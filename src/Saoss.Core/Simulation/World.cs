using Saoss.Core.Ships;
using Saoss.Core.Simulation.Ai;
using Saoss.Core.Simulation.Modes;
using Saoss.Core.Simulation.Snapshots;
using Saoss.Core.Simulation.Weapons;

namespace Saoss.Core.Simulation;

/// <summary>
/// The whole match state and the fixed-step update [calculateFrame].
/// Set each ship's <see cref="Ship.Controls"/>, call <see cref="Step"/>, then read <see cref="Events"/>.
/// While <see cref="PendingChoice"/> is set the match is paused until <see cref="ChooseShip"/> is called,
/// and once <see cref="Outcome"/> is set it no longer changes. Without <see cref="MatchSettings.ChoicesPauseMatch"/>
/// the match goes on while humans choose (<see cref="IsChoosingShip"/>); they stay out until they buy.
/// </summary>
/// </summary>
public sealed class World
{
    private const float PlanetGravityExponent = 2.3f;
    private const float MinPlanetBounceSpeed = 0.1f;
    private const float PlanetEscapeDistance = 5f;
    private const float MothRevivalChance = 50f;
    private const float DroppedOutScore = -1f;

    private readonly List<Ship> _ships;
    private readonly List<GameEvent> _events = [];
    private readonly Queue<int> _choiceQueue = new();
    private int _stepEventCount;

    /// <summary>
    /// Creates a match and gives every player a random ship. In a credits game every player then gets the
    /// starting credits and buys a ship; human players who choose their ships leave <see cref="PendingChoice"/> set.
    /// </summary>
    public World(MatchSettings settings, int seed)
    {
        Settings = settings;
        Random = new GameRandom(seed);
        Rules = GameModeRules.Create(settings);
        _ships = [.. settings.Players.Select((setup, index) => new Ship(index, setup.Team, setup.IsHuman))];
        Asteroids = new AsteroidField(Random);
        Pickups = [.. Enumerable.Range(0, GameConstants.MaxPickups).Select(_ => new Pickup())];
        foreach (var ship in _ships)
        {
            ship.Respawn(RandomShip(), Random);
        }

        if (settings.UsesCredits)
        {
            StartCreditsGame();
        }
    }

    /// <summary>Settings the match was started with.</summary>
    public MatchSettings Settings { get; }

    /// <summary>Rules and state of the game mode.</summary>
    public GameModeRules Rules { get; }

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

    /// <summary>
    /// Events produced by the last <see cref="Step"/>, followed by those of any <see cref="ChooseShip"/> call since;
    /// the next step drops them.
    /// </summary>
    public IReadOnlyList<GameEvent> Events => _events;

    /// <summary>
    /// A human player who must buy a ship before the match goes on, or null. AI players buy at once. Always null
    /// when choices do not pause the match.
    /// </summary>
    public ShipChoice? PendingChoice { get; private set; }

    /// <summary>How the match ended, or null while it is still running.</summary>
    public MatchOutcome? Outcome { get; private set; }

    /// <summary>Players waiting to buy a ship, in buying order; the head is <see cref="PendingChoice"/> once humans are reached.</summary>
    internal IReadOnlyCollection<int> ChoiceQueue => _choiceQueue;

    /// <summary>How many of <see cref="Events"/> came from the last step; the next step drops these.</summary>
    internal int StepEventCount => _stepEventCount;

    /// <summary>
    /// Advances the match by one tick; does nothing while a ship choice is pending or after the match has ended.
    /// Steps: AI pilots, ship movement and weapons (in ship order), pickups, asteroids, planet gravity and impacts,
    /// death timers, explosions and respawns, then the mode's scoring and the end-of-match check.
    /// </summary>
    public void Step()
    {
        _events.RemoveRange(0, _stepEventCount);
        _stepEventCount = 0;
        if (PendingChoice != null || Outcome != null)
        {
            return;
        }

        // A player who left while choosing is now AI and buys at once.
        ResolveAiChoices();
        Tick++;

        // The original ran the AI for destroyed ships too.
        foreach (var ship in _ships)
        {
            if (ship.IsInGame && !ship.IsHuman)
            {
                AiPilot.Update(ship, this);
            }
        }

        foreach (var ship in _ships)
        {
            if (ship.IsInGame && ship.IsAlive)
            {
                var behavior = ShipBehaviors.For(ship.Type);
                ShipMovement.Update(ship, this);
                behavior.UpdatePrimaryProjectiles(ship, this);
                behavior.UpdateSpecialProjectiles(ship, this);
                ProjectileClashes.Update(ship, this);
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

        // The original advanced beams while drawing each living ship, i.e. once after every tick.
        foreach (var ship in _ships)
        {
            if (ship.IsInGame && ship.IsAlive)
            {
                foreach (var beam in ship.Beams)
                {
                    beam.Update();
                }
            }
        }

        Rules.Update(this);
        if (Rules.IsOver(this))
        {
            Outcome = MatchOutcome.From(this);
        }

        _stepEventCount = _events.Count;
    }

    /// <summary>
    /// Buys the pending player's ship: <paramref name="type"/>, or a random affordable one when null (the menu's
    /// "random ship" entry). Returns false and changes nothing when the player cannot afford the type; the
    /// original menu then jumped to <see cref="ShipShop.FallbackShip"/> and waited for another confirmation.
    /// </summary>
    public bool ChooseShip(int shipIndex, ShipType? type)
    {
        if (!IsChoosingShip(shipIndex))
        {
            throw new InvalidOperationException($"Ship {shipIndex} is not choosing a ship.");
        }

        var ship = _ships[shipIndex];
        var definition = type is { } chosen ? ShipCatalog.Get(chosen) : ShipShop.RandomAffordable(ship, Random);
        if (!ShipShop.CanAfford(ship, definition))
        {
            return false;
        }

        RemoveFromChoiceQueue(shipIndex);
        CompletePurchase(ship, definition);
        ResolveAiChoices();
        return true;
    }

    /// <summary>
    /// Whether the player may buy a ship now: it is the <see cref="PendingChoice"/>, or, when choices do not pause
    /// the match, any player waiting to buy.
    /// </summary>
    public bool IsChoosingShip(int shipIndex) =>
        PendingChoice?.ShipIndex == shipIndex || (!Settings.ChoicesPauseMatch && _choiceQueue.Contains(shipIndex));

    /// <summary>Queues an event for the presentation layer.</summary>
    public void Emit(GameEvent gameEvent) => _events.Add(gameEvent);

    /// <summary>Overwrites the match-level state with values read from a snapshot; ships and modes are restored separately.</summary>
    internal void RestoreState(WorldState state)
    {
        Tick = state.Tick;
        _events.Clear();
        _events.AddRange(state.Events);
        _stepEventCount = state.StepEventCount;
        _choiceQueue.Clear();
        foreach (var index in state.ChoiceQueue)
        {
            _choiceQueue.Enqueue(index);
        }

        // Kept when unchanged so the presentation layer can tell a new choice from the same one by reference.
        if (PendingChoice?.ShipIndex != state.PendingChoiceIndex)
        {
            PendingChoice = state.PendingChoiceIndex is { } index ? new ShipChoice(index) : null;
        }

        if (Outcome == null || state.Outcome == null)
        {
            Outcome = state.Outcome;
        }
    }

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

            // Every ship touching the pickup in the same tick gets the hit point.
            foreach (var ship in _ships)
            {
                if (ship.IsInGame && ship.IsAlive && Distance(pickup.X - ship.X, pickup.Y - ship.Y) < ship.Definition.Radius)
                {
                    ship.Energy++;
                    pickup.Lifetime = 0;
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
        if (IsWaitingToBuy(ship))
        {
            return;
        }

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
            Rules.OnRespawnPoint(this, ship);
            ReplaceShip(ship);
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

    private static void DropOut(Ship ship)
    {
        ship.Score = DroppedOutScore;
        ship.IsInGame = false;
    }

    // The original bought the first ships during the first tick, after one tick with the previous match's ships.
    private void StartCreditsGame()
    {
        foreach (var ship in _ships)
        {
            ship.Score = Settings.Credits;
        }

        foreach (var ship in _ships)
        {
            // The original looped forever when the starting credits could not buy any ship.
            if (ship.Score < ShipShop.FallbackShip.Cost)
            {
                DropOut(ship);
            }
            else if (Settings.PlayersChooseShips)
            {
                _choiceQueue.Enqueue(ship.Index);

                // A buyer the match does not wait for stays out until the purchase brings it in.
                if (!Settings.ChoicesPauseMatch && ship.IsHuman)
                {
                    ship.DeathTimer = GameConstants.SpawnTicks;
                }
            }
            else
            {
                ShipShop.Buy(ship, ShipShop.RandomAffordable(ship, Random), Random);
            }
        }

        ResolveAiChoices();
    }

    // The new ship at the respawn point: random without credits; with credits the player buys one or drops out.
    private void ReplaceShip(Ship ship)
    {
        if (!Settings.UsesCredits)
        {
            ship.Respawn(RandomShip(), Random);
            Emit(new ShipRespawning(ship.Index));
            return;
        }

        ship.IsShieldActive = false;
        if (ShipShop.MustDropOut(ship))
        {
            DropOut(ship);
        }
        else if (Settings.PlayersChooseShips)
        {
            _choiceQueue.Enqueue(ship.Index);
            ResolveAiChoices();
        }
        else
        {
            CompletePurchase(ship, ShipShop.RandomAffordable(ship, Random));
        }
    }

    // Players choose one at a time in ship order, so an AI's counter-pick sees the ships bought before it.
    // The original AI scrolled through the on-screen menu; only its final pick is reproduced.
    private void ResolveAiChoices()
    {
        if (!Settings.ChoicesPauseMatch)
        {
            foreach (var waiting in _choiceQueue.Where(i => !_ships[i].IsHuman).ToList())
            {
                RemoveFromChoiceQueue(waiting);
                var buyer = _ships[waiting];
                CompletePurchase(buyer, ShipShop.AiChoice(this, buyer) ?? ShipShop.RandomAffordable(buyer, Random));
            }

            return;
        }

        while (_choiceQueue.TryPeek(out var index) && !_ships[index].IsHuman)
        {
            _choiceQueue.Dequeue();
            var ship = _ships[index];
            CompletePurchase(ship, ShipShop.AiChoice(this, ship) ?? ShipShop.RandomAffordable(ship, Random));
        }

        PendingChoice = _choiceQueue.TryPeek(out var pending) ? new ShipChoice(pending) : null;
    }

    private bool IsWaitingToBuy(Ship ship) => !Settings.ChoicesPauseMatch && _choiceQueue.Contains(ship.Index);

    private void RemoveFromChoiceQueue(int shipIndex)
    {
        var rest = _choiceQueue.Where(i => i != shipIndex).ToList();
        _choiceQueue.Clear();
        foreach (var index in rest)
        {
            _choiceQueue.Enqueue(index);
        }
    }

    private void CompletePurchase(Ship ship, ShipDefinition definition)
    {
        ShipShop.Buy(ship, definition, Random);
        if (ship.DeathTimer == GameConstants.SpawnTicks)
        {
            Emit(new ShipRespawning(ship.Index));
        }
    }
}
