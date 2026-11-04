using Microsoft.Xna.Framework;
using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Tests.Rendering.CameraVisibility;

/// <summary>
/// One camera setup followed through a match like the game does (camera update after every step), checking each
/// tick whether every in-game, alive ship is fully inside the view of the game's projection.
/// </summary>
internal sealed class CameraVisibilityProbe
{
    private const int RespawnTicks = 200;

    private readonly float _tanHalfFov = MathF.Tan(MathHelper.ToRadians(GameCamera.FieldOfView) / 2f);
    private readonly int[] _streaks;
    private readonly int[] _aliveTicks;
    private readonly string[] _streakNotes;
    private long _shipTicks;
    private long _offScreenTicks;
    private long _respawnOffScreenTicks;
    private int _longestStreak;
    private string _longestNote = "-";

    /// <summary>Creates a probe for <paramref name="ships"/> ships.</summary>
    public CameraVisibilityProbe(CameraMode mode, float aspect, int ships)
    {
        Camera = new GameCamera(mode);
        Aspect = aspect;
        _streaks = new int[ships];
        _aliveTicks = new int[ships];
        _streakNotes = new string[ships];
    }

    /// <summary>The camera followed.</summary>
    public GameCamera Camera { get; }

    /// <summary>Viewport width / height.</summary>
    public float Aspect { get; }

    /// <summary>Updates the camera after a step and checks every flying ship.</summary>
    public void Observe(World world)
    {
        Camera.Update(world, Aspect);
        var (eye, target, up) = Camera.View();
        var view = Matrix.CreateLookAt(eye, target, up);
        foreach (var ship in world.Ships)
        {
            Observe(ship, view);
        }
    }

    /// <summary>The results so far.</summary>
    public CameraVisibilityStats Stats() =>
        new(Camera.Mode, Aspect, _shipTicks, _offScreenTicks, _respawnOffScreenTicks, _longestStreak, _longestNote);

    private void Observe(Ship ship, Matrix view)
    {
        var i = ship.Index;
        if (!ship.IsInGame || !ship.IsAlive)
        {
            _aliveTicks[i] = 0;
            _streaks[i] = 0;
            return;
        }

        _aliveTicks[i]++;
        _shipTicks++;
        if (IsFullyVisible(ship, view))
        {
            _streaks[i] = 0;
            return;
        }

        var respawning = _aliveTicks[i] <= RespawnTicks;
        _offScreenTicks++;
        _respawnOffScreenTicks += respawning ? 1 : 0;
        if (_streaks[i]++ == 0)
        {
            _streakNotes[i] = $"{(respawning ? "respawn" : "moving")} ship ({ship.X:0},{ship.Y:0}) "
                + $"cam ({Camera.X:0},{Camera.Y:0},{Camera.Z:0})";
        }

        if (_streaks[i] > _longestStreak)
        {
            _longestStreak = _streaks[i];
            _longestNote = _streakNotes[i];
        }
    }

    private bool IsFullyVisible(Ship ship, Matrix view)
    {
        var p = Vector3.Transform(new Vector3(ship.X, ship.Y, GameConstants.ShipPlaneZ), view);
        var depth = -p.Z;
        var halfHeight = depth * _tanHalfFov;
        var radius = ship.Definition.Radius;
        return MathF.Abs(p.Y) + radius <= halfHeight && MathF.Abs(p.X) + radius <= halfHeight * Aspect;
    }
}
