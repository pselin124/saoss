using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Weapons;

/// <summary>
/// Weapon behavior of one ship type. Each hook matches one function of the original <c>Player</c> class.
/// The defaults are what most ships share: no fire, no special, standard primary projectiles and no
/// special projectiles.
/// </summary>
public class ShipBehavior
{
    /// <summary>Fires the primary weapon; cooldown and fuel were already checked [ammu].</summary>
    public virtual void Fire(Ship ship, World world)
    {
    }

    /// <summary>Activates the special; cooldown and fuel were already checked [ease].</summary>
    public virtual void ActivateSpecial(Ship ship, World world)
    {
    }

    /// <summary>Per-tick special effects that run during movement, before the position update.</summary>
    public virtual void BeforePositionUpdate(Ship ship, World world)
    {
    }

    /// <summary>Runs on the tick a timed special (<see cref="Ship.SpecialTimeLeft"/>) runs out.</summary>
    public virtual void EndSpecial(Ship ship, World world)
    {
    }

    /// <summary>Moves primary projectiles and resolves their hits [liikutaPadit].</summary>
    public virtual void UpdatePrimaryProjectiles(Ship ship, World world) => UpdateStandardPrimary(ship, world);

    /// <summary>Moves special projectiles and resolves their hits [liikutaePadit].</summary>
    public virtual void UpdateSpecialProjectiles(Ship ship, World world)
    {
    }

    /// <summary>
    /// The common primary projectile update (the original's default branch): move, planet, and hit
    /// other ships. A hit removes the projectile, damages and pushes the target and plays the hit sound.
    /// </summary>
    protected static void UpdateStandardPrimary(Ship ship, World world)
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
            if (ship.Type == ShipType.StarWorm)
            {
                projectile.Angle += MathF.PI / 45f;
            }

            Combat.ApplyPlanet(projectile);
            foreach (var target in Combat.OtherTargets(world, ship))
            {
                if (!Combat.Touches(target, projectile, weapon.Radius))
                {
                    continue;
                }

                world.PlaySound(ship, ShipSound.Hit);
                projectile.Lifetime = 0;
                if (i == 0 && ship.Type == ShipType.PowerVacuum)
                {
                    ship.FireCooldown = weapon.FireDelay;
                }

                if (ship.Type is ShipType.Tringu or ShipType.Trek)
                {
                    world.Emit(new EffectEvent(EffectKind.HeavyShotImpact, target.X, target.Y, projectile.VelocityX, projectile.VelocityY));
                }

                // The Power Vacuum's shot grows while held, so its damage scales with the shot's energy.
                target.Energy -= ship.Type == ShipType.PowerVacuum
                    ? weapon.Damage / 2000f * projectile.Energy
                    : weapon.Damage;
                Combat.Knock(target, projectile, weapon.Knockback);
            }
        }
    }
}
