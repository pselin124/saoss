using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Weapons;

/// <summary>
/// Shared building blocks for weapon behaviors: spawning projectiles, planet effects on projectiles,
/// hit tests and target lists. Keeps the per-ship behaviors short.
/// </summary>
public static class Combat
{
    /// <summary>Collision circles are smaller than the nominal radii in the original.</summary>
    public const float RadiusScale = 0.785f;

    /// <summary>Projectiles closer to the center than this squared distance hit the planet [planeettasadetoiseen].</summary>
    public const float PlanetRadiusSquared = GameConstants.PlanetRadius * GameConstants.PlanetRadius;

    /// <summary>Projectiles closer to the center than this squared distance feel planet gravity [gravitysadetoiseen].</summary>
    public const float ProjectileGravityRangeSquared = 1600f;

    private const int BeamSamples = 10;
    private const float BeamHitMargin = 0.2f;

    /// <summary>Ships that can currently be hit: in the game and flying.</summary>
    public static IEnumerable<Ship> Targets(World world) => world.Ships.Where(s => s.IsInGame && s.IsAlive);

    /// <summary>Ships that can be hit, excluding the shooter.</summary>
    public static IEnumerable<Ship> OtherTargets(World world, Ship shooter) => Targets(world).Where(s => s.Index != shooter.Index);

    /// <summary>Enemy ships that can be hit: other ships not on the shooter's team.</summary>
    public static IEnumerable<Ship> EnemyTargets(World world, Ship shooter) => OtherTargets(world, shooter).Where(s => s.Team != shooter.Team);

    /// <summary>A Magneto with its shield up cannot be hurt by most weapons.</summary>
    public static bool IsShielded(Ship ship) => ship.Type == ShipType.Magneto && ship.IsShieldActive;

    /// <summary>
    /// Whether the projectile touches the ship, using the given weapon radius [Player::tormaa(Pati, Alus)].
    /// Shielded Magnetos are never touched.
    /// Note: the original passed the shooter's primary radius here even for special projectiles.
    /// </summary>
    public static bool Touches(Ship target, Projectile projectile, float weaponRadius)
    {
        if (IsShielded(target))
        {
            return false;
        }

        var reach = (target.Definition.Radius + weaponRadius) * RadiusScale;
        return Square(target.Y - projectile.Y) + Square(target.X - projectile.X) < reach * reach;
    }

    /// <summary>
    /// Standard planet effect on a projectile: destroyed inside the planet, otherwise pulled toward it
    /// when closer than 40 units.
    /// </summary>
    public static void ApplyPlanet(Projectile projectile)
    {
        var distanceSquared = Square(projectile.X) + Square(projectile.Y);
        if (distanceSquared < PlanetRadiusSquared)
        {
            projectile.Lifetime = 0;
        }
        else if (distanceSquared < ProjectileGravityRangeSquared)
        {
            projectile.VelocityX -= projectile.X / distanceSquared * GameConstants.ProjectileGravity;
            projectile.VelocityY -= projectile.Y / distanceSquared * GameConstants.ProjectileGravity;
        }
    }

    /// <summary>
    /// Planet effect used by several special-cased weapons: like <see cref="ApplyPlanet"/>, but gravity also pulls a
    /// projectile that just hit the planet. Returns whether it hit the planet.
    /// </summary>
    public static bool ApplyPlanetAndPull(Projectile projectile)
    {
        var distanceSquared = Square(projectile.X) + Square(projectile.Y);
        var hitPlanet = distanceSquared < PlanetRadiusSquared;
        if (hitPlanet)
        {
            projectile.Lifetime = 0;
        }

        if (distanceSquared < ProjectileGravityRangeSquared)
        {
            projectile.VelocityX -= projectile.X / distanceSquared * GameConstants.ProjectileGravity;
            projectile.VelocityY -= projectile.Y / distanceSquared * GameConstants.ProjectileGravity;
        }

        return hitPlanet;
    }

    /// <summary>Returns the first free slot among the first <paramref name="count"/> entries, or null.</summary>
    public static Projectile? FreeSlot(Projectile[] pool, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (!pool[i].IsActive)
            {
                return pool[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Initializes a primary projectile leaving the ship: placed at <paramref name="distanceFactor"/> times the ship
    /// radius in direction heading + <paramref name="offsetDegrees"/>, moving along the heading at weapon speed
    /// plus the ship's velocity.
    /// </summary>
    public static void Launch(Projectile projectile, Ship ship, float offsetDegrees, float distanceFactor)
    {
        var weapon = ship.Definition.Primary;
        var (headingX, headingY) = ship.Heading;
        var offset = (ship.Angle + offsetDegrees) / 180f * MathF.PI;
        projectile.Lifetime = weapon.Lifetime;
        projectile.X = ship.X + (MathF.Cos(offset) * ship.Definition.Radius * distanceFactor);
        projectile.Y = ship.Y + (MathF.Sin(offset) * ship.Definition.Radius * distanceFactor);
        projectile.VelocityX = (headingX * weapon.Speed) + ship.VelocityX;
        projectile.VelocityY = (headingY * weapon.Speed) + ship.VelocityY;
        projectile.Angle = ship.Angle;
        projectile.Energy = weapon.ProjectileEnergy;
    }

    /// <summary>
    /// Whether a circle touches a beam, sampled at 10 points from the beam's start along <paramref name="length"/>.
    /// <paramref name="backward"/> is the angle from the beam's end to its start, as in the original; the samples may
    /// reach past the drawn end.
    /// </summary>
    public static bool BeamCrosses(Beam beam, float backward, float length, float x, float y, float radius)
    {
        var reachSquared = Square(BeamHitMargin + (radius * RadiusScale));
        for (var j = 0; j < BeamSamples; j++)
        {
            var offset = (float)j / BeamSamples * length;
            var dx = x - beam.X1 + (MathF.Cos(backward) * offset);
            var dy = y - beam.Y1 + (MathF.Sin(backward) * offset);
            if ((dx * dx) + (dy * dy) < reachSquared)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Pushes the target by the projectile's velocity times the knockback factor.</summary>
    public static void Knock(Ship target, Projectile projectile, float knockback)
    {
        target.VelocityX += projectile.VelocityX * knockback;
        target.VelocityY += projectile.VelocityY * knockback;
    }

    /// <summary>Plays one of the ship's own sounds.</summary>
    public static void PlaySound(this World world, Ship ship, ShipSound sound, bool loop = false) =>
        world.Emit(new ShipSoundPlayed(ship.Index, ship.Type, sound, loop));

    /// <summary>Squares a value.</summary>
    public static float Square(float value) => value * value;
}
