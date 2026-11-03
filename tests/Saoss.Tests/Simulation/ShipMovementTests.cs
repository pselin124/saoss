using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation;

/// <summary>
/// Tests the per-tick ship movement rules ported from <c>Player::liiku</c>.
/// </summary>
[TestClass]
public sealed class ShipMovementTests
{
    private const float Tolerance = 1e-6f;

    [TestMethod]
    public void Drag_MatchesOriginalFormula()
    {
        var expected = 0.5f / (1f + (0.006f * 0.9f * (0.5f + 1f)));

        Assert.AreEqual(expected, ShipMovement.Drag(0.5f, 0.9f), Tolerance);
    }

    [TestMethod]
    public void Thrust_FromRest_AcceleratesAlongHeadingWithThrustAndCoastDrag()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f, angle: 90f);
        ship.Controls = new ShipControls { Thrust = true };

        ShipMovement.Update(ship, world);

        var thrust = ShipCatalog.Get(ShipType.Magneto).Thrust;
        var expectedSpeed = ShipMovement.Drag(ShipMovement.Drag(thrust, 0.9f), 0.1f);
        Assert.AreEqual(0f, ship.VelocityX, 1e-6f);
        Assert.AreEqual(expectedSpeed, ship.VelocityY, Tolerance);
        Assert.AreEqual(50f + expectedSpeed, ship.Y, Tolerance);
        Assert.IsTrue(ship.IsThrusting);
    }

    [TestMethod]
    public void Left_TurnsCounterClockwiseByTurnRate_BothDirectionsCancel()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tringu, 50f, 50f, angle: 10f);

        ship.Controls = new ShipControls { Left = true };
        ShipMovement.Update(ship, world);
        Assert.AreEqual(12f, ship.Angle, Tolerance);

        ship.Controls = new ShipControls { Left = true, Right = true };
        ShipMovement.Update(ship, world);
        Assert.AreEqual(12f, ship.Angle, Tolerance);
    }

    [TestMethod]
    public void Turning_PastHalfCircle_WrapsAngle()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Tringu, 50f, 50f, angle: 179.5f);
        ship.Controls = new ShipControls { Left = true };

        ShipMovement.Update(ship, world);

        Assert.AreEqual(-178.5f, ship.Angle, 1e-4f);
    }

    [TestMethod]
    public void Ufo_MovesAtFixedSpeedWhileThrusting_AndStopsWhenReleased()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Ufo, 50f, 50f);

        ship.Controls = new ShipControls { Thrust = true };
        ShipMovement.Update(ship, world);
        Assert.AreEqual(ShipCatalog.Get(ShipType.Ufo).Thrust, ship.VelocityX, Tolerance);

        ship.Controls = default;
        ShipMovement.Update(ship, world);
        Assert.AreEqual(0f, ship.VelocityX);
        Assert.AreEqual(0f, ship.VelocityY);
    }

    [TestMethod]
    public void StarWorm_AlwaysThrustsAndRecordsTail()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.StarWorm, 50f, 50f);

        ShipMovement.Update(ship, world);

        Assert.IsTrue(ship.IsThrusting);
        Assert.AreEqual(50f, ship.TailX[0], Tolerance);
        Assert.IsGreaterThan(50f, ship.X);
    }

    [TestMethod]
    public void Electra_FlipsAroundOncePerThrustPress()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Electra, 50f, 50f, angle: 0f);

        ship.Controls = new ShipControls { Thrust = true };
        ShipMovement.Update(ship, world);
        Assert.AreEqual(180f, ship.Angle, Tolerance);

        ShipMovement.Update(ship, world);
        Assert.AreEqual(180f, ship.Angle, Tolerance);

        ship.Controls = default;
        ShipMovement.Update(ship, world);
        ship.Controls = new ShipControls { Thrust = true };
        ShipMovement.Update(ship, world);
        Assert.AreEqual(0f, ship.Angle, Tolerance);
    }

    [TestMethod]
    public void Fuel_RegeneratesByFuelRegenPerTick()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f);
        ship.Fuel = 10f;

        ShipMovement.Update(ship, world);

        Assert.AreEqual(10f + ShipCatalog.Get(ShipType.Magneto).FuelRegen, ship.Fuel, Tolerance);
    }

    [TestMethod]
    public void OutsideArena_IsPushedBack()
    {
        var world = TestWorld.Create();
        var ship = TestWorld.Place(world, 0, ShipType.Magneto, GameConstants.ArenaMaxX + 5f, 0f);

        ShipMovement.Update(ship, world);

        Assert.IsLessThan(0f, ship.VelocityX);
    }

    [TestMethod]
    public void Slime_SlowsThrust()
    {
        var world = TestWorld.Create();
        var clean = TestWorld.Place(world, 0, ShipType.Magneto, 50f, 50f);
        var slimed = TestWorld.Place(world, 1, ShipType.Magneto, -50f, -50f);
        slimed.SlimeCount = GameConstants.MaxSlimes;
        clean.Controls = slimed.Controls = new ShipControls { Thrust = true };

        ShipMovement.Update(clean, world);
        ShipMovement.Update(slimed, world);

        Assert.AreEqual(clean.VelocityX * 0.2f, slimed.VelocityX, 1e-5f);
    }
}
