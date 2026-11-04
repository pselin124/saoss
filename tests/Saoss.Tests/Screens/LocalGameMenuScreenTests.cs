using Saoss.Core.Simulation;
using Saoss.Game.Screens;

namespace Saoss.Tests.Screens;

/// <summary>
/// Tests which local game menu entries start which kind of match.
/// </summary>
[TestClass]
public sealed class LocalGameMenuScreenTests
{
    [TestMethod]
    public void GameKindOf_MapsNewGameEntriesToTheOriginalMenuPositions()
    {
        foreach (var item in Enum.GetValues<LocalGameMenuItem>())
        {
            var kind = LocalGameMenuScreen.GameKindOf(item);
            if (kind != null)
            {
                Assert.AreEqual((int)item, (int)kind.Value, item.ToString());
            }
        }
    }

    [TestMethod]
    public void GameKindOf_ResumeSettingsAndExit_StartNoGame()
    {
        Assert.IsNull(LocalGameMenuScreen.GameKindOf(LocalGameMenuItem.ResumeGame));
        Assert.IsNull(LocalGameMenuScreen.GameKindOf(LocalGameMenuItem.Settings));
        Assert.IsNull(LocalGameMenuScreen.GameKindOf(LocalGameMenuItem.ExitMenu));
    }

    [TestMethod]
    public void GameKindOf_CoversEveryLocalGameKind()
    {
        var kinds = Enum.GetValues<LocalGameMenuItem>().Select(LocalGameMenuScreen.GameKindOf).OfType<LocalGameKind>();

        CollectionAssert.AreEquivalent(Enum.GetValues<LocalGameKind>(), kinds.ToArray());
    }
}
