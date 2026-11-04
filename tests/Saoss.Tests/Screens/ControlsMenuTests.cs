using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Screens;
using Saoss.Game.Settings;
using Saoss.Tests.Input;

namespace Saoss.Tests.Screens;

[TestClass]
public sealed class ControlsMenuTests
{
    private static readonly int FireRow = ControlsMenu.FirstActionRow + (int)PlayerAction.Fire;

    [TestMethod]
    public void Rows_ListEveryAction_ThenResetAndBack()
    {
        var actions = Enum.GetValues<PlayerAction>();
        var resetRow = ControlsMenu.ResetRow;

        Assert.AreEqual(ControlsMenu.FirstActionRow + actions.Length, resetRow);
        CollectionAssert.AreEqual(actions, Enumerable.Range(ControlsMenu.FirstActionRow, actions.Length).Select(r => ControlsMenu.ActionAt(r)!.Value).ToArray());
        Assert.IsNull(ControlsMenu.ActionAt(ControlsMenu.DeviceRow));
        Assert.IsNull(ControlsMenu.ActionAt(ControlsMenu.ResetRow));
    }

    [TestMethod]
    public void EnterOnAnAction_WaitsForAKey_AndSwapsAConflict()
    {
        var (menu, input) = Open();
        MoveTo(menu, input, FireRow);

        Press(menu, input, Keys.Enter);
        Assert.IsTrue(menu.IsWaitingForKey);
        Press(menu, input, Keys.W);

        Assert.IsFalse(menu.IsWaitingForKey);
        Assert.AreEqual(Keys.W, menu.KeyOf(PlayerAction.Fire));
        Assert.AreEqual(Keys.RightShift, menu.Controls[1].Keys!.Thrust);
        Assert.AreEqual("Swapped: Player 2 Thrust is now Right Shift", menu.Message);
    }

    [TestMethod]
    public void WaitingForAKey_RefusesReservedKeys_AndEscapeCancels()
    {
        var (menu, input) = Open();
        MoveTo(menu, input, FireRow);
        Press(menu, input, Keys.Enter);

        Press(menu, input, Keys.Enter);
        Assert.IsTrue(menu.IsWaitingForKey);
        StringAssert.Contains(menu.Message, "reserved");

        Press(menu, input, Keys.Escape);
        Assert.IsFalse(menu.IsWaitingForKey);
        Assert.AreEqual(Keys.RightShift, menu.KeyOf(PlayerAction.Fire));
        Assert.IsFalse(Press(menu, input, Keys.Down), "Escape only cancelled the wait.");
    }

    [TestMethod]
    public void Pad_NavigatesTheMenu_AndBLeaves()
    {
        var (menu, input) = Open();
        input.StepPad(0, FakeInput.Pad(dPad: FakeInput.DPad("down")));
        menu.Update(input.Tracker);
        Assert.AreEqual(ControlsMenu.DeviceRow, menu.Selection);

        input.StepPad(0, FakeInput.Pad(dPad: FakeInput.DPad("right")));
        menu.Update(input.Tracker);
        Assert.AreEqual(InputDevice.Gamepad1, menu.Controls[0].Device);

        input.StepPad(0, FakeInput.Pad(buttons: [Buttons.B]));
        Assert.IsTrue(menu.Update(input.Tracker), "B leaves like Escape.");
    }

    [TestMethod]
    public void Device_SkipsGamepadsOfOtherPlayers()
    {
        var controls = new GameSettings().Controls.Select((c, i) => i == 1 ? c with { Device = InputDevice.Gamepad1 } : c).ToArray();
        var (menu, input) = Open(controls);
        MoveTo(menu, input, ControlsMenu.DeviceRow);

        Press(menu, input, Keys.Right);
        Assert.AreEqual(InputDevice.Gamepad2, menu.Controls[0].Device);
        Press(menu, input, Keys.Left);
        Press(menu, input, Keys.Left);
        Assert.AreEqual(InputDevice.Gamepad4, menu.Controls[0].Device);
    }

    [TestMethod]
    public void PlayerRow_ShowsAnotherPlayersKeys()
    {
        var (menu, input) = Open();

        Press(menu, input, Keys.Left);

        Assert.AreEqual(3, menu.Player);
        Assert.AreEqual(Keys.NumPad8, menu.KeyOf(PlayerAction.Thrust));
    }

    [TestMethod]
    public void Reset_RestoresEveryPlayersKeys_ButKeepsDevices()
    {
        var controls = new GameSettings().Controls.Select((c, i) => c with { Device = InputDevice.Gamepad1 + i, Keys = c.Keys! with { Fire = Keys.F1 + i } }).ToArray();
        var (menu, input) = Open(controls);
        MoveTo(menu, input, ControlsMenu.ResetRow);

        Press(menu, input, Keys.Enter);

        CollectionAssert.AreEqual(PlayerKeyBindings.Defaults.ToArray(), menu.Controls.Select(c => c.Keys).ToArray());
        Assert.AreEqual(InputDevice.Gamepad3, menu.Controls[2].Device);
    }

    [TestMethod]
    public void PreviousMenu_Leaves()
    {
        var (menu, input) = Open();

        Press(menu, input, Keys.Up);

        Assert.AreEqual(ControlsMenu.BackRow, menu.Selection);
        Assert.IsTrue(Press(menu, input, Keys.Enter));
    }

    private static (ControlsMenu Menu, FakeInput Input) Open(IReadOnlyList<PlayerControlSettings>? controls = null) =>
        (new ControlsMenu(controls ?? new GameSettings().Controls), new FakeInput());

    // Press, then release, so the next press is a new one.
    private static bool Press(ControlsMenu menu, FakeInput input, Keys key)
    {
        input.Step(key);
        var left = menu.Update(input.Tracker);
        input.Step();
        menu.Update(input.Tracker);
        return left;
    }

    private static void MoveTo(ControlsMenu menu, FakeInput input, int row)
    {
        while (menu.Selection != row)
        {
            Press(menu, input, Keys.Down);
        }
    }
}
