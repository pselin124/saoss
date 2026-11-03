namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Wiiii!: drops long-lived mines around itself that wander randomly; the special switches on a fast boost mode
/// (handled in movement) that lasts until fuel runs out and blocks firing.
/// </summary>
public sealed class WiiiiBehavior : ShipBehavior
{
    private const float DropSpreadRoll = 800f;
    private const float DropSpreadDivisor = 400f;
    private const float DropOffset = 1f;
    private const float WanderRoll = 100f;
    private const float WanderDivisor = 100f;

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
        mine.Lifetime = weapon.Lifetime;
        mine.X = (world.Random.Next(DropSpreadRoll) / DropSpreadDivisor) - DropOffset + ship.X;
        mine.Y = (world.Random.Next(DropSpreadRoll) / DropSpreadDivisor) - DropOffset + ship.Y;
        mine.VelocityX = 0;
        mine.VelocityY = 0;
        mine.Angle = ship.Angle;
        mine.Energy = weapon.ProjectileEnergy;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        if (ship.IsShieldActive)
        {
            return;
        }

        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        ship.IsShieldActive = true;
    }

    /// <summary>
    /// Per mine: count down, take a random step, planet (gravity only changes the unused velocity), then hit other
    /// ships.
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
            mine.X += (world.Random.Next(WanderRoll) / WanderDivisor * weapon.Speed) - (weapon.Speed / 2f);
            mine.Y += (world.Random.Next(WanderRoll) / WanderDivisor * weapon.Speed) - (weapon.Speed / 2f);
            Combat.ApplyPlanetAndPull(mine);

            foreach (var target in Combat.OtherTargets(world, ship))
            {
                if (!Combat.Touches(target, mine, weapon.Radius))
                {
                    continue;
                }

                world.PlaySound(ship, ShipSound.Hit);

                // The original subtracts the damage twice.
                target.Energy -= weapon.Damage * 2f;
                mine.Lifetime = 0;
                Combat.Knock(target, mine, weapon.Knockback);
            }
        }
    }
}
