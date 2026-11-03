namespace Saoss.Core;

/// <summary>
/// Global simulation constants carried over from the original <c>Define.h</c>.
/// Per-tick values assume <see cref="TicksPerSecond"/> simulation steps per second.
/// </summary>
public static class GameConstants
{
    /// <summary>Simulation rate; the original game's default <c>fps</c> setting.</summary>
    public const int TicksPerSecond = 100;

    /// <summary>Maximum number of player slots (human, AI or network).</summary>
    public const int MaxPlayers = 16;

    /// <summary>Number of distinct ship types.</summary>
    public const int ShipTypeCount = 24;

    /// <summary>Velocity damping factor [kitka].</summary>
    public const float Friction = 0.006f;

    /// <summary>Arena bounds in world units. Ships outside are pushed back.</summary>
    public const float ArenaMinX = -160f;

    /// <inheritdoc cref="ArenaMinX"/>
    public const float ArenaMaxX = 160f;

    /// <inheritdoc cref="ArenaMinX"/>
    public const float ArenaMinY = -120f;

    /// <inheritdoc cref="ArenaMinX"/>
    public const float ArenaMaxY = 120f;

    /// <summary>Z coordinate of the plane the ships fly on [alustaso].</summary>
    public const float ShipPlaneZ = -35f;

    /// <summary>Radius of the planet at the origin [planeettasade].</summary>
    public const float PlanetRadius = 8f;

    /// <summary>Planet mass in the gravity formula [pmassa].</summary>
    public const float PlanetMass = 50000f;

    /// <summary>Gravity constant for ships [gravity].</summary>
    public const float Gravity = 0.0004f;

    /// <summary>Damage per tick while touching the planet [planeettadamage].</summary>
    public const float PlanetDamage = 5f;

    /// <summary>Gravity constant for projectiles near the planet [patigravity].</summary>
    public const float ProjectileGravity = 0.1f;

    /// <summary>Projectiles closer than this to the planet feel its gravity [gravitysade].</summary>
    public const float ProjectileGravityRadius = 35f;

    /// <summary>Ticks a destroyed ship stays exploded before it starts to respawn [kuolemisaika].</summary>
    public const int DeathTicks = 150;

    /// <summary>Ticks of the respawn animation before the ship is back in play [syntymisaika].</summary>
    public const int SpawnTicks = 200;

    /// <summary>Number of asteroids in the arena [asteroideja].</summary>
    public const int AsteroidCount = 20;

    /// <summary>Hit points of a new asteroid [asteroidienergia].</summary>
    public const float AsteroidEnergy = 10f;

    /// <summary>Maximum slime blobs stuck on one ship [maxlimat].</summary>
    public const int MaxSlimes = 30;

    /// <summary>Number of remembered past positions used for tails [palikkamaara].</summary>
    public const int TailLength = 12;

    /// <summary>Maximum beam segments per ship [maxviivat].</summary>
    public const int MaxBeams = 60;

    /// <summary>Size of the health pickup pool [pikkumiehia].</summary>
    public const int MaxPickups = 200;
}
