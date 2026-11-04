using Saoss.Tests.Rendering.CameraVisibility;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Checks that the match camera keeps every flying ship on screen in all-AI matches, for both camera modes and
/// 4:3, 16:9 and 21:9 windows.
/// </summary>
[TestClass]
public sealed class GameCameraVisibilityTests
{
    private const int Seed = 1;
    private const int Ticks = 6_000;
    private const int MaxOffScreenStreak = 30;
    private const double MaxOffScreenPercent = 0.5;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(4)]
    [DataRow(8)]
    public void FlyingShipsStayOnScreen(int players)
    {
        var results = CameraVisibilityRun.Measure([Seed], players, Ticks);

        foreach (var stats in results)
        {
            TestContext.WriteLine(stats.ToString());
            Assert.IsLessThanOrEqualTo(MaxOffScreenStreak, stats.LongestStreak, stats.ToString());
            Assert.IsLessThan(MaxOffScreenPercent, stats.OffScreenPercent, stats.ToString());
        }
    }
}
