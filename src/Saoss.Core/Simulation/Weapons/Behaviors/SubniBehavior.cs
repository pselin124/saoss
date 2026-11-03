namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Subni: fires single shots alternately from the left and right side; the special drops a mine behind
/// the ship that creeps toward the nearest enemy and explodes on contact.
/// </summary>
public sealed class SubniBehavior : ShipBehavior
{
    private const float SideAngle = 55f;
    private const float MuzzleDistance = 0.8f;
    private const float DropDistance = 1.1f;

    // Mines use a tighter contact circle than the usual collision scale.
    private const float MineContactScale = 0.6f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        world.PlaySound(ship, ShipSound.Fire);
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (shot != null)
        {
            Combat.Launch(shot, ship, SideAngle * ship.FireSide, MuzzleDistance);
            ship.FireSide = -ship.FireSide;
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var mine = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (mine != null)
        {
            var behind = (ship.Angle + 180f) / 180f * MathF.PI;
            mine.X = ship.X + (MathF.Cos(behind) * ship.Definition.Radius * DropDistance);
            mine.Y = ship.Y + (MathF.Sin(behind) * ship.Definition.Radius * DropDistance);
            mine.Angle = 0;
            mine.Lifetime = special.Duration;
            mine.Energy = special.ProjectileEnergy;
        }
    }

    /// <inheritdoc/>
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
            if (Combat.Square(mine.X) + Combat.Square(mine.Y) < Combat.PlanetRadiusSquared)
            {
                mine.Lifetime = 0;
            }

            Detonate(ship, world, mine);
            var nearest = Combat.EnemyTargets(world, ship)
                .MinBy(target => Combat.Square(mine.X - target.X) + Combat.Square(mine.Y - target.Y));
            if (nearest != null)
            {
                var dx = mine.X - nearest.X;
                var dy = mine.Y - nearest.Y;
                var distance = MathF.Sqrt((dx * dx) + (dy * dy));
                mine.X -= dx / distance * special.Speed;
                mine.Y -= dy / distance * special.Speed;
            }
        }
    }

    // Every ship touching the mine is hit, including the Subni itself and its teammates.
    private static void Detonate(Ship ship, World world, Projectile mine)
    {
        var special = ship.Definition.Special;
        foreach (var target in Combat.Targets(world))
        {
            var reach = (special.Radius + target.Definition.Radius) * MineContactScale;
            if (Combat.Square(mine.X - target.X) + Combat.Square(mine.Y - target.Y) >= reach * reach)
            {
                continue;
            }

            mine.Lifetime = 0;
            if (!Combat.IsShielded(target))
            {
                target.Energy -= special.Damage;
            }

            world.PlaySound(ship, ShipSound.SpecialHit);
        }
    }
}
