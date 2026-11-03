namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Popper: rapidly fires long-lived bouncy shots that kick the Popper back and keep hitting whatever they touch;
/// the special drops a mine that explodes after a while, hurting nearby ships (including the Popper) and shots.
/// </summary>
public sealed class PopperBehavior : ShipBehavior
{
    private const float MuzzleDistance = 1.5f;
    private const float BoundsPush = 0.02f;
    private const float ImpactMassDivisor = 200f;
    private const float BlastRadius = 25f;
    private const float ShipAndShotFalloff = 3.2f;
    private const float SpecialFalloff = 4f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;
        var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (shot == null)
        {
            return;
        }

        world.PlaySound(ship, ShipSound.Fire);
        Combat.Launch(shot, ship, 0f, MuzzleDistance);
        ship.VelocityX -= shot.VelocityX * weapon.Knockback;
        ship.VelocityY -= shot.VelocityY * weapon.Knockback;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var mine = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (mine != null)
        {
            mine.Lifetime = special.Duration;
            mine.Energy = special.ProjectileEnergy;
            mine.X = ship.X;
            mine.Y = ship.Y;
            mine.VelocityX = 0;
            mine.VelocityY = 0;
        }
    }

    /// <summary>
    /// Moves the shots, steers them back into the arena and applies the planet. A shot touching any ship damages and
    /// pushes it (scaled by the masses) and is slowed by the ship's velocity, but stays alive.
    /// </summary>
    public override void UpdatePrimaryProjectiles(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        for (var i = 0; i < weapon.MaxProjectiles; i++)
        {
            var shot = ship.PrimaryProjectiles[i];
            if (!shot.IsActive)
            {
                continue;
            }

            shot.Lifetime--;
            shot.VelocityX += shot.X > GameConstants.ArenaMaxX ? -BoundsPush : shot.X < GameConstants.ArenaMinX ? BoundsPush : 0;
            shot.VelocityY += shot.Y > GameConstants.ArenaMaxY ? -BoundsPush : shot.Y < GameConstants.ArenaMinY ? BoundsPush : 0;
            shot.X += shot.VelocityX;
            shot.Y += shot.VelocityY;
            Combat.ApplyPlanet(shot);

            // No owner check: the Popper can hit itself; the shot is not removed, so it hits every tick it overlaps.
            foreach (var target in Combat.Targets(world))
            {
                if (Combat.Touches(target, shot, weapon.Radius))
                {
                    Impact(ship, world, shot, target);
                }
            }
        }
    }

    /// <summary>Counts mines down; a mine explodes on its last tick.</summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var mine = ship.SpecialProjectiles[i];
            if (!mine.IsActive)
            {
                continue;
            }

            mine.Lifetime--;
            if (mine.Lifetime == 1)
            {
                mine.Lifetime = 0;
                Explode(ship, world, mine);
            }
        }
    }

    private static void Impact(Ship ship, World world, Projectile shot, Ship target)
    {
        var weapon = ship.Definition.Primary;
        world.PlaySound(ship, ShipSound.Hit);
        target.Energy -= weapon.Damage;
        var targetVelocityX = target.VelocityX;
        var targetVelocityY = target.VelocityY;
        target.VelocityX += shot.VelocityX * weapon.Knockback / target.Definition.Mass * ship.Definition.Mass;
        target.VelocityY += shot.VelocityY * weapon.Knockback / target.Definition.Mass * ship.Definition.Mass;
        shot.VelocityX -= targetVelocityX * target.Definition.Mass / ImpactMassDivisor;
        shot.VelocityY -= targetVelocityY * target.Definition.Mass / ImpactMassDivisor;
    }

    // No team check: the blast hurts the Popper and its teammates too.
    private static void Explode(Ship ship, World world, Projectile mine)
    {
        var damage = ship.Definition.Special.Damage;
        world.PlaySound(ship, ShipSound.SpecialHit);
        foreach (var target in Combat.Targets(world))
        {
            var distance = Distance(target.X - mine.X, target.Y - mine.Y);
            if (distance < BlastRadius && !Combat.IsShielded(target))
            {
                target.Energy -= damage - (distance * ShipAndShotFalloff);
            }
        }

        foreach (var owner in world.Ships.Where(s => s.IsInGame))
        {
            if (owner.Definition.Primary.CanBeDestroyed)
            {
                Blast(mine, owner.PrimaryProjectiles, owner.Definition.Primary.MaxProjectiles, damage, ShipAndShotFalloff);
            }

            // Far specials can gain energy here (damage minus up to 100), as in the original.
            if (owner.Definition.Special.CanBeDestroyed)
            {
                Blast(mine, owner.SpecialProjectiles, owner.Definition.Special.MaxProjectiles, damage, SpecialFalloff);
            }
        }

        world.Emit(new EffectEvent(EffectKind.PopperMineExplosion, mine.X, mine.Y));
    }

    private static void Blast(Projectile mine, Projectile[] pool, int count, float damage, float falloff)
    {
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (!projectile.IsActive)
            {
                continue;
            }

            var distance = Distance(projectile.X - mine.X, projectile.Y - mine.Y);
            if (distance < BlastRadius)
            {
                projectile.Energy -= damage - (distance * falloff);
                if (projectile.Energy <= 0)
                {
                    projectile.Lifetime = 0;
                }
            }
        }
    }

    private static float Distance(float x, float y) => MathF.Sqrt((x * x) + (y * y));
}
