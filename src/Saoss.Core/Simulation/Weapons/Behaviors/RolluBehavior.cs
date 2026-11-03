namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Rollu: fires a heavy bomb that bursts into a ring of 30 fragments when the fire button is released or
/// its time runs out; the special releases leeches that chase the nearest enemy and drain its fuel.
/// </summary>
public sealed class RolluBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.7f;
    private const float BombBoundsPush = 0.03f;
    private const int FragmentCount = 30;
    private const float FragmentSpawnDistance = 4f;
    private const float FragmentSpeed = 0.15f;
    private const int FragmentLifetime = 40;
    private const float FragmentEnergyDivisor = 8f;
    private const float FragmentDamageDivisor = 8f;
    private const float DropDistance = 1.1f;
    private const float LeechGrip = 0.05f;
    private const float MinLeechOffset = 0.01f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        var bomb = ship.PrimaryProjectiles[0];
        if (bomb.IsActive)
        {
            return;
        }

        world.PlaySound(ship, ShipSound.Fire);
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;
        Combat.Launch(bomb, ship, 0f, MuzzleDistance);
    }

    /// <summary>
    /// Slot 0 is the bomb, the other slots are its fragments. Per projectile: move, planet, keep the bomb in
    /// the arena, burst the bomb when it expires or fire is released, then hit other ships (the bomb with full
    /// damage and the special hit sound, fragments with an eighth of it).
    /// </summary>
    public override void UpdatePrimaryProjectiles(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        for (var i = 0; i < weapon.MaxProjectiles; i++)
        {
            var projectile = ship.PrimaryProjectiles[i];
            if (!projectile.IsActive)
            {
                continue;
            }

            projectile.Lifetime--;
            projectile.X += projectile.VelocityX;
            projectile.Y += projectile.VelocityY;
            Combat.ApplyPlanetAndPull(projectile);

            var isBomb = i == 0;
            if (isBomb)
            {
                KeepInArena(projectile);
                if (projectile.Lifetime <= 0 || !ship.Controls.Fire)
                {
                    Burst(ship, world, projectile);
                }
            }

            foreach (var target in Combat.OtherTargets(world, ship))
            {
                if (!Combat.Touches(target, projectile, weapon.Radius))
                {
                    continue;
                }

                projectile.Lifetime = 0;
                world.PlaySound(ship, isBomb ? ShipSound.SpecialHit : ShipSound.Hit);
                target.Energy -= isBomb ? weapon.Damage : weapon.Damage / FragmentDamageDivisor;
                Combat.Knock(target, projectile, weapon.Knockback);
            }
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var leech = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (leech == null)
        {
            return;
        }

        var behind = (ship.Angle + 180f) / 180f * MathF.PI;
        leech.X = ship.X + (MathF.Cos(behind) * ship.Definition.Radius * DropDistance);
        leech.Y = ship.Y + (MathF.Sin(behind) * ship.Definition.Radius * DropDistance);
        leech.VelocityX = ship.VelocityX;
        leech.VelocityY = ship.VelocityY;
        leech.Angle = ship.Angle - 180f;
        leech.Lifetime = special.Duration;
        leech.Energy = special.ProjectileEnergy;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
    }

    /// <summary>
    /// Per leech: find the nearest enemy, drain its fuel and stick to it when touching, apply drag,
    /// accelerate toward the enemy, move, face it, and finally the planet.
    /// </summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var leech = ship.SpecialProjectiles[i];
            if (!leech.IsActive)
            {
                continue;
            }

            leech.Lifetime--;
            var target = Combat.EnemyTargets(world, ship)
                .MinBy(t => Combat.Square(leech.X - t.X) + Combat.Square(leech.Y - t.Y));
            var dx = target == null ? 0f : leech.X - target.X;
            var dy = target == null ? 0f : leech.Y - target.Y;
            var distance = MathF.Sqrt((dx * dx) + (dy * dy));
            if (target != null && distance < (special.Radius + target.Definition.Radius) * Combat.RadiusScale)
            {
                Drain(ship, world, leech, target, ref dx, ref dy);
            }

            // The drag uses the Rollu's own speed, not the leech's.
            leech.VelocityX /= 1f + (GameConstants.Friction * (MathF.Abs(ship.VelocityX) + 1f));
            leech.VelocityY /= 1f + (GameConstants.Friction * (MathF.Abs(ship.VelocityY) + 1f));
            if (target != null)
            {
                leech.VelocityX -= dx / distance * special.Speed;
                leech.VelocityY -= dy / distance * special.Speed;
            }

            leech.X += leech.VelocityX;
            leech.Y += leech.VelocityY;
            if (target != null)
            {
                leech.Angle = (MathF.Atan2(dy, dx) * 180f / MathF.PI) - 180f;
            }

            if (Combat.ApplyPlanetAndPull(leech))
            {
                world.Emit(new EffectEvent(EffectKind.LeechPlanetImpact, leech.X, leech.Y));
            }
        }
    }

    private static void KeepInArena(Projectile bomb)
    {
        if (bomb.X + bomb.VelocityX > GameConstants.ArenaMaxX)
        {
            bomb.VelocityX -= BombBoundsPush;
        }

        if (bomb.Y + bomb.VelocityY > GameConstants.ArenaMaxY)
        {
            bomb.VelocityY -= BombBoundsPush;
        }

        if (bomb.X + bomb.VelocityX < GameConstants.ArenaMinX)
        {
            bomb.VelocityX += BombBoundsPush;
        }

        if (bomb.Y + bomb.VelocityY < GameConstants.ArenaMinY)
        {
            bomb.VelocityY += BombBoundsPush;
        }
    }

    // Fills free fragment slots with a ring of up to 30 fragments around the bomb.
    private static void Burst(Ship ship, World world, Projectile bomb)
    {
        var weapon = ship.Definition.Primary;
        world.PlaySound(ship, ShipSound.SpecialFire);
        bomb.Lifetime = 0;
        var remaining = FragmentCount;
        for (var k = 1; k < weapon.MaxProjectiles && remaining > 0; k++)
        {
            var fragment = ship.PrimaryProjectiles[k];
            if (fragment.Lifetime != 0)
            {
                continue;
            }

            remaining--;
            var direction = (float)remaining / FragmentCount * 2f * MathF.PI;
            fragment.X = bomb.X + (MathF.Cos(direction) * FragmentSpawnDistance);
            fragment.Y = bomb.Y + (MathF.Sin(direction) * FragmentSpawnDistance);
            fragment.VelocityX = MathF.Cos(direction) * FragmentSpeed;
            fragment.VelocityY = MathF.Sin(direction) * FragmentSpeed;
            fragment.Lifetime = FragmentLifetime;
            fragment.Energy = weapon.ProjectileEnergy / FragmentEnergyDivisor;
        }
    }

    // Takes fuel (shields do not help) and makes the leech ride along with the target.
    private static void Drain(Ship ship, World world, Projectile leech, Ship target, ref float dx, ref float dy)
    {
        world.PlaySound(ship, ShipSound.SpecialHit);
        target.Fuel = MathF.Max(0, target.Fuel - ship.Definition.Special.Damage);
        if (dx == 0)
        {
            dx = MinLeechOffset;
        }

        if (dy == 0)
        {
            dy = MinLeechOffset;
        }

        leech.VelocityX = target.VelocityX + (MathF.Sign(dx) * LeechGrip);
        leech.VelocityY = target.VelocityY + (MathF.Sign(dy) * LeechGrip);
    }
}
