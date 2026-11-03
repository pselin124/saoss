namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Bloddu: fires single straight shots; the special bites hit points off nearby enemies, the closer the more,
/// and each bitten point drops out as a health pickup that any ship can collect.
/// </summary>
public sealed class BlodduBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.8f;
    private const float BiteChance = 75f;
    private const float PercentRoll = 100f;
    private const float MinEnergyLeft = 1f;
    private const float PickupSpreadRoll = 300f;
    private const float PickupSpreadDivisor = 100f;
    private const float PickupOffset = 0.625f;
    private const int PickupDelay = 70;

    // One bite chance for each of these distances the enemy is closer than.
    private static readonly float[] BiteRanges = [23f, 19f, 15f, 11f];

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
        var (headingX, headingY) = ship.Heading;
        shot.Lifetime = weapon.Lifetime;
        shot.X = ship.X + (headingX * ship.Definition.Radius * MuzzleDistance);
        shot.Y = ship.Y + (headingY * ship.Definition.Radius * MuzzleDistance);
        shot.VelocityX = (headingX * weapon.Speed) + ship.VelocityX;
        shot.VelocityY = (headingY * weapon.Speed) + ship.VelocityY;
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

        // Shields do not protect against bites.
        foreach (var enemy in Combat.EnemyTargets(world, ship))
        {
            var distance = MathF.Sqrt(Combat.Square(enemy.X - ship.X) + Combat.Square(enemy.Y - ship.Y));
            var bites = BiteRanges.Count(range => distance < range);
            for (var k = 0; k < bites; k++)
            {
                if (world.Random.Next(PercentRoll) < BiteChance && enemy.Energy > MinEnergyLeft)
                {
                    enemy.Energy -= 1;
                    DropPickup(world, enemy, special.Duration);
                }
            }
        }
    }

    private static void DropPickup(World world, Ship enemy, int lifetime)
    {
        var pickup = world.Pickups.FirstOrDefault(p => !p.IsActive);
        if (pickup == null)
        {
            return;
        }

        var radius = enemy.Definition.Radius;
        pickup.X = enemy.X + (world.Random.Next(PickupSpreadRoll) / PickupSpreadDivisor * radius / 2f) - (PickupOffset * radius);
        pickup.Y = enemy.Y + (world.Random.Next(PickupSpreadRoll) / PickupSpreadDivisor * radius / 2f) - (PickupOffset * radius);
        pickup.Lifetime = lifetime;
        pickup.Delay = PickupDelay;
    }
}
