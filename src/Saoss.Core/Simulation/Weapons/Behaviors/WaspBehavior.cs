namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Wasp: fires a fast stream of small shots; the special blows the Wasp up, damaging nearby ships and
/// destroying nearby breakable projectiles.
/// </summary>
public sealed class WaspBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.6f;
    private const float ShipBlastRadius = 20f;
    private const float ShipDamageFalloff = 4.5f;
    private const float ProjectileBlastRadius = 12f;
    private const float ProjectileDamageFalloff = 6f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;
        world.PlaySound(ship, ShipSound.Fire);

        var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (shot != null)
        {
            Combat.Launch(shot, ship, 0f, MuzzleDistance);
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.SpecialCooldown = special.Cooldown;

        // The explosion and its sound come from the normal death handling.
        ship.Energy = 0;
        ship.Fuel -= special.FuelCost;
        BlastShips(ship, world);

        // Breakable projectiles of every ship are weakened, including the Wasp's own.
        foreach (var owner in world.Ships.Where(s => s.IsInGame))
        {
            if (owner.Definition.Primary.CanBeDestroyed)
            {
                BlastProjectiles(ship, owner.PrimaryProjectiles, owner.Definition.Primary.MaxProjectiles);
            }

            if (owner.Definition.Special.CanBeDestroyed)
            {
                BlastProjectiles(ship, owner.SpecialProjectiles, owner.Definition.Special.MaxProjectiles);
            }
        }
    }

    // No team check: the blast hurts teammates too.
    private static void BlastShips(Ship wasp, World world)
    {
        foreach (var target in Combat.OtherTargets(world, wasp))
        {
            var distance = Distance(target.X - wasp.X, target.Y - wasp.Y);
            if (distance < ShipBlastRadius && !Combat.IsShielded(target))
            {
                target.Energy -= wasp.Definition.Special.Damage - (distance * ShipDamageFalloff);
            }
        }
    }

    private static void BlastProjectiles(Ship wasp, Projectile[] pool, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (!projectile.IsActive)
            {
                continue;
            }

            var distance = Distance(projectile.X - wasp.X, projectile.Y - wasp.Y);
            if (distance < ProjectileBlastRadius)
            {
                projectile.Energy -= wasp.Definition.Special.Damage - (distance * ProjectileDamageFalloff);
                if (projectile.Energy <= 0)
                {
                    projectile.Lifetime = 0;
                }
            }
        }
    }

    private static float Distance(float x, float y) => MathF.Sqrt((x * x) + (y * y));
}
