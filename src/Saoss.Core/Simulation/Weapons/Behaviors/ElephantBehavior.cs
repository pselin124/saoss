namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Elephant: fires wobbling shots alternately from both sides of the nose; the special drops a cluster bomb
/// behind the ship that bursts into a ring of 20 bomblets, each of which bursts again into 15 more.
/// </summary>
public sealed class ElephantBehavior : ShipBehavior
{
    private const float SideAngle = 26f;
    private const float MuzzleDistance = 1.5f;
    private const float WobbleRadians = MathF.PI / 180f * 2f;
    private const float WobbleRoll = 2f;
    private const float BombDropDistance = 1.1f;
    private const float BombHitBox = 3f;
    private const int FirstBurstCount = 20;
    private const float FirstBurstSpeed = 0.4f;
    private const int SecondBurstCount = 15;
    private const float SecondBurstSpeed = 0.3f;
    private const float BurstRadius = 3.5f;

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
        Combat.Launch(shot, ship, SideAngle * ship.FireSide, MuzzleDistance);
        ship.FireSide *= -1;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var bomb = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (bomb == null)
        {
            return;
        }

        var backward = (ship.Angle + 180f) / 180f * MathF.PI;
        bomb.X = ship.X + (MathF.Cos(backward) * ship.Definition.Radius * BombDropDistance);
        bomb.Y = ship.Y + (MathF.Sin(backward) * ship.Definition.Radius * BombDropDistance);
        bomb.VelocityX = MathF.Cos(backward) * special.Speed;
        bomb.VelocityY = MathF.Sin(backward) * special.Speed;
        bomb.Lifetime = special.Duration;
        bomb.Red = 1;
        bomb.Green = 0;
        bomb.Blue = 0;
        bomb.Angle = 0;
        bomb.Energy = special.ProjectileEnergy;
    }

    /// <summary>
    /// Per shot: count down, move, planet, hit other ships, then turn the shot randomly 2 degrees left or right.
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
            shot.X += shot.VelocityX;
            shot.Y += shot.VelocityY;
            Combat.ApplyPlanetAndPull(shot);

            foreach (var target in Combat.OtherTargets(world, ship))
            {
                if (!Combat.Touches(target, shot, weapon.Radius))
                {
                    continue;
                }

                world.PlaySound(ship, ShipSound.Hit);
                shot.Lifetime = 0;

                // The original subtracts the damage twice.
                target.Energy -= weapon.Damage * 2f;
                Combat.Knock(target, shot, weapon.Knockback);
            }

            Wobble(shot, world);
        }
    }

    /// <summary>
    /// Per bomb or bomblet: one with exactly one tick left bursts (red bomb into green bomblets, green into blue,
    /// blue just disappears); others count down, move and hit any ship including the Elephant itself.
    /// Planet effects were disabled in the original.
    /// </summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;

        // Bursts fill free slots from the start of the pool, continuing where the previous burst this tick stopped.
        var nextSlot = 0;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var bomb = ship.SpecialProjectiles[i];
            if (bomb.Lifetime == 1)
            {
                bomb.Lifetime = 0;
                if (bomb.Red == 1)
                {
                    Burst(ship, bomb, FirstBurstCount, FirstBurstSpeed, green: 1, blue: 0, ref nextSlot);
                }
                else if (bomb.Green == 1)
                {
                    Burst(ship, bomb, SecondBurstCount, SecondBurstSpeed, green: 0, blue: 1, ref nextSlot);
                }
            }
            else if (bomb.IsActive)
            {
                bomb.Lifetime--;
                bomb.X += bomb.VelocityX;
                bomb.Y += bomb.VelocityY;
                HitShips(ship, world, bomb);
            }
        }
    }

    private static void Wobble(Projectile shot, World world)
    {
        var speed = MathF.Sqrt((shot.VelocityX * shot.VelocityX) + (shot.VelocityY * shot.VelocityY));
        var heading = MathF.Atan2(shot.VelocityY, shot.VelocityX);
        heading += (int)world.Random.Next(WobbleRoll) == 1 ? WobbleRadians : -WobbleRadians;
        shot.VelocityX = MathF.Cos(heading) * speed;
        shot.VelocityY = MathF.Sin(heading) * speed;
        shot.Angle = MathF.Atan2(shot.VelocityY, shot.VelocityX) * 180f / MathF.PI;
    }

    // Spawns a ring of bomblets around the bursting one, moving outward.
    private static void Burst(Ship ship, Projectile source, int count, float speed, float green, float blue, ref int nextSlot)
    {
        var special = ship.Definition.Special;

        // The original only checked the remaining count here and could run past the pool; we stop at its end.
        var remaining = count;
        for (; nextSlot < special.MaxProjectiles && remaining > 0; nextSlot++)
        {
            var bomblet = ship.SpecialProjectiles[nextSlot];
            if (bomblet.IsActive)
            {
                continue;
            }

            remaining--;
            var direction = (float)remaining / count * 2f * MathF.PI;
            bomblet.X = source.X + (MathF.Cos(direction) * BurstRadius);
            bomblet.Y = source.Y + (MathF.Sin(direction) * BurstRadius);
            bomblet.VelocityX = MathF.Cos(direction) * speed;
            bomblet.VelocityY = MathF.Sin(direction) * speed;
            bomblet.Lifetime = special.Duration / 2;
            bomblet.Red = 0;
            bomblet.Green = green;
            bomblet.Blue = blue;

            // Energy is not reset: a bomblet keeps whatever the slot held before, as in the original.
        }
    }

    // Damages every ship the bomb touches, the Elephant included (the original had no shooter check).
    private static void HitShips(Ship ship, World world, Projectile bomb)
    {
        var special = ship.Definition.Special;
        foreach (var target in Combat.Targets(world))
        {
            // The original's box test is signed, not absolute: any ship far left of or below the bomb passes it.
            if (target.X - bomb.X >= BombHitBox || target.Y - bomb.Y >= BombHitBox)
            {
                continue;
            }

            if (!Combat.Touches(target, bomb, ship.Definition.Primary.Radius))
            {
                continue;
            }

            world.PlaySound(ship, ShipSound.SpecialHit);
            bomb.Lifetime = 0;
            target.Energy -= special.Damage;
        }
    }
}
