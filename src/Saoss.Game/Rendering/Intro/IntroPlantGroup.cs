namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// One push/pop block of plants in the intro's <c>draw</c>: each step translates from the previous plant (the steps
/// accumulate) and draws a plant there.
/// </summary>
/// <param name="MinDetail">Lowest detail level that shows the group (the original's <c>sets->detail &gt; MinDetail - 1</c>).</param>
/// <param name="Steps">Relative X/Z moves on the ground, each followed by a plant.</param>
public sealed record IntroPlantGroup(int MinDetail, IReadOnlyList<(float X, float Z, IntroPlant Plant)> Steps);
