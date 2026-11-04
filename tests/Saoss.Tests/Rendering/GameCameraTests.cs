using Microsoft.Xna.Framework;
using Saoss.Core.Ships;
using Saoss.Game.Rendering.Scene;
using Saoss.Tests.Simulation;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests the two match cameras (scene.cpp calculateFrame and drawFrame, cammode 0 and 1).
/// </summary>
[TestClass]
public sealed class GameCameraTests
{
    private const float Tolerance = 1e-4f;
    private const float Aspect = 4f / 3f;

    [TestMethod]
    public void TopDown_LooksStraightDownWithoutBillboardTurn()
    {
        var camera = new GameCamera(CameraMode.TopDown);

        camera.Update(CreateWorld(), Aspect);
        var (eye, target, up) = camera.View();

        Assert.AreEqual(0f, camera.Alpha);
        Assert.AreEqual(0f, camera.Beta);
        Assert.AreEqual(0f, camera.SmoothedAlpha);
        Assert.AreEqual(new Vector3(camera.X, camera.Y, camera.Z), eye);
        Assert.AreEqual(new Vector3(camera.X, camera.Y, 0f), target);
        Assert.AreEqual(Vector3.UnitY, up);
    }

    [TestMethod]
    public void TopDown_ZoomsToWidthOnceItBeatsThreeQuartersOfHeight()
    {
        // Spread 10 wide and 12 high: the top-down rule picks the width, the tilted camera the larger height.
        var topDown = new GameCamera(CameraMode.TopDown);
        var tilted = new GameCamera(CameraMode.Tilted);

        topDown.Update(CreateWorld(), Aspect);
        tilted.Update(CreateWorld(), Aspect);

        Assert.AreEqual(((20f * 19f) + (10f * 4f / 3f)) / 20f, topDown.Z, Tolerance);
        Assert.AreEqual(((20f * 19f) + (12f * 4f / 3f)) / 20f, tilted.Z, Tolerance);
        Assert.AreEqual(tilted.X, topDown.X, Tolerance);
    }

    [TestMethod]
    public void Tilted_LeansTowardTheArenaCenter()
    {
        var camera = new GameCamera(CameraMode.Tilted);

        camera.Update(CreateWorld(), Aspect);
        var (eye, target, _) = camera.View();

        Assert.AreNotEqual(0f, camera.Beta);
        Assert.AreEqual(camera.X / 4f, eye.X, Tolerance);
        Assert.AreEqual(Saoss.Core.GameConstants.ShipPlaneZ, target.Z);
    }

    [TestMethod]
    [DataRow(CameraMode.TopDown)]
    [DataRow(CameraMode.Tilted)]
    public void ZoomsOutBeyondTheOriginalPaceWhenAShipIsOffScreen(CameraMode mode)
    {
        // From the starting height both ships are far outside the view; the original pace alone would reach 32.3.
        var camera = new GameCamera(mode);
        var world = TestWorld.Create(players: 2);
        TestWorld.Place(world, 0, ShipType.Magneto, -100f, -80f);
        TestWorld.Place(world, 1, ShipType.Magneto, 100f, 70f);

        camera.Update(world, Aspect);

        Assert.IsGreaterThan(((20f * 19f) + (200f * 4f / 3f)) / 20f + 10f, camera.Z);
    }

    private static Saoss.Core.Simulation.World CreateWorld()
    {
        var world = TestWorld.Create(players: 2);
        TestWorld.Place(world, 0, ShipType.Magneto, -10f, -6f);
        TestWorld.Place(world, 1, ShipType.Magneto, 0f, 6f);
        return world;
    }
}
