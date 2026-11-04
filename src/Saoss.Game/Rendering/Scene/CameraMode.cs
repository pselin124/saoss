namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// How the match camera looks at the arena [sets->cammode]. The settings menu shows them as "3d" and "2d".
/// </summary>
public enum CameraMode
{
    /// <summary>Chase camera that leans toward the arena center; billboards turn to face it (cammode 0).</summary>
    Tilted = 0,

    /// <summary>Camera straight above the ships; billboards are not turned (cammode 1).</summary>
    TopDown = 1,
}
