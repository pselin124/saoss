using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Weapons;

[TestClass]
public sealed class AllShipsSmokeTests
{
    private const int Ticks = 600;

    [TestMethod]
    public void EveryShip_HoldingAllButtons_StaysFinite()
    {
        foreach (var type in Enum.GetValues<ShipType>())
        {
            var world = TestWorld.Create();
            var ship = TestWorld.Place(world, 0, type, 60f, 40f, angle: 30f);
            TestWorld.Place(world, 1, ShipType.Elephant, -60f, -40f);
            var controls = new ShipControls { Fire = true, Special = true, Thrust = true, Left = true };

            for (var i = 0; i < Ticks; i++)
            {
                ship.Controls = controls;
                world.Step();
                foreach (var s in world.Ships)
                {
                    Assert.IsFalse(float.IsNaN(s.X) || float.IsNaN(s.Y), $"{type}: ship {s.Index} position is NaN at tick {i}.");
                }
            }
        }
    }
}
