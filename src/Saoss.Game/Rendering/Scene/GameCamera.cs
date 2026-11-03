using Saoss.Core;
using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// The tilted chase camera of the original (calculateFrame, cammode 0): it follows the middle of all
/// ships, zooms out to fit them, and leans toward the arena center.
/// </summary>
public sealed class GameCamera
{
    private const float FollowSmoothing = 19f;
    private const float FollowScale = 1.2f;
    private const float ZoomFactor = 4f / 3f;
    private const float InitialHeight = 20f;
    private const float OriginalPi = 3.14f;

    /// <summary>Camera target and height [camposx, camposy, camposz].</summary>
    public float X { get; private set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; private set; }

    /// <inheritdoc cref="X"/>
    public float Z { get; private set; } = InitialHeight;

    /// <summary>Billboard rotation around Z in degrees [alfa].</summary>
    public float Alpha { get; private set; }

    /// <summary>Smoothed version of <see cref="Alpha"/> used for the up vector [alfa2].</summary>
    public float SmoothedAlpha { get; private set; }

    /// <summary>Billboard tilt in degrees [beta].</summary>
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

        var spread = MathF.Max(spreadX, spreadY);
        Z = ((Z * FollowSmoothing) + (spread * ZoomFactor)) / (FollowSmoothing + 1f);

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
