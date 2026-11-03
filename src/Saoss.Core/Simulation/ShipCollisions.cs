using Saoss.Core.Ships;

namespace Saoss.Core.Simulation;

/// <summary>
/// Ship-to-ship bumping and ramming damage [Player::tormaa(x, y, radius, i)].
/// </summary>
public static class ShipCollisions
{
    // Collision circles are smaller than the nominal radius.
    private const float RadiusScale = 0.785f;
    private const float InertiaFreePushBack = 0.2f;

    /// <summary>
    /// If the two ships will overlap after this tick's movement, damages them with ramming specials
    /// and pushes them apart with an impulse based on their relative speed and masses.
    /// </summary>
    /// <returns>True if the ships collided.</returns>
    public static bool Resolve(Ship mover, Ship other)
    {
        var dx = other.X + other.VelocityX - (mover.X + mover.VelocityX);
        var dy = other.Y + other.VelocityY - (mover.Y + mover.VelocityY);
        var reach = (other.Definition.Radius + mover.Definition.Radius) * RadiusScale;
        if (MathF.Sqrt((dx * dx) + (dy * dy)) >= reach)
        {
            return false;
        }

        var normal = MathF.Atan2(mover.Y - other.Y, mover.X - other.X);
        var relativeSpeed = MathF.Sqrt(Square(other.VelocityX - mover.VelocityX) + Square(other.VelocityY - mover.VelocityY));
        var reducedMass = 1f / ((1f / mover.Definition.Mass) + (1f / other.Definition.Mass));
        var impulse = relativeSpeed * reducedMass * 2f;

        ApplyRamDamage(attacker: mover, target: other);
        ApplyRamDamage(attacker: other, target: mover);

        Push(mover, other, MathF.Cos(normal) * impulse, MathF.Sin(normal) * impulse);
        Push(other, mover, -MathF.Cos(normal) * impulse, -MathF.Sin(normal) * impulse);
        return true;
    }

    // Bulldog rams with damage scaled by its fuel and empties it; Wiiii! rams with full damage. A Magneto shield blocks both.
    private static void ApplyRamDamage(Ship attacker, Ship target)
    {
        if (attacker.Type is not (ShipType.Bulldog or ShipType.Wiiii))
        {
            return;
        }

        if (target.Type == ShipType.Magneto && target.IsShieldActive)
        {
            return;
        }

        var damage = attacker.Definition.Special.Damage;
        if (attacker.Type == ShipType.Bulldog)
        {
            target.Energy -= damage * attacker.Fuel / attacker.Definition.MaxFuel;
            attacker.Fuel = 0;
        }
        else
        {
            target.Energy -= damage;
        }
    }

    private static void Push(Ship ship, Ship from, float impulseX, float impulseY)
    {
        if (ship.Definition.IsInertiaFree)
        {
            ship.X -= (from.X - ship.X) * InertiaFreePushBack;
            ship.Y -= (from.Y - ship.Y) * InertiaFreePushBack;
        }
        else
        {
            ship.VelocityX += impulseX / ship.Definition.Mass;
            ship.VelocityY += impulseY / ship.Definition.Mass;
        }
    }

    private static float Square(float value) => value * value;
}
