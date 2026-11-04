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

    [TestMethod]
    public void TopDown_LooksStraightDownWithoutBillboardTurn()
    {
        var camera = new GameCamera(CameraMode.TopDown);

        camera.Update(CreateWorld());
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

        topDown.Update(CreateWorld());
        tilted.Update(CreateWorld());

        Assert.AreEqual(((20f * 19f) + (10f * 4f / 3f)) / 20f, topDown.Z, Tolerance);
        Assert.AreEqual(((20f * 19f) + (12f * 4f / 3f)) / 20f, tilted.Z, Tolerance);
        Assert.AreEqual(tilted.X, topDown.X, Tolerance);
    }

    [TestMethod]
    public void Tilted_LeansTowardTheArenaCenter()
    {
        var camera = new GameCamera(CameraMode.Tilted);

        camera.Update(CreateWorld());
        var (eye, target, _) = camera.View();

        Assert.AreNotEqual(0f, camera.Beta);
        Assert.AreEqual(camera.X / 4f, eye.X, Tolerance);
        Assert.AreEqual(Saoss.Core.GameConstants.ShipPlaneZ, target.Z);
    }

    private static Saoss.Core.Simulation.World CreateWorld()
    {
        var world = TestWorld.Create(players: 2);
        TestWorld.Place(world, 0, ShipType.Magneto, -10f, -6f);
        TestWorld.Place(world, 1, ShipType.Magneto, 0f, 6f);
        return world;
    }
}
