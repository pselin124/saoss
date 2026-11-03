namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Peace Cannon: a continuous green laser beam that burns every ship and projectile along it; the special drops
/// slow black holes that pull in ships and projectiles and hurt whatever they touch.
/// </summary>
public sealed class PeaceCannonBehavior : ShipBehavior
{
    private const float BeamStartDistance = 1.6f;
    private const float DropDistance = 1.1f;
    private const float BoundsPush = 0.02f;
    private const float HoleGravity = 0.08f;
    private const float HoleGravityRange = 40f;
    private const float HoleProjectileGravityFactor = 5f;

    /// <summary>
    /// Fires the beam every tick while held (there is no fire delay). Steps: start the looping sound, pay, place the
    /// beam, then damage every other ship and weaken every projectile of other ships that a beam sample touches.
    /// </summary>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        if (!ship.IsFireLoopPlaying)
        {
            world.PlaySound(ship, ShipSound.Fire, loop: true);
            ship.IsFireLoopPlaying = true;
        }

        ship.Fuel -= weapon.FuelCost;
        var (headingX, headingY) = ship.Heading;
        var start = ship.Definition.Radius * BeamStartDistance;
        var beam = ship.Beams[0];
        beam.Set(
            ship.X + (start * headingX) + ship.VelocityX,
            ship.Y + (start * headingY) + ship.VelocityY,
            ship.X + (weapon.Radius * headingX) + ship.VelocityX,
            ship.Y + (weapon.Radius * headingY) + ship.VelocityY,
            red: 0f,
            green: 1f,
            blue: 0f,
            weapon.Lifetime);
        beam.VelocityX = 0;
        beam.VelocityY = 0;

        // No team, shield or "can be destroyed" checks: the beam burns everything of every other ship.
        var backward = MathF.Atan2(beam.Y1 - beam.Y2, beam.X1 - beam.X2);
        foreach (var other in world.Ships.Where(s => s.IsInGame && s.Index != ship.Index))
        {
            if (other.IsAlive && Combat.BeamCrosses(beam, backward, weapon.Radius, other.X, other.Y, other.Definition.Radius))
            {
                other.Energy -= weapon.Damage;
            }

            var otherWeapons = other.Definition;
            Burn(ship, beam, backward, other.PrimaryProjectiles, otherWeapons.Primary.MaxProjectiles, otherWeapons.Primary.Radius);
            Burn(ship, beam, backward, other.SpecialProjectiles, otherWeapons.Special.MaxProjectiles, otherWeapons.Special.Radius);
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var hole = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (hole == null)
        {
            return;
        }

        var behind = (ship.Angle + 180f) / 180f * MathF.PI;
        hole.X = ship.X + (MathF.Cos(behind) * ship.Definition.Radius * DropDistance);
        hole.Y = ship.Y + (MathF.Sin(behind) * ship.Definition.Radius * DropDistance);
        hole.VelocityX = MathF.Cos(behind) * special.Speed;
        hole.VelocityY = MathF.Sin(behind) * special.Speed;
        hole.Lifetime = special.Duration;
        hole.Angle = 0;
        hole.Energy = special.ProjectileEnergy;
    }

    /// <summary>
    /// Per hole: move, push back inside the arena, vanish in the planet (no gravity), then for every other ship pull
    /// and hurt the ship and pull and weaken all of its projectiles. No team or shield checks.
    /// </summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var hole = ship.SpecialProjectiles[i];
            if (!hole.IsActive)
            {
                continue;
            }

            hole.Lifetime--;
            hole.X += hole.VelocityX;
            hole.Y += hole.VelocityY;
            hole.VelocityX += hole.X > GameConstants.ArenaMaxX ? -BoundsPush : hole.X < GameConstants.ArenaMinX ? BoundsPush : 0;
            hole.VelocityY += hole.Y > GameConstants.ArenaMaxY ? -BoundsPush : hole.Y < GameConstants.ArenaMinY ? BoundsPush : 0;
            if (Combat.Square(hole.X) + Combat.Square(hole.Y) < Combat.PlanetRadiusSquared)
            {
                hole.Lifetime = 0;
            }

            foreach (var other in world.Ships.Where(s => s.IsInGame && s.Index != ship.Index))
            {
                if (other.IsAlive)
                {
                    PullShip(ship, hole, other);
                }

                var otherWeapons = other.Definition;
                PullProjectiles(ship, hole, other.PrimaryProjectiles, otherWeapons.Primary.MaxProjectiles, otherWeapons.Primary.Radius);
                PullProjectiles(ship, hole, other.SpecialProjectiles, otherWeapons.Special.MaxProjectiles, otherWeapons.Special.Radius);
            }
        }
    }

    private static void Burn(Ship ship, Beam beam, float backward, Projectile[] pool, int count, float radius)
    {
        var weapon = ship.Definition.Primary;
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (projectile.IsActive && Combat.BeamCrosses(beam, backward, weapon.Radius, projectile.X, projectile.Y, radius))
            {
                Weaken(projectile, weapon.Damage);
            }
        }
    }

    private static void PullShip(Ship cannon, Projectile hole, Ship target)
    {
        var special = cannon.Definition.Special;
        var dx = target.X - hole.X;
        var dy = target.Y - hole.Y;
        var distance = MathF.Sqrt((dx * dx) + (dy * dy));
        if (distance < HoleGravityRange)
        {
            var pull = HoleGravity * cannon.Definition.Mass / target.Definition.Mass / (distance * distance);
            target.VelocityX -= dx * pull;
            target.VelocityY -= dy * pull;
        }

        if (distance < (special.Radius + target.Definition.Radius) * Combat.RadiusScale)
        {
            target.Energy -= special.Damage;
        }
    }

    private static void PullProjectiles(Ship cannon, Projectile hole, Projectile[] pool, int count, float radius)
    {
        var special = cannon.Definition.Special;
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (!projectile.IsActive)
            {
                continue;
            }

            var dx = projectile.X - hole.X;
            var dy = projectile.Y - hole.Y;
            var distance = MathF.Sqrt((dx * dx) + (dy * dy));
            if (distance < HoleGravityRange)
            {
                var pull = HoleGravity * HoleProjectileGravityFactor / (distance * distance);
                projectile.VelocityX -= dx * pull;
                projectile.VelocityY -= dy * pull;
            }

            if (distance < (special.Radius + radius) * Combat.RadiusScale)
            {
                Weaken(projectile, special.Damage);
            }
        }
    }

    private static void Weaken(Projectile projectile, float damage)
    {
        projectile.Energy -= damage;
        if (projectile.Energy < 0)
        {
            projectile.Energy = 0;
            projectile.Lifetime = 0;
        }
    }
}
