using Saoss.Core.Ships;
using Saoss.Core.Simulation.Weapons;

namespace Saoss.Core.Simulation;

/// <summary>
/// Per-tick ship update: weapon triggers, turning, thrust, friction, arena bounds, ship collisions
/// and the position step. Port of the original <c>Player::liiku</c>.
/// </summary>
public static class ShipMovement
{
    private const float WeaponAngleReturnRate = 1.5f;
    private const float NormalThrustDrag = 0.9f;
    private const float CoastDrag = 0.1f;
    private const float BoostThrustFactor = 20f;
    private const float BoostDrag = 10.9f;
    private const float BoostTurnFactor = 1.5f;
    private const float ShieldThrustFactor = 2f;
    private const float ShieldDrag = 0.5f;
    private const float ShieldTurnDivisor = 5f;
    private const float ShieldFuelRegenDivisor = 8f;
    private const float BoundsPush = 0.02f;
    private const float BoostBoundsPush = 0.2f;
    private const float PropellerFactor = 30f;
    private const float WeaponChargeDecay = 0.3f;

    /// <summary>
    /// Advances one living ship by one tick.
    /// Steps: special and fire triggers, weapon angle return, Electra reversing, thrust, tail history,
    /// turning, fuel regeneration, friction, arena bounds, propeller, ship-to-ship collisions,
    /// pending teleport, ship-specific effects, and finally the position update.
    /// </summary>
    public static void Update(Ship ship, World world)
    {
        var definition = ship.Definition;
        var behavior = ShipBehaviors.For(ship.Type);
        var controls = ship.Controls;
        if (ship.Type == ShipType.StarWorm)
        {
            controls.Thrust = true;
        }

        UpdateTriggers(ship, world, behavior, controls);
        ReturnWeaponAngle(ship, controls);

        var slowdown = ship.SlimeSlowdown;
        if (ship.Type == ShipType.Electra)
        {
            ReverseElectra(ship, controls);
        }

        var isBoosting = ship.Type == ShipType.Wiiii && ship.IsShieldActive;
        var isShapperShield = ship.Type == ShipType.Shapper && ship.IsShieldActive;
        ship.IsThrusting = controls.Thrust || isBoosting || ship.Type == ShipType.Electra;
        if (ship.IsThrusting)
        {
            ApplyThrust(ship, slowdown, isBoosting, isShapperShield);
        }

        if (!controls.Thrust && (ship.Type == ShipType.Ufo || ship.Type == ShipType.StarWorm))
        {
            ship.VelocityX = 0;
            ship.VelocityY = 0;
        }

        ship.ShiftTail(recordCurrent: ship.Type == ShipType.StarWorm);
        Turn(ship, controls, slowdown, isBoosting, isShapperShield);

        if (ship.FireCooldown > 0)
        {
            ship.FireCooldown--;
        }

        RegenerateFuel(ship, isShapperShield);

        if (!definition.IsInertiaFree)
        {
            ship.VelocityX = Drag(ship.VelocityX, CoastDrag);
            ship.VelocityY = Drag(ship.VelocityY, CoastDrag);
        }

        KeepInArena(ship, isBoosting);

        if (ship.Type is ShipType.Leechmother or ShipType.Elephant or ShipType.Electra)
        {
            var (headingX, headingY) = ship.Heading;
            ship.PropellerAngle += ((headingX * ship.VelocityX) + (headingY * ship.VelocityY)) * PropellerFactor;
        }

        if (!controls.Fire && ship.IsFireLoopPlaying)
        {
            ship.IsFireLoopPlaying = false;
            world.Emit(new ShipLoopStopped(ship.Index));
        }

        foreach (var other in world.Ships)
        {
            if (other.Index > ship.Index && other.IsInGame && other.IsAlive)
            {
                ShipCollisions.Resolve(ship, other);
            }
        }

        if (ship.Type == ShipType.Ufo && ship.IsTeleportPending)
        {
            ship.X = ship.TeleportX;
            ship.Y = ship.TeleportY;
            ship.IsTeleportPending = false;
        }

        behavior.BeforePositionUpdate(ship, world);

        if (ship.WeaponCharge > 0)
        {
            ship.WeaponCharge = MathF.Max(0, ship.WeaponCharge - WeaponChargeDecay);
        }

        ship.X += ship.VelocityX;
        ship.Y += ship.VelocityY;
    }

    /// <summary>The original drag formula: v / (1 + friction * factor * (|v| + 1)).</summary>
    public static float Drag(float velocity, float factor) =>
        velocity / (1f + (GameConstants.Friction * factor * (MathF.Abs(velocity) + 1f)));

    private static void UpdateTriggers(Ship ship, World world, ShipBehavior behavior, ShipControls controls)
    {
        var definition = ship.Definition;
        if (controls.Special && ship.Fuel >= definition.Special.FuelCost && ship.SpecialCooldown < 1)
        {
            behavior.ActivateSpecial(ship, world);
        }

        if (ship.SpecialTimeLeft > 0)
        {
            ship.SpecialTimeLeft--;
        }

        if (ship.Type == ShipType.Wiiii && ship.IsShieldActive)
        {
            if (ship.Fuel <= 0)
            {
                ship.IsShieldActive = false;
                ship.Fuel = 0;
            }
            else
            {
                ship.Fuel -= definition.Special.FuelCost;
            }
        }

        if (ship.SpecialTimeLeft <= 0 && ship.Type == ShipType.Magneto)
        {
            ship.IsShieldActive = false;
        }

        if (ship.SpecialCooldown > 0)
        {
            ship.SpecialCooldown--;
        }

        if (controls.Fire && (ship.Type != ShipType.Wiiii || !ship.IsShieldActive) && CanFire(ship))
        {
            behavior.Fire(ship, world);
        }

        // The Power Vacuum's charged shot is released when the button is let go.
        if (!controls.Fire && ship.Type == ShipType.PowerVacuum && ship.PrimaryProjectiles[0].IsActive)
        {
            behavior.Fire(ship, world);
        }
    }

    private static bool CanFire(Ship ship) => ship.Fuel >= ship.Definition.Primary.FuelCost && ship.FireCooldown < 1;

    private static void ReturnWeaponAngle(Ship ship, ShipControls controls)
    {
        if (controls.Fire && CanFire(ship))
        {
            return;
        }

        var weaponAngle = ship.WeaponAngle;
        while (weaponAngle > ship.Angle + 180f)
        {
            weaponAngle -= 360f;
        }

        while (weaponAngle < ship.Angle - 180f)
        {
            weaponAngle += 360f;
        }

        if (weaponAngle > ship.Angle)
        {
            weaponAngle -= WeaponAngleReturnRate;
        }

        if (weaponAngle < ship.Angle)
        {
            weaponAngle += WeaponAngleReturnRate;
        }

        ship.WeaponAngle = weaponAngle;
    }

    // Electra flips around once each time thrust is pressed; FireSide remembers whether it already did.
    private static void ReverseElectra(Ship ship, ShipControls controls)
    {
        if (!controls.Thrust && ship.FireSide == 0)
        {
            ship.FireSide = 1;
        }

        if (controls.Thrust && ship.FireSide == 1)
        {
            ship.Angle = WrapAngle(ship.Angle + 180f);
            ship.FireSide = 0;
        }
    }

    private static void ApplyThrust(Ship ship, float slowdown, bool isBoosting, bool isShapperShield)
    {
        var thrust = ship.Definition.Thrust;
        var (headingX, headingY) = ship.Heading;
        if (ship.Definition.IsInertiaFree)
        {
            ship.VelocityX = thrust * headingX * slowdown;
            ship.VelocityY = thrust * headingY * slowdown;
            return;
        }

        var (factor, drag) = isBoosting ? (BoostThrustFactor, BoostDrag)
            : isShapperShield ? (ShieldThrustFactor, ShieldDrag)
            : (1f, NormalThrustDrag);
        ship.VelocityX = Drag(ship.VelocityX + (thrust * factor * headingX * slowdown), drag);
        ship.VelocityY = Drag(ship.VelocityY + (thrust * factor * headingY * slowdown), drag);
    }

    private static void Turn(Ship ship, ShipControls controls, float slowdown, bool isBoosting, bool isShapperShield)
    {
        var turnRate = ship.Definition.TurnRate;
        var step = isBoosting ? turnRate * BoostTurnFactor
            : isShapperShield ? turnRate / ShieldTurnDivisor
            : turnRate;

        // Left and right are applied separately in the original, so holding both cancels out.
        foreach (var sign in (ReadOnlySpan<int>)[controls.Left ? 1 : 0, controls.Right ? -1 : 0])
        {
            if (sign == 0)
            {
                continue;
            }

            ship.Angle += sign * step * slowdown;
            ship.WeaponAngle += sign * step * slowdown;
            if (ship.Type == ShipType.Bulldog)
            {
                SteerBulldogShots(ship, sign * turnRate * 2f);
            }
        }

        ship.Angle = WrapAngle(ship.Angle);
    }

    private static void SteerBulldogShots(Ship ship, float degrees)
    {
        for (var i = 0; i < ship.Definition.Primary.MaxProjectiles; i++)
        {
            var projectile = ship.PrimaryProjectiles[i];
            if (projectile.IsActive)
            {
                projectile.Angle += degrees;
            }
        }
    }

    private static void RegenerateFuel(Ship ship, bool isShapperShield)
    {
        var definition = ship.Definition;
        if (ship.Fuel >= definition.MaxFuel)
        {
            return;
        }

        if (isShapperShield)
        {
            ship.Fuel += definition.FuelRegen / ShieldFuelRegenDivisor;
        }
        else if (ship.Type != ShipType.PowerVacuum || !ship.PrimaryProjectiles[0].IsActive)
        {
            ship.Fuel += definition.FuelRegen;
        }
    }

    private static void KeepInArena(Ship ship, bool isBoosting)
    {
        if (ship.Definition.IsInertiaFree)
        {
            var step = ship.Definition.Thrust;
            ship.X += ship.X > GameConstants.ArenaMaxX ? -step : ship.X < GameConstants.ArenaMinX ? step : 0;
            ship.Y += ship.Y > GameConstants.ArenaMaxY ? -step : ship.Y < GameConstants.ArenaMinY ? step : 0;
            return;
        }

        var push = isBoosting ? BoostBoundsPush : BoundsPush;
        ship.VelocityX += ship.X > GameConstants.ArenaMaxX ? -push : ship.X < GameConstants.ArenaMinX ? push : 0;
        ship.VelocityY += ship.Y > GameConstants.ArenaMaxY ? -push : ship.Y < GameConstants.ArenaMinY ? push : 0;
    }

    /// <summary>Wraps an angle in degrees once into [-180, 180], like the original.</summary>
    public static float WrapAngle(float degrees) =>
        degrees > 180f ? degrees - 360f : degrees < -180f ? degrees + 360f : degrees;
}
