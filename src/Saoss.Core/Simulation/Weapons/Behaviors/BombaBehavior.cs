namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Bomba: lays slow mines that home in on the nearest enemy; the special zaps every enemy ship and breakable
/// enemy projectile close to the Bomba with a short cyan lightning bolt.
/// </summary>
public sealed class BombaBehavior : ShipBehavior
{
    private const float MuzzleDistance = 1.4f;
    private const float LaunchSpeedFactor = 150f;
    private const float MineDrag = 1f;

    // Without enemies the mines drift toward ship 0, scaled by this pseudo distance.
    private const float NoEnemyDistance = GameConstants.ArenaMaxX * 4f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var mine = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (mine == null)
        {
            return;
        }

        world.PlaySound(ship, ShipSound.Fire);
        var (headingX, headingY) = ship.Heading;
        var launchSpeed = ship.Definition.Thrust * LaunchSpeedFactor;
        mine.Lifetime = weapon.Lifetime;
        mine.X = ship.X + (headingX * ship.Definition.Radius * MuzzleDistance);
        mine.Y = ship.Y + (headingY * ship.Definition.Radius * MuzzleDistance);
        mine.VelocityX = (headingX * launchSpeed) + ship.VelocityX;
        mine.VelocityY = (headingY * launchSpeed) + ship.VelocityY;
        mine.Angle = ship.Angle;
        mine.Energy = weapon.ProjectileEnergy;
    }

    /// <summary>
    /// Per mine: move, planet, hit every ship it touches (the Bomba itself included), then accelerate toward the
    /// nearest enemy with drag and face it.
    /// </summary>
    public override void UpdatePrimaryProjectiles(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        for (var i = 0; i < weapon.MaxProjectiles; i++)
        {
            var mine = ship.PrimaryProjectiles[i];
            if (!mine.IsActive)
            {
                continue;
            }

            mine.Lifetime--;
            mine.X += mine.VelocityX;
            mine.Y += mine.VelocityY;
            Combat.ApplyPlanetAndPull(mine);

            foreach (var target in Combat.Targets(world))
            {
                if (!Combat.Touches(target, mine, weapon.Radius))
                {
                    continue;
                }

                world.PlaySound(ship, ShipSound.Hit);
                mine.Lifetime = 0;

                // The original applies this damage twice.
                target.Energy -= 2f * weapon.Damage;
                Combat.Knock(target, mine, weapon.Knockback);
            }

            Steer(ship, world, mine);
        }
    }

    /// <summary>
    /// Zaps enemy ships within the special radius (shields protect) and weakens breakable enemy projectiles in it.
    /// Fuel, cooldown and sound are only spent when something was zapped, so the special can be held until it hits.
    /// </summary>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        var zapped = false;
        foreach (var enemy in world.Ships.Where(s => s.IsInGame && s.Index != ship.Index && s.Team != ship.Team))
        {
            if (enemy.IsAlive && Distance(enemy.X - ship.X, enemy.Y - ship.Y) < special.Radius + (enemy.Definition.Radius * Combat.RadiusScale))
            {
                if (!Combat.IsShielded(enemy))
                {
                    enemy.Energy -= special.Damage;
                }

                AddBolt(ship, enemy.X, enemy.Y);
                zapped = true;
            }

            var enemyWeapons = enemy.Definition;
            if (enemyWeapons.Primary.CanBeDestroyed)
            {
                zapped |= ZapProjectiles(ship, enemy.PrimaryProjectiles, enemyWeapons.Primary.MaxProjectiles, enemyWeapons.Primary.Radius);
            }

            if (enemyWeapons.Special.CanBeDestroyed)
            {
                zapped |= ZapProjectiles(ship, enemy.SpecialProjectiles, enemyWeapons.Special.MaxProjectiles, enemyWeapons.Special.Radius);
            }
        }

        if (zapped)
        {
            ship.Fuel -= special.FuelCost;
            ship.SpecialCooldown = special.Cooldown;
            world.PlaySound(ship, ShipSound.SpecialFire);
        }
    }

    private static void Steer(Ship ship, World world, Projectile mine)
    {
        var nearest = Combat.EnemyTargets(world, ship)
            .MinBy(t => Distance(mine.X - t.X, mine.Y - t.Y));
        var distance = nearest == null ? NoEnemyDistance : Distance(mine.X - nearest.X, mine.Y - nearest.Y);
        var target = nearest ?? world.Ships[0];
        var dx = target.X - mine.X;
        var dy = target.Y - mine.Y;
        mine.VelocityX = ShipMovement.Drag(mine.VelocityX + (dx / distance * ship.Definition.Primary.Speed), MineDrag);
        mine.VelocityY = ShipMovement.Drag(mine.VelocityY + (dy / distance * ship.Definition.Primary.Speed), MineDrag);
        mine.Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI;
    }

    private static bool ZapProjectiles(Ship ship, Projectile[] pool, int count, float radius)
    {
        var special = ship.Definition.Special;
        var zapped = false;
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (!projectile.IsActive || Distance(projectile.X - ship.X, projectile.Y - ship.Y) >= special.Radius + (radius * Combat.RadiusScale))
            {
                continue;
            }

            projectile.Energy -= special.Damage;
            if (projectile.Energy < 0)
            {
                projectile.Energy = 0;
                projectile.Lifetime = 0;
            }

            AddBolt(ship, projectile.X, projectile.Y);
            zapped = true;
        }

        return zapped;
    }

    // The bolt keeps whatever velocity its beam slot had, like in the original.
    private static void AddBolt(Ship ship, float x, float y)
    {
        var beam = ship.Beams.FirstOrDefault(b => !b.IsActive);
        beam?.Set(x, y, ship.X, ship.Y, red: 0f, green: 1f, blue: 1f, ship.Definition.Special.Duration);
    }

    private static float Distance(float x, float y) => MathF.Sqrt((x * x) + (y * y));
}
