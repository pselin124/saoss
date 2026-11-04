using Saoss.Game.Rendering.Scene;

namespace Saoss.Tests.Rendering.CameraVisibility;

/// <summary>How well one camera setup kept the flying ships on screen over a set of matches.</summary>
/// <param name="Mode">Camera mode.</param>
/// <param name="Aspect">Viewport width / height.</param>
/// <param name="ShipTicks">Ticks summed over every in-game, alive ship.</param>
/// <param name="OffScreenTicks">Of <paramref name="ShipTicks"/>, those with the ship not fully inside the view.</param>
/// <param name="RespawnOffScreenTicks">
/// Of <paramref name="OffScreenTicks"/>, those in the ship's first 200 alive ticks (after a respawn or the match start).
/// </param>
/// <param name="LongestStreak">Longest run of consecutive off-screen ticks of one ship.</param>
/// <param name="LongestStreakNote">Situation at the start of the longest streak.</param>
internal sealed record CameraVisibilityStats(
    CameraMode Mode,
    float Aspect,
    long ShipTicks,
    long OffScreenTicks,
    long RespawnOffScreenTicks,
    int LongestStreak,
    string LongestStreakNote)
{
    /// <summary>Off-screen share of <see cref="ShipTicks"/> in percent.</summary>
    public double OffScreenPercent => ShipTicks == 0 ? 0 : 100.0 * OffScreenTicks / ShipTicks;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Mode,-7} {Aspect,5:0.00}: off {OffScreenTicks,6} / {ShipTicks} ({OffScreenPercent:0.000}%), "
        + $"respawn {RespawnOffScreenTicks,6}, longest {LongestStreak,4} [{LongestStreakNote}]";
}
