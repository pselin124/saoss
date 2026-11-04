using Saoss.Game.Settings;

namespace Saoss.Tests.Settings;

/// <summary>
/// Tests the value steps of the settings menus and the resolution list.
/// </summary>
[TestClass]
public sealed class SettingsStepsTests
{
    [TestMethod]
    [DataRow(2, true, 3)]
    [DataRow(16, true, 2)]
    [DataRow(2, false, 16)]
    public void PlayerCount_WrapsBetweenTwoAndSixteen(int count, bool increase, int expected) =>
        Assert.AreEqual(expected, SettingsSteps.PlayerCount(count, increase));

    [TestMethod]
    [DataRow(400, true, 425)]
    [DataRow(1000, true, 25)]
    [DataRow(25, false, 1000)]
    [DataRow(20, true, 45)]
    public void Credits_StepsOf25WithWrapAround(int credits, bool increase, int expected) =>
        Assert.AreEqual(expected, SettingsSteps.Credits(credits, increase));

    [TestMethod]
    [DataRow(95, true, 100)]
    [DataRow(100, true, 110)]
    [DataRow(200, true, 50)]
    [DataRow(110, false, 100)]
    [DataRow(100, false, 95)]
    [DataRow(50, false, 200)]
    public void TicksPerSecond_FiveBelowHundredTenAbove(int ticks, bool increase, int expected) =>
        Assert.AreEqual(expected, SettingsSteps.TicksPerSecond(ticks, increase));

    [TestMethod]
    [DataRow(80, true, false, 85)]
    [DataRow(80, true, true, 81)]
    [DataRow(100, true, false, 0)]
    [DataRow(0, false, true, 100)]
    public void Volume_StepsOfFiveOrOneWithWrapAround(int volume, bool increase, bool fine, int expected) =>
        Assert.AreEqual(expected, SettingsSteps.Volume(volume, increase, fine));

    [TestMethod]
    [DataRow(3, true, 0)]
    [DataRow(0, false, 3)]
    [DataRow(1, true, 2)]
    public void Detail_WrapsBetweenZeroAndThree(int detail, bool increase, int expected) =>
        Assert.AreEqual(expected, SettingsSteps.Detail(detail, increase));

    [TestMethod]
    public void Team_CountsUpAndWraps()
    {
        Assert.AreEqual(1, SettingsSteps.Team(0));
        Assert.AreEqual(0, SettingsSteps.Team(GameSettings.MaxTeam));
    }

    [TestMethod]
    public void Resolutions_AddDesktopAndFittingWidescreenSizesLargestFirst()
    {
        var modes = new[] { new Resolution(800, 600), new Resolution(1920, 1080), new Resolution(1024, 768), new Resolution(1920, 1080) };

        var list = ResolutionOptions.Build(modes, new Resolution(1920, 1080));

        CollectionAssert.AreEqual(
            new[] { new Resolution(1920, 1080), new Resolution(1600, 900), new Resolution(1280, 720), new Resolution(1024, 768), new Resolution(800, 600) },
            list.ToArray());
    }

    [TestMethod]
    public void Resolutions_RightGoesLargerLeftSmallerWithWrapAround()
    {
        var list = ResolutionOptions.Build([new Resolution(800, 600)], new Resolution(1600, 900));
        var start = ResolutionOptions.IndexOf(list, new Resolution(1280, 720));

        Assert.AreEqual(new Resolution(1600, 900), list[ResolutionOptions.Step(start, list.Count, larger: true)]);
        Assert.AreEqual(new Resolution(800, 600), list[ResolutionOptions.Step(start, list.Count, larger: false)]);
        Assert.AreEqual(list.Count - 1, ResolutionOptions.Step(0, list.Count, larger: true));
        Assert.AreEqual(0, ResolutionOptions.Step(list.Count - 1, list.Count, larger: false));
    }

    [TestMethod]
    public void Resolutions_UnlistedSize_StartsAtFirstNarrowerSize()
    {
        var list = ResolutionOptions.Build([new Resolution(800, 600)], new Resolution(1600, 900));

        Assert.AreEqual(new Resolution(1280, 720), list[ResolutionOptions.IndexOf(list, new Resolution(1500, 1000))]);
        Assert.AreEqual(list.Count - 1, ResolutionOptions.IndexOf(list, new Resolution(640, 480)));
    }
}
