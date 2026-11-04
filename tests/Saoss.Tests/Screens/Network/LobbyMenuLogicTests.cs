using Saoss.Core;
using Saoss.Game.Screens.Network;
using Saoss.Network.Lobby;

namespace Saoss.Tests.Screens.Network;

[TestClass]
public sealed class LobbyMenuLogicTests
{
    private const int MenuItems = 4;
    private const int TableColumns = 2;

    [TestMethod]
    public void Cursor_WrapsBetweenColumnsAndRows()
    {
        var cursor = new LobbyCursor(MenuItems, TableColumns);

        cursor.Left(tableRows: 16);
        Assert.AreEqual(2, cursor.Column);
        cursor.Up(tableRows: 16);
        Assert.AreEqual(15, cursor.Row);
        cursor.Right(tableRows: 16);

        Assert.IsTrue(cursor.InMenu);
        Assert.AreEqual(MenuItems - 1, cursor.Row, "The row is kept inside the menu.");
        cursor.Down(tableRows: 16);
        Assert.AreEqual(0, cursor.Row);
    }

    [TestMethod]
    public void Cursor_StaysInsideAShrinkingTable()
    {
        var cursor = new LobbyCursor(MenuItems, TableColumns);
        cursor.Right(tableRows: 16);
        cursor.Up(tableRows: 16);

        cursor.Clamp(tableRows: 3);

        Assert.AreEqual(2, cursor.Row);
    }

    [TestMethod]
    public void Escape_SelectsTheLastMenuItem()
    {
        var cursor = new LobbyCursor(MenuItems, TableColumns);
        cursor.Right(tableRows: 16);

        cursor.SelectLastMenuItem();

        Assert.IsTrue(cursor.InMenu);
        Assert.AreEqual(MenuItems - 1, cursor.Row);
    }

    [TestMethod]
    public void MaxPlayers_SkipsLimitsTheHumansDoNotFitIn()
    {
        var lobby = new LobbyState(["a", "b", "c"]);
        lobby.TrySetMaxPlayers(3);

        HostSettingsMenu.StepMaxPlayers(lobby, up: false);

        Assert.AreEqual(LobbyState.SlotCount, lobby.MaxPlayers, "2 is too few for three humans, so the limit wraps to 16.");
        HostSettingsMenu.StepMaxPlayers(lobby, up: true);
        Assert.AreEqual(3, lobby.MaxPlayers);
    }

    [TestMethod]
    public void ModeAndPlayerCount_Wrap()
    {
        Assert.AreEqual(GameMode.Deathmatch, HostSettingsMenu.StepMode(GameMode.EatTheMushroom, up: true));
        Assert.AreEqual(GameMode.EatTheMushroom, HostSettingsMenu.StepMode(GameMode.Deathmatch, up: false));
        Assert.AreEqual(1, HostSetupMenu.StepPlayerCount(4, MenuDirection.Right));
        Assert.AreEqual(4, HostSetupMenu.StepPlayerCount(1, MenuDirection.Left));
        Assert.AreEqual(3, HostSetupMenu.StepPlayerCount(2, MenuDirection.Enter));
    }
}
