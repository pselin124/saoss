namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Trek: fires big slow shots with a little recoil; the special launches two drones (each costs one
/// energy) that hover next to the nearest enemy zapping it with a laser, then fly back to return the energy.
/// </summary>
public sealed class TrekBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.6f;
    private const float Recoil = 0.1f;
    private const float LaunchAngle = 160f;
    private const float LaunchAngleStep = 40f;
    private const float LaunchDistance = 1.1f;
    private const int DronesPerLaunch = 2;
    private const float DroneAngle = 90f;
    private const float DroneRemoveDistance = 0.8f;
    private const float DroneMinGap = 1.2f;
    private const float DroneMaxGap = 1.5f;
    private const int LaserDelay = 30;
    private const int LaserBeamLifetime = 2;

    // Distance used when no enemy was found, so the drone simply approaches its fallback target.
    private const float NoEnemyDistance = (GameConstants.ArenaMaxX * 2f) + (GameConstants.ArenaMaxY * 2f);

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (shot != null)
        {
            world.PlaySound(ship, ShipSound.Fire);
            Combat.Launch(shot, ship, 0f, MuzzleDistance);
        }

        var (headingX, headingY) = ship.Heading;
        ship.VelocityX -= Recoil * headingX;
        ship.VelocityY -= Recoil * headingY;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.SpecialFire);
        for (var side = 0; side < DronesPerLaunch; side++)
        {
            // The ship keeps at least one energy for itself.
            if (ship.Energy <= DronesPerLaunch - side)
            {
                continue;
            }

            var drone = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
            if (drone == null)
            {
                continue;
            }

            ship.Energy--;
            var direction = (ship.Angle + LaunchAngle + (LaunchAngleStep * side)) / 180f * MathF.PI;
            drone.Angle = 0;
            drone.Energy = special.ProjectileEnergy;
            drone.X = ship.X + (MathF.Cos(direction) * ship.Definition.Radius * LaunchDistance);
            drone.Y = ship.Y + (MathF.Sin(direction) * ship.Definition.Radius * LaunchDistance);
            drone.Lifetime = special.Duration;
            ship.Fuel -= special.FuelCost / 2f;
            ship.SpecialCooldown = special.Cooldown;
        }
    }

    /// <inheritdoc/>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var drone = ship.SpecialProjectiles[i];
            if (!drone.IsActive)
            {
                continue;
            }

            drone.Lifetime--;
            if (Combat.Square(drone.X) + Combat.Square(drone.Y) < Combat.PlanetRadiusSquared)
            {
                drone.Lifetime = 0;
            }

            if (drone.Delay > 0)
            {
                drone.Delay--;
            }

            if (drone.Lifetime < special.Duration / 3f)
            {
                ReturnToShip(ship, drone);
            }
            else
            {
                Hunt(ship, world, drone);
            }
        }
    }

    // Flies home; reaching the ship gives back the energy the drone cost.
    private static void ReturnToShip(Ship ship, Projectile drone)
    {
        MoveToward(drone, ship.X, ship.Y, ship.Definition.Special.Speed);
        if (Combat.Square(ship.X - drone.X) + Combat.Square(ship.Y - drone.Y) < Combat.Square(ship.Definition.Radius))
        {
            drone.Lifetime = 0;
            ship.Energy++;
        }
    }

    // Keeps a small gap to the nearest enemy and zaps it every 30 ticks while inside that gap.
    private static void Hunt(Ship ship, World world, Projectile drone)
    {
        var special = ship.Definition.Special;
        var target = Combat.EnemyTargets(world, ship)
            .MinBy(t => Combat.Square(drone.X - t.X) + Combat.Square(drone.Y - t.Y));
        var distance = target == null
            ? NoEnemyDistance
            : MathF.Sqrt(Combat.Square(drone.X - target.X) + Combat.Square(drone.Y - target.Y));

        // Without enemies the original falls back to player 0, or player 1 for player 0 itself.
        target ??= FallbackTarget(ship, world);
        drone.Angle = DroneAngle;
        if (target == null)
        {
            return;
        }

        var targetRadius = target.Definition.Radius;
        if (distance < DroneRemoveDistance)
        {
            drone.Lifetime = 0;
        }
        else if (distance < targetRadius + DroneMinGap)
        {
            MoveToward(drone, target.X, target.Y, -special.Speed);
        }
        else if (distance > targetRadius + DroneMaxGap)
        {
            MoveToward(drone, target.X, target.Y, special.Speed);
        }
        else if (distance < targetRadius + DroneMaxGap && drone.Delay <= 0)
        {
            drone.Delay = LaserDelay;
            if (!Combat.IsShielded(target))
            {
                world.PlaySound(ship, ShipSound.SpecialHit);
                target.Energy -= special.Damage;
            }

            AddLaserBeam(ship, drone, target);
        }
    }

    private static Ship? FallbackTarget(Ship ship, World world)
    {
        var index = ship.Index == 0 ? 1 : 0;
        return index < world.Ships.Count ? world.Ships[index] : null;
    }

    private static void MoveToward(Projectile drone, float x, float y, float step)
    {
        var dx = x - drone.X;
        var dy = y - drone.Y;
        var distance = MathF.Sqrt((dx * dx) + (dy * dy));
        drone.X += dx / distance * step;
        drone.Y += dy / distance * step;
    }

    private static void AddLaserBeam(Ship ship, Projectile drone, Ship target)
    {
        var beam = ship.Beams.FirstOrDefault(b => !b.IsActive);
        if (beam == null)
        {
            return;
        }

        beam.X1 = drone.X;
        beam.Y1 = drone.Y;
        beam.X2 = target.X;
        beam.Y2 = target.Y;
        beam.Red = 1f;
        beam.Green = 1f;
        beam.VelocityX = 0;
        beam.VelocityY = 0;
        beam.Lifetime = LaserBeamLifetime;
    }
}
