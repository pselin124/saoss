namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// Where the intro's plants grow (lippu.cpp <c>draw</c>, in a frame scaled by <see cref="Scale"/>), in drawing order:
/// mushrooms, daisies, star flowers, sunflowers. Higher detail levels add groups. The shadow pass uses the same layout.
/// </summary>
public static class IntroPlantLayout
{
    /// <summary>Uniform scale applied to the whole meadow.</summary>
    public const float Scale = 1.5f;

    private const IntroPlant M = IntroPlant.Mushroom;
    private const IntroPlant B = IntroPlant.BlueDaisy;
    private const IntroPlant R = IntroPlant.RedDaisy;
    private const IntroPlant S = IntroPlant.StarFlower;
    private const IntroPlant F = IntroPlant.Sunflower;

    /// <summary>All groups in drawing order.</summary>
    public static IReadOnlyList<IntroPlantGroup> Groups { get; } =
    [
        new(0, [(5.0f, 0, M), (1.0f, 2, M)]),
        new(1, [(4, 1.5f, M), (-1.0f, 0.5f, M), (-0.2f, 1.3f, M)]),
        new(2, [(3, -1, M), (-1.0f, 1.2f, M), (-1.3f, 0.9f, M), (0.1f, 1.2f, M)]),
        new(3, [(7.5f, 1.5f, M), (0.2f, 1.5f, M), (-0.4f, 1.5f, M), (-1.5f, 0.5f, M), (-2.8f, 0.5f, M)]),

        new(0, [(8, -13, B), (-17, 0, R)]),
        new(1, [(-2.0f, -14, B), (2.0f, 0, R), (2.0f, 0, B), (-3.0f, 4.2f, R), (-1.4f, 0, B)]),
        new(2, [(5.0f, -8.0f, B), (-2.5f, 1.7f, R), (-2.5f, 0.7f, B), (-2.5f, 0, R), (-2.5f, -0.7f, B), (-2.5f, -1.7f, R)]),
        new(3, [(7.5f, -10.5f, B), (-17.5f, 0, R)]),
        new(3, [(0, -4.0f, B), (-0.6f, 1.2f, R), (-1.7f, 2.0f, B), (-0.3f, -1.5f, R), (1.7f, 1.2f, B), (1.1f, 0, R)]),

        new(0, [(3, -5, S), (2, 0, S)]),
        new(1, [(3, -3, S), (2, 0, S), (-1, 1, S)]),
        new(2, [(4.0f, -6, S), (-2, 2, S), (2, 0, S), (1, 3, S)]),
        new(3, [(7, -7, S), (1, -1, S), (1, -1, S), (0, 2, S), (-1, 1, S), (-1, 1, S)]),

        new(0, [(-12, -8, F), (2, 0, F)]),
        new(1, [(-12, -6, F), (2, 0, F), (1, -1, F)]),
        new(2, [(-11, -7, F), (-2, 0, F)]),
        new(2, [(-1, 2, F), (-2, 0, F)]),
        new(3, [(-1, 3.5f, F), (-2, 0, F)]),
        new(3, [(-10, -3, F), (0, 1.5f, F), (0, 1.5f, F), (0, 1.5f, F)]),
    ];
}
