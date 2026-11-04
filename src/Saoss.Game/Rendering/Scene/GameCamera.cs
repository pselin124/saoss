using Microsoft.Xna.Framework;
using Saoss.Core;
using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// The match camera of the original (calculateFrame and the gluLookAt of drawFrame). It follows the middle of all
/// ships and zooms out to fit them. In <see cref="CameraMode.Tilted"/> it also leans toward the arena center and
/// billboards turn toward it; in <see cref="CameraMode.TopDown"/> it looks straight down and the billboard angles
/// stay zero.
/// </summary>
/// <param name="mode">Which of the original's two cameras to use.</param>
public sealed class GameCamera(CameraMode mode = CameraMode.Tilted)
{
    private const float FollowSmoothing = 19f;
    private const float FollowScale = 1.2f;
    private const float ZoomFactor = 4f / 3f;
    private const float InitialHeight = 20f;
    private const float OriginalPi = 3.14f;
    private const float TopDownWidthWeight = 48f / 64f;

    /// <summary>Which camera this is.</summary>
    public CameraMode Mode { get; } = mode;

    /// <summary>Camera target and height [camposx, camposy, camposz].</summary>
    public float X { get; private set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; private set; }

    /// <inheritdoc cref="X"/>
    public float Z { get; private set; } = InitialHeight;

    /// <summary>Billboard rotation around Z in degrees [alfa]; zero for the top-down camera.</summary>
    public float Alpha { get; private set; }

    /// <summary>Smoothed version of <see cref="Alpha"/> used for the up vector [alfa2].</summary>
    public float SmoothedAlpha { get; private set; }

    /// <summary>Billboard tilt in degrees [beta]; zero for the top-down camera.</summary>
    public float Beta { get; private set; }

    /// <summary>Advances the camera one tick toward the ships that are in the game.</summary>
    public void Update(World world)
    {
        var ships = world.Ships.Where(s => s.IsInGame).ToList();
        float maxX = GameConstants.ArenaMinX, maxY = GameConstants.ArenaMinY;
        float minX = GameConstants.ArenaMaxX, minY = GameConstants.ArenaMaxY;
        foreach (var ship in ships)
        {
            maxX = MathF.Max(maxX, ship.X);
            maxY = MathF.Max(maxY, ship.Y);
            minX = MathF.Min(minX, ship.X);
            minY = MathF.Min(minY, ship.Y);
        }

        X = ((FollowSmoothing * X) + ((maxX + minX) / 2f / FollowScale)) / (FollowSmoothing + 1f);
        Y = ((FollowSmoothing * Y) + ((maxY + minY) / 2f / FollowScale)) / (FollowSmoothing + 1f);

        float spreadX = 0, spreadY = 0;
        for (var i = 0; i < ships.Count; i++)
        {
            for (var j = i; j < ships.Count; j++)
            {
                spreadX = MathF.Max(spreadX, MathF.Abs(ships[i].X - ships[j].X));
                spreadY = MathF.Max(spreadY, MathF.Abs(ships[i].Y - ships[j].Y));
            }
        }

        Z = ((Z * FollowSmoothing) + (Spread(spreadX, spreadY) * ZoomFactor)) / (FollowSmoothing + 1f);
        if (Mode == CameraMode.Tilted)
        {
            UpdateLean();
        }
    }

    /// <summary>Eye, look-at point and up vector of the camera [gluLookAt in drawFrame].</summary>
    public (Vector3 Eye, Vector3 Target, Vector3 Up) View()
    {
        if (Mode == CameraMode.TopDown)
        {
            return (new Vector3(X, Y, Z), new Vector3(X, Y, 0f), Vector3.UnitY);
        }

        var alpha2 = SmoothedAlpha / 180f * OriginalPi;
        var beta = Beta / 180f * OriginalPi;
        return (
            new Vector3(X / 4f, Y / 4f, Z),
            new Vector3(X, Y, GameConstants.ShipPlaneZ),
            new Vector3(MathF.Cos(alpha2) * MathF.Cos(beta), MathF.Sin(alpha2) * MathF.Cos(beta), -MathF.Sin(beta)));
    }

    // The original's top-down rule picks the width as soon as it beats three quarters of the height.
    private float Spread(float spreadX, float spreadY) => Mode == CameraMode.TopDown
        ? (spreadX > spreadY * TopDownWidthWeight ? spreadX : spreadY)
        : MathF.Max(spreadX, spreadY);

    private void UpdateLean()
    {
        var leanX = 0.75f * X;
        var leanY = 0.75f * Y;
        var height = Z - GameConstants.ShipPlaneZ;
        var lean = MathF.Sqrt((leanX * leanX) + (leanY * leanY));
        var direction = MathF.Atan2(leanY, leanX) * 180f / OriginalPi;
        Alpha = direction;
        SmoothedAlpha = ((lean / 50f * direction) + (250f * SmoothedAlpha)) / (250f + (lean / 50f));
        Beta = ((-MathF.Atan2(lean, height) * 180f / OriginalPi) + (FollowSmoothing * Beta)) / (FollowSmoothing + 1f);
    }
}
