namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Star Worm: fires single spinning shots from its head; the special sprays 25 randomly drifting sparks backward
/// from the end of its tail.
/// </summary>
public sealed class StarWormBehavior : ShipBehavior
{
    private const float MuzzleDistance = 2.5f;
    private const int Sparks = 25;
    private const float TailGap = 2f;
    private const float SparkSpeedRoll = 100f;
    private const float SparkSpeedDivisor = 200f;
    private const float JitterRoll = 100f;
    private const float JitterDivisor = 1000f;
    private const float JitterOffset = 0.05f;

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
        world.PlaySound(ship, ShipSound.SpecialFire);

        var last = GameConstants.TailLength - 1;
        var backward = (ship.TailAngle[last] * MathF.PI / 180f) - MathF.PI;
        var count = Sparks;
        for (var i = 0; i < special.MaxProjectiles && count > 0; i++)
        {
            var spark = ship.SpecialProjectiles[i];
            if (spark.IsActive)
            {
                continue;
            }

            spark.Lifetime = special.Duration;
            spark.Energy = special.ProjectileEnergy;
            spark.X = ship.TailX[last] + (MathF.Cos(backward) * TailGap);
            spark.Y = ship.TailY[last] + (MathF.Sin(backward) * TailGap);

            // Each velocity component rolls its own random speed.
            spark.VelocityX = MathF.Cos(backward) * (special.Speed + (world.Random.Next(SparkSpeedRoll) / SparkSpeedDivisor));
            spark.VelocityY = MathF.Sin(backward) * (special.Speed + (world.Random.Next(SparkSpeedRoll) / SparkSpeedDivisor));
            count--;
        }
    }

    /// <inheritdoc/>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var spark = ship.SpecialProjectiles[i];
            if (!spark.IsActive)
            {
                continue;
            }

            spark.Lifetime--;
            spark.VelocityX += Jitter(world);
            spark.VelocityY += Jitter(world);
            spark.X += spark.VelocityX;
            spark.Y += spark.VelocityY;
            Combat.ApplyPlanet(spark);
            Combat.HitShipsWithSpecial(ship, world, spark);
        }
    }

    private static float Jitter(World world) => (world.Random.Next(JitterRoll) / JitterDivisor) - JitterOffset;
}
