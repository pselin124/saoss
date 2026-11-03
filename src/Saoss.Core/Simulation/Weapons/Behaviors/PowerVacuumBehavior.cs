namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Power Vacuum: holding fire grows a green ball at the nose that gets stronger and redder; releasing fire launches
/// it. The special drops slime seekers behind the ship that chase the nearest enemy and slow it down with slime.
/// </summary>
public sealed class PowerVacuumBehavior : ShipBehavior
{
    private const float MuzzleDistance = 1.5f;
    private const float StartEnergy = 100f;
    private const float MaxEnergy = 2000f;
    private const float ColorShift = 0.001f;
    private const float DropDistance = 1.1f;
    private const float SeekerHitScale = 0.6f;

    /// <summary>
    /// Called while fire is held (to start or grow the ball in slot 0) and after release while the ball exists
    /// (to launch a copy of it from a free slot and remove the ball).
    /// </summary>
    public override void Fire(Ship ship, World world)
    {
        if (ship.Controls.Fire)
        {
            Charge(ship);
        }
        else
        {
            Release(ship, world);
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var seeker = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (seeker == null)
        {
            return;
        }

        ship.Fuel -= special.FuelCost;
        var behind = (ship.Angle + 180f) / 180f * MathF.PI;
        seeker.X = ship.X + (MathF.Cos(behind) * ship.Definition.Radius * DropDistance);
        seeker.Y = ship.Y + (MathF.Sin(behind) * ship.Definition.Radius * DropDistance);
        seeker.Lifetime = special.Duration;
        seeker.Energy = special.ProjectileEnergy;
        seeker.Angle = 0;
    }

    /// <summary>
    /// Per seeker: count down, vanish in the planet (no gravity), slime every enemy it touches, then step toward the
    /// nearest enemy. Without any enemy it stays put.
    /// </summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var seeker = ship.SpecialProjectiles[i];
            if (!seeker.IsActive)
            {
                continue;
            }

            seeker.Lifetime--;
            if (Combat.Square(seeker.X) + Combat.Square(seeker.Y) < Combat.PlanetRadiusSquared)
            {
                seeker.Lifetime = 0;
            }

            var nearest = float.MaxValue;
            var (awayX, awayY) = (0f, 0f);

            // Shields do not stop slime, and one seeker can slime several enemies in the same tick.
            foreach (var enemy in Combat.EnemyTargets(world, ship))
            {
                var dx = seeker.X - enemy.X;
                var dy = seeker.Y - enemy.Y;
                var distance = MathF.Sqrt((dx * dx) + (dy * dy));
                if (distance < nearest)
                {
                    nearest = distance;
                    (awayX, awayY) = (dx, dy);
                }

                if (distance < (special.Radius + enemy.Definition.Radius) * SeekerHitScale)
                {
                    seeker.Lifetime = 0;
                    enemy.AddSlime(world.Random);
                    world.PlaySound(ship, ShipSound.SpecialHit);
                }
            }

            if (nearest < float.MaxValue && nearest > 0)
            {
                seeker.X -= awayX / nearest * special.Speed;
                seeker.Y -= awayY / nearest * special.Speed;
            }
        }
    }

    private static void Charge(Ship ship)
    {
        var ball = ship.PrimaryProjectiles[0];
        var (headingX, headingY) = ship.Heading;
        var radius = ship.Definition.Radius;
        if (!ball.IsActive)
        {
            ball.Energy = StartEnergy;
            ball.Red = 0;
            ball.Green = 1f;
            ball.Blue = 0;
        }
        else
        {
            if (ball.Energy < MaxEnergy)
            {
                ball.Energy++;
            }

            if (ball.Red < 1f)
            {
                ball.Red += ColorShift;
            }

            if (ball.Green > 0)
            {
                ball.Green -= ColorShift;
            }
        }

        ball.Lifetime = ship.Definition.Primary.Lifetime;
        ball.X = ship.X + (headingX * radius * MuzzleDistance);
        ball.Y = ship.Y + (headingY * radius * MuzzleDistance);
        ball.VelocityX = 0;
        ball.VelocityY = 0;
    }

    private static void Release(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        var ball = ship.PrimaryProjectiles[0];
        if (ball.IsActive)
        {
            world.PlaySound(ship, ShipSound.Fire);
        }

        // Slot 0 is the ball itself; if no other slot is free the ball stays until one frees up.
        for (var i = 1; i < weapon.MaxProjectiles; i++)
        {
            var shot = ship.PrimaryProjectiles[i];
            if (shot.IsActive)
            {
                continue;
            }

            var (headingX, headingY) = ship.Heading;
            ship.FireCooldown = weapon.FireDelay;
            shot.Lifetime = weapon.Lifetime;
            shot.X = ship.X + (headingX * ship.Definition.Radius * MuzzleDistance);
            shot.Y = ship.Y + (headingY * ship.Definition.Radius * MuzzleDistance);
            shot.VelocityX = (headingX * weapon.Speed) + ship.VelocityX;
            shot.VelocityY = (headingY * weapon.Speed) + ship.VelocityY;
            shot.Energy = ball.Energy;
            shot.Red = ball.Red;
            shot.Green = ball.Green;
            shot.Blue = ball.Blue;
            ball.Lifetime = 0;
            return;
        }
    }
}
