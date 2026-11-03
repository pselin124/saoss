namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Stick Insect: fires a wide fan of 18 small shots in front of the ship; the special drops a bomb behind the ship
/// that bursts into 30 drifting fragments when it runs out.
/// </summary>
public sealed class StickInsectBehavior : ShipBehavior
{
    private const int FanShots = 18;
    private const float FanMuzzleDistance = 3.5f;
    private const float FanPositionOffset = 60f;
    private const float FanPositionWidth = 120f;
    private const float FanDirectionOffset = 45f;
    private const float FanDirectionWidth = 90f;
    private const float FanMinSpeedFactor = 0.3f;
    private const float BombMuzzleDistance = 1.3f;
    private const int Fragments = 30;
    private const float FragmentSpeedRoll = 35f;
    private const float FragmentMinSpeed = 0.15f;
    private const float JitterRoll = 100f;
    private const float JitterDivisor = 1000f;
    private const float JitterOffset = 0.05f;

    // Projectile.Red marks the bomb and Projectile.Green a fragment, as in the original (they also set the color).
    private const float Marked = 1f;

    /// <summary>
    /// Fills up to 18 free slots with a fan: the shots start spread 120 degrees wide around the nose, fly in
    /// directions 90 degrees wide, and the middle ones are fastest.
    /// </summary>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;
        world.PlaySound(ship, ShipSound.Fire);

        var count = FanShots;
        for (var i = 0; i < weapon.MaxProjectiles && count > 0; i++)
        {
            var shot = ship.PrimaryProjectiles[i];
            if (shot.IsActive)
            {
                continue;
            }

            var position = ToRadians(ship.Angle + FanPositionOffset - (count * FanPositionWidth / FanShots));
            var direction = ToRadians(ship.Angle + FanDirectionOffset - (count * FanDirectionWidth / FanShots));
            var speed = weapon.Speed * (MathF.Sin(count * MathF.PI / FanShots) + FanMinSpeedFactor);
            shot.Lifetime = weapon.Lifetime;
            shot.X = ship.X + (MathF.Cos(position) * ship.Definition.Radius * FanMuzzleDistance);
            shot.Y = ship.Y + (MathF.Sin(position) * ship.Definition.Radius * FanMuzzleDistance);
            shot.VelocityX = (MathF.Cos(direction) * speed) + ship.VelocityX;
            shot.VelocityY = (MathF.Sin(direction) * speed) + ship.VelocityY;
            shot.Angle = ship.Angle;
            shot.Energy = weapon.ProjectileEnergy;
            count--;
        }
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

        var backward = ToRadians(ship.Angle + 180f);
        bomb.X = ship.X + (MathF.Cos(backward) * ship.Definition.Radius * BombMuzzleDistance);
        bomb.Y = ship.Y + (MathF.Sin(backward) * ship.Definition.Radius * BombMuzzleDistance);
        bomb.VelocityX = (MathF.Cos(backward) * special.Speed) + ship.VelocityX;
        bomb.VelocityY = (MathF.Sin(backward) * special.Speed) + ship.VelocityY;
        bomb.Lifetime = special.Duration;
        bomb.Angle = 0;
        bomb.Energy = special.ProjectileEnergy;
        bomb.Red = Marked;
        bomb.Green = 0;
    }

    /// <summary>
    /// A projectile on its last tick vanishes, and a bomb bursts into fragments. The others move (fragments drift
    /// randomly), feel the planet and hit any ship they touch, including the Stick Insect itself.
    /// </summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var projectile = ship.SpecialProjectiles[i];
            if (projectile.Lifetime == 1)
            {
                projectile.Lifetime = 0;
                if (projectile.Red == Marked)
                {
                    Burst(ship, world, projectile);
                }

                continue;
            }

            if (!projectile.IsActive)
            {
                continue;
            }

            projectile.Lifetime--;
            if (projectile.Green == Marked)
            {
                projectile.VelocityX += Jitter(world);
                projectile.VelocityY += Jitter(world);
            }

            projectile.X += projectile.VelocityX;
            projectile.Y += projectile.VelocityY;
            Combat.ApplyPlanet(projectile);
            Combat.HitShipsWithSpecial(ship, world, projectile);
        }
    }

    // Fragments may land in later slots and then move in this same tick, as in the original.
    // The burst is aimed using the ship's current heading, not the bomb's.
    private static void Burst(Ship ship, World world, Projectile bomb)
    {
        var special = ship.Definition.Special;
        var count = Fragments;
        for (var j = 0; j < special.MaxProjectiles && count > 0; j++)
        {
            var fragment = ship.SpecialProjectiles[j];
            if (fragment.IsActive)
            {
                continue;
            }

            var direction = ToRadians(ship.Angle + (count * 360f / Fragments));
            fragment.X = bomb.X + (MathF.Cos(direction) * ship.Definition.Radius * BombMuzzleDistance);
            fragment.Y = bomb.Y + (MathF.Sin(direction) * ship.Definition.Radius * BombMuzzleDistance);
            fragment.VelocityX = (MathF.Cos(direction) * special.Speed * FragmentSpeed(world)) + bomb.VelocityX;
            fragment.VelocityY = (MathF.Sin(direction) * special.Speed * FragmentSpeed(world)) + bomb.VelocityY;
            fragment.Lifetime = special.Duration / 2;
            fragment.Angle = 0;
            fragment.Energy = special.ProjectileEnergy;
            fragment.Red = 0;
            fragment.Green = Marked;
            count--;
        }
    }

    private static float FragmentSpeed(World world) => (world.Random.Next(FragmentSpeedRoll) / 100f) + FragmentMinSpeed;

    private static float Jitter(World world) => (world.Random.Next(JitterRoll) / JitterDivisor) - JitterOffset;

    private static float ToRadians(float degrees) => degrees / 180f * MathF.PI;
}
