namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Magneto: fires two shots at once from both sides of the nose; the special is a shield that blocks
/// shots and rams while it lasts.
/// </summary>
public sealed class MagnetoBehavior : ShipBehavior
{
    private const float SideAngle = 45f;
    private const float MuzzleDistance = 0.9f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var left = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (left != null)
        {
            world.PlaySound(ship, ShipSound.Fire);
            Combat.Launch(left, ship, SideAngle, MuzzleDistance);
        }

        var right = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (right != null)
        {
            Combat.Launch(right, ship, -SideAngle, MuzzleDistance);
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        ship.SpecialTimeLeft = special.Duration;
        ship.IsShieldActive = true;
    }
}
