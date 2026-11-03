namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Space Turtle: fires single wobbling shots that hit hard; the special sprays a spinning ring of small shots, one
/// per tick, all around the ship while it cools down.
/// </summary>
public sealed class SpaceTurtleBehavior : ShipBehavior
{
    private const float MuzzleDistance = 1.6f;
    private const int RingActive = 3;
    private const int RingDone = 1;
    private const int RingShotLifetime = 100;
    private const float RingRadius = 6f;
    private const float AngleScale = 100f;
    private const float WobblePeriod = 10f;
    private const float WobbleStrength = 0.03f;

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
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        ship.FireSide = RingActive;
    }

    /// <summary>
    /// While the special cools down, fires one ring shot per tick; the shot's direction turns a full circle over the
    /// cooldown, starting from the heading. Each tick overwrites the slot numbered by the elapsed cooldown.
    /// </summary>
    public override void BeforePositionUpdate(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        if (ship.SpecialCooldown <= 0 || ship.FireSide != RingActive)
        {
            return;
        }

        if (ship.SpecialCooldown == 1)
        {
            ship.FireSide = RingDone;
        }

        world.PlaySound(ship, ShipSound.SpecialFire);
        var slot = special.Cooldown - ship.SpecialCooldown;
        var direction = ((float)slot / special.Cooldown * 2f * MathF.PI) + (ship.Angle / 180f * MathF.PI);
        var shot = ship.SpecialProjectiles[slot];
        shot.Lifetime = RingShotLifetime;
        shot.X = (MathF.Cos(direction) * RingRadius) + ship.X;
        shot.Y = (MathF.Sin(direction) * RingRadius) + ship.Y;
        shot.VelocityX = (MathF.Cos(direction) * special.Speed) + ship.VelocityX;
        shot.VelocityY = (MathF.Sin(direction) * special.Speed) + ship.VelocityY;
        shot.Energy = special.ProjectileEnergy;
        shot.Angle = MathF.Atan2(shot.VelocityY * AngleScale, shot.VelocityX * AngleScale) * 180f / MathF.PI;
    }

    /// <summary>
    /// Moves the shots, applies the planet, hits other ships and makes each shot wobble sideways over its life.
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
                if (Combat.Touches(target, shot, weapon.Radius))
                {
                    world.PlaySound(ship, ShipSound.Hit);
                    shot.Lifetime = 0;

                    // The original subtracted the damage twice.
                    target.Energy -= weapon.Damage * 2f;
                    Combat.Knock(target, shot, weapon.Knockback);
                }
            }

            Wobble(shot, weapon.Lifetime);
        }
    }

    /// <inheritdoc/>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var shot = ship.SpecialProjectiles[i];
            if (!shot.IsActive)
            {
                continue;
            }

            shot.Lifetime--;
            shot.X += shot.VelocityX;
            shot.Y += shot.VelocityY;
            Combat.ApplyPlanet(shot);
            Combat.HitShipsWithSpecial(ship, world, shot);
        }
    }

    // The shot angle starts in degrees but is used as radians here, and the second cosine divides only the remaining
    // lifetime by 10 (operator precedence in the original); both kept as they shape the wobble.
    private static void Wobble(Projectile shot, int lifetime)
    {
        shot.Angle += MathF.Cos((lifetime - shot.Lifetime) / WobblePeriod);
        var strength = MathF.Cos(lifetime - (shot.Lifetime / WobblePeriod)) * WobbleStrength;
        shot.VelocityX += MathF.Cos(shot.Angle + (MathF.PI / 2f)) * strength;
        shot.VelocityY += MathF.Sin(shot.Angle + (MathF.PI / 2f)) * strength;
    }
}
