namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Rayska: sprays a rapid stream of small shots with random spread; the special drops long-lived mines behind
/// the ship that drift, bounce off the arena edges and knock back any ship they hit.
/// </summary>
public sealed class RayskaBehavior : ShipBehavior
{
    private const float MaxSpread = 20f;
    private const float SpreadRoll = 10000f;
    private const float SpreadDivisor = 250f;
    private const float MuzzleDistance = 0.8f;
    private const float DropDistance = 1.1f;
    private const float BoundsPush = 0.02f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;
        var spread = (world.Random.Next(SpreadRoll) / SpreadDivisor) - MaxSpread;

        var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (shot == null)
        {
            return;
        }

        world.PlaySound(ship, ShipSound.Fire);
        var direction = (ship.Angle + spread) / 180f * MathF.PI;
        shot.Lifetime = weapon.Lifetime;
        shot.X = ship.X + (MathF.Cos(direction) * ship.Definition.Radius * MuzzleDistance);
        shot.Y = ship.Y + (MathF.Sin(direction) * ship.Definition.Radius * MuzzleDistance);
        shot.VelocityX = (MathF.Cos(direction) * weapon.Speed) + ship.VelocityX;
        shot.VelocityY = (MathF.Sin(direction) * weapon.Speed) + ship.VelocityY;
        shot.Angle = ship.Angle;

        // The original gives the shot the special projectile's energy.
        shot.Energy = ship.Definition.Special.ProjectileEnergy;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var mine = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (mine == null)
        {
            return;
        }

        var behind = (ship.Angle + 180f) / 180f * MathF.PI;
        mine.X = ship.X + (MathF.Cos(behind) * ship.Definition.Radius * DropDistance);
        mine.Y = ship.Y + (MathF.Sin(behind) * ship.Definition.Radius * DropDistance);
        mine.VelocityX = (MathF.Cos(behind) * special.Speed) + ship.VelocityX;
        mine.VelocityY = (MathF.Sin(behind) * special.Speed) + ship.VelocityY;
        mine.Lifetime = special.Duration;
        mine.Angle = 0;
        mine.Energy = special.ProjectileEnergy;
    }

    /// <summary>
    /// Per mine: push back inside the arena, move, face the flight direction, planet, then hit every ship it
    /// touches (the Rayska itself included).
    /// </summary>
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
            KeepInArena(mine);
            mine.X += mine.VelocityX;
            mine.Y += mine.VelocityY;
            mine.Angle = MathF.Atan2(mine.VelocityY, mine.VelocityX) * 180f / MathF.PI;
            Combat.ApplyPlanetAndPull(mine);

            foreach (var target in Combat.Targets(world))
            {
                if (!Combat.Touches(target, mine, ship.Definition.Primary.Radius))
                {
                    continue;
                }

                world.PlaySound(ship, ShipSound.SpecialHit);
                mine.Lifetime = 0;
                world.Emit(new EffectEvent(EffectKind.DriftMineImpact, target.X, target.Y, mine.VelocityX, mine.VelocityY));
                target.Energy -= special.Damage;
                Combat.Knock(target, mine, special.Knockback);
            }
        }
    }

    private static void KeepInArena(Projectile mine)
    {
        mine.VelocityX += mine.X > GameConstants.ArenaMaxX ? -BoundsPush : mine.X < GameConstants.ArenaMinX ? BoundsPush : 0;
        mine.VelocityY += mine.Y > GameConstants.ArenaMaxY ? -BoundsPush : mine.Y < GameConstants.ArenaMinY ? BoundsPush : 0;
    }
}
