namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Moth: fires three shots at once, to the left, forward and to the right; the special refills a third of its fuel
/// tank (it has no other fuel regeneration).
/// </summary>
public sealed class MothBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.8f;
    private const float FirstShotAngle = -90f;
    private const float ShotAngleStep = 90f;
    private const int ShotCount = 3;
    private const float RefillFraction = 3f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var offset = FirstShotAngle;
        for (var j = 0; j < ShotCount; j++)
        {
            var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
            if (shot != null)
            {
                world.PlaySound(ship, ShipSound.Fire);
                var direction = (ship.Angle + offset) / 180f * MathF.PI;
                shot.Lifetime = weapon.Lifetime;
                shot.X = ship.X + (MathF.Cos(direction) * ship.Definition.Radius * MuzzleDistance);
                shot.Y = ship.Y + (MathF.Sin(direction) * ship.Definition.Radius * MuzzleDistance);
                shot.VelocityX = (MathF.Cos(direction) * weapon.Speed) + ship.VelocityX;
                shot.VelocityY = (MathF.Sin(direction) * weapon.Speed) + ship.VelocityY;
                shot.Angle = ship.Angle;
                shot.Energy = weapon.ProjectileEnergy;
            }

            offset += ShotAngleStep;
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.Fuel = MathF.Min(ship.Fuel + (ship.Definition.MaxFuel / RefillFraction), ship.Definition.MaxFuel);
        ship.SpecialCooldown = ship.Definition.Special.Cooldown;

        // The original also releases the special button.
        ship.Controls = ship.Controls with { Special = false };
    }
}
