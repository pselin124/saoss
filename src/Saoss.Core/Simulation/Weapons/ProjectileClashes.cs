using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Weapons;

/// <summary>
/// Projectiles that destroy each other [Player::tarkistaHajosikoPadit]. Each ship checks its projectiles
/// against its own and all later ships' projectiles, so every pair is handled once per tick.
/// Whether a pair interacts depends only on the two weapon definitions, not on the ship type.
/// </summary>
public static class ProjectileClashes
{
    // Two special weapons with pools at least this large never clash (a performance limit in the original).
    private const int LargeSpecialPool = 180;

    /// <summary>
    /// Resolves clashes of <paramref name="ship"/>'s projectiles: primary vs primary, primary vs special,
    /// special vs primary and special vs special.
    /// </summary>
    public static void Update(Ship ship, World world)
    {
        var own = ship.Definition;
        foreach (var other in world.Ships)
        {
            if (other.Index < ship.Index || !other.IsInGame)
            {
                continue;
            }

            var theirs = other.Definition;
            var sameShip = other.Index == ship.Index;

            if ((own.Primary.CanBeDestroyed && theirs.Primary.DestroysProjectiles) || (theirs.Primary.CanBeDestroyed && own.Primary.DestroysProjectiles))
            {
                Clash(
                    ship.PrimaryProjectiles, own.Primary.MaxProjectiles, own.Primary.Radius, own.Primary.Damage, own.Primary.DestroysProjectiles, own.Primary.CanBeDestroyed,
                    other.PrimaryProjectiles, theirs.Primary.MaxProjectiles, theirs.Primary.Radius, theirs.Primary.Damage, theirs.Primary.DestroysProjectiles, theirs.Primary.CanBeDestroyed,
                    sameShip);
            }

            if ((own.Primary.CanBeDestroyed && theirs.Special.DestroysProjectiles) || (theirs.Special.CanBeDestroyed && own.Primary.DestroysProjectiles))
            {
                Clash(
                    ship.PrimaryProjectiles, own.Primary.MaxProjectiles, own.Primary.Radius, own.Primary.Damage, own.Primary.DestroysProjectiles, own.Primary.CanBeDestroyed,
                    other.SpecialProjectiles, theirs.Special.MaxProjectiles, theirs.Special.Radius, theirs.Special.Damage, theirs.Special.DestroysProjectiles, theirs.Special.CanBeDestroyed,
                    sameShip);
            }

            // The original used the two primary radii for special vs primary.
            if ((own.Special.CanBeDestroyed && theirs.Primary.DestroysProjectiles) || (theirs.Primary.CanBeDestroyed && own.Special.DestroysProjectiles))
            {
                Clash(
                    ship.SpecialProjectiles, own.Special.MaxProjectiles, own.Primary.Radius, own.Special.Damage, own.Special.DestroysProjectiles, own.Special.CanBeDestroyed,
                    other.PrimaryProjectiles, theirs.Primary.MaxProjectiles, theirs.Primary.Radius, theirs.Primary.Damage, theirs.Primary.DestroysProjectiles, theirs.Primary.CanBeDestroyed,
                    sameShip);
            }

            var specialsMayClash = !(sameShip && ship.Type is ShipType.Cubix or ShipType.StarWorm)
                && !(theirs.Special.MaxProjectiles >= LargeSpecialPool && own.Special.MaxProjectiles >= LargeSpecialPool);
            if (specialsMayClash
                && ((own.Special.CanBeDestroyed && theirs.Special.DestroysProjectiles) || (theirs.Special.CanBeDestroyed && own.Special.DestroysProjectiles)))
            {
                Clash(
                    ship.SpecialProjectiles, own.Special.MaxProjectiles, own.Special.Radius, own.Special.Damage, own.Special.DestroysProjectiles, own.Special.CanBeDestroyed,
                    other.SpecialProjectiles, theirs.Special.MaxProjectiles, theirs.Special.Radius, theirs.Special.Damage, theirs.Special.DestroysProjectiles, theirs.Special.CanBeDestroyed,
                    sameShip);
            }
        }
    }

    // For each of our live projectiles, damages overlapping projectiles of the other pool and takes damage back.
    // Our projectile stops checking after the first other projectile that can damage it.
    private static void Clash(
        Projectile[] ours, int ourCount, float ourRadius, float ourDamage, bool ourDestroys, bool ourBreakable,
        Projectile[] theirs, int theirCount, float theirRadius, float theirDamage, bool theirDestroys, bool theirBreakable,
        bool sameShip)
    {
        var radiusSum = ourRadius + theirRadius;
        var reachSquared = Combat.Square(radiusSum * Combat.RadiusScale);
        for (var i = 0; i < ourCount; i++)
        {
            var mine = ours[i];
            if (!mine.IsActive)
            {
                continue;
            }

            for (var k = 0; k < theirCount; k++)
            {
                var other = theirs[k];
                if (!other.IsActive || (sameShip && k == i))
                {
                    continue;
                }

                var dx = other.X - mine.X;
                var dy = other.Y - mine.Y;
                if (dx >= radiusSum || dx <= -radiusSum || dy >= radiusSum || dy <= -radiusSum || (dx * dx) + (dy * dy) >= reachSquared)
                {
                    continue;
                }

                if (theirBreakable && ourDestroys)
                {
                    other.Energy -= ourDamage;
                    if (other.Energy <= 0)
                    {
                        other.Lifetime = 0;
                    }
                }

                if (ourBreakable && theirDestroys)
                {
                    mine.Energy -= theirDamage;
                    if (mine.Energy <= 0)
                    {
                        mine.Lifetime = 0;
                    }

                    break;
                }
            }
        }
    }
}
