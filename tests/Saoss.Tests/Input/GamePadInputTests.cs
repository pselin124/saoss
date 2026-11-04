using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Saoss.Core.Simulation;
using Saoss.Game.Input;
using Saoss.Game.Screens;

namespace Saoss.Tests.Input;

[TestClass]
public sealed class GamePadInputTests
{
    private static readonly PlayerControls Pad1Player = new(PlayerKeyBindings.Defaults[0], InputDevice.Gamepad1);

    [TestMethod]
    public void Held_MapsStickTriggersAndButtons()
    {
        Assert.AreEqual(new PlayerButtons(Thrust: true, Left: true, Right: false, Down: false, Fire: false, Special: false), GamePadMapping.Held(FakeInput.Pad(new Vector2(-0.8f, 0.8f))));
        Assert.AreEqual(new PlayerButtons(false, false, true, true, false, false), GamePadMapping.Held(FakeInput.Pad(new Vector2(0.9f, -0.9f))));
        Assert.AreEqual(new PlayerButtons(false, false, false, false, true, true), GamePadMapping.Held(FakeInput.Pad(leftTrigger: 0.7f, rightTrigger: 1f)));
        Assert.AreEqual(new PlayerButtons(true, false, false, false, true, true), GamePadMapping.Held(FakeInput.Pad(buttons: [Buttons.A, Buttons.B, Buttons.RightShoulder])));
    }

    [TestMethod]
    public void Held_DPadSteersLikeTheStick()
    {
        Assert.IsTrue(GamePadMapping.Held(FakeInput.Pad(dPad: FakeInput.DPad("up"))).Thrust);
        Assert.IsTrue(GamePadMapping.Held(FakeInput.Pad(dPad: FakeInput.DPad("down"))).Down);
        Assert.IsTrue(GamePadMapping.Held(FakeInput.Pad(dPad: FakeInput.DPad("left"))).Left);
        Assert.IsTrue(GamePadMapping.Held(FakeInput.Pad(dPad: FakeInput.DPad("right"))).Right);
    }

    [TestMethod]
    public void Held_IgnoresSmallStickAndTriggerMovement()
    {
        Assert.AreEqual(default, GamePadMapping.Held(FakeInput.Pad(new Vector2(0.3f, -0.3f), leftTrigger: 0.2f, rightTrigger: 0.4f)));
    }

    [TestMethod]
    public void Held_DisconnectedPad_PressesNothing()
    {
        Assert.AreEqual(default, GamePadMapping.Held(GamePadState.Default));
        Assert.AreEqual(default, GamePadMapping.Held(default));
        Assert.IsFalse(new FakeInput().Step().Tracker.PadState(3).IsConnected);
    }

    [TestMethod]
    public void LocalPlayerInput_ReadsTheAssignedPad_NotTheKeyboard()
    {
        var input = new FakeInput();
        var player = new LocalPlayerInput(Pad1Player);

        input.Step(Keys.Up, Keys.RightShift);
        Assert.AreEqual(default, player.Read(input.Tracker));

        input.StepPad(0, FakeInput.Pad(new Vector2(0.9f, 0f), buttons: [Buttons.A]));
        Assert.AreEqual(new ShipControls { Right = true, Fire = true }, player.Read(input.Tracker));
    }

    [TestMethod]
    public void LocalPlayerInput_LatchesTheSpecialButtonOfAPad()
    {
        var input = new FakeInput();
        var player = new LocalPlayerInput(Pad1Player);
        input.StepPad(0, FakeInput.Pad(buttons: [Buttons.B]));

        var issued = player.Read(input.Tracker);
        player.AfterStep(issued, issued with { Special = false });
        input.Step();

        Assert.IsTrue(issued.Special);
        Assert.IsFalse(player.Read(input.Tracker).Special, "Held after the ship cleared it.");
        input.StepPad(0, FakeInput.Pad());
        player.Read(input.Tracker);
        input.StepPad(0, FakeInput.Pad(buttons: [Buttons.B]));
        Assert.IsTrue(player.Read(input.Tracker).Special, "Pressed again.");
    }

    [TestMethod]
    public void ShipPurchaseMenu_IsDrivenByTheBuyersPad()
    {
        var input = new FakeInput();
        var menu = new ShipPurchaseMenu();

        input.StepPad(0, FakeInput.Pad(dPad: FakeInput.DPad("down")));
        Assert.IsFalse(menu.Update(input.Tracker, Pad1Player));
        input.Step();
        Assert.IsFalse(menu.Update(input.Tracker, Pad1Player), "Still held: no second move.");
        Assert.AreEqual(1, menu.Selection);

        input.StepPad(0, FakeInput.Pad(buttons: [Buttons.A]));
        Assert.IsTrue(menu.Update(input.Tracker, Pad1Player));
    }

    [TestMethod]
    public void MenuKeys_MapPadToArrowsEnterAndEscape()
    {
        CollectionAssert.AreEquivalent(new[] { Keys.Up, Keys.Left }, GamePadMenuKeys.Held(FakeInput.Pad(new Vector2(-1f, 1f))).ToArray());
        CollectionAssert.AreEquivalent(new[] { Keys.Down }, GamePadMenuKeys.Held(FakeInput.Pad(dPad: FakeInput.DPad("down"))).ToArray());
        CollectionAssert.AreEquivalent(new[] { Keys.Right }, GamePadMenuKeys.Held(FakeInput.Pad(dPad: FakeInput.DPad("right"))).ToArray());
        CollectionAssert.AreEquivalent(new[] { Keys.Enter }, GamePadMenuKeys.Held(FakeInput.Pad(buttons: [Buttons.A])).ToArray());
        CollectionAssert.AreEquivalent(new[] { Keys.Enter }, GamePadMenuKeys.Held(FakeInput.Pad(buttons: [Buttons.Start])).ToArray());
        CollectionAssert.AreEquivalent(new[] { Keys.Escape }, GamePadMenuKeys.Held(FakeInput.Pad(buttons: [Buttons.B])).ToArray());
        CollectionAssert.AreEquivalent(new[] { Keys.Escape }, GamePadMenuKeys.Held(FakeInput.Pad(buttons: [Buttons.Back])).ToArray());
        Assert.IsFalse(GamePadMenuKeys.Held(GamePadState.Default).Any());
    }

    [TestMethod]
    public void Tracker_PadPressesMenuKeysOnce_ButNotPlayerKeys()
    {
        var input = new FakeInput();

        input.StepPad(2, FakeInput.Pad(buttons: [Buttons.A]));
        Assert.IsTrue(input.Tracker.WasPressed(Keys.Enter));
        Assert.IsFalse(input.Tracker.WasKeyPressed(Keys.Enter));
        Assert.IsFalse(input.Tracker.IsDown(Keys.Enter));

        input.Step();
        Assert.IsFalse(input.Tracker.WasPressed(Keys.Enter), "Held, not pressed again.");

        input.StepPad(2, FakeInput.Pad());
        input.StepPad(2, FakeInput.Pad(dPad: FakeInput.DPad("up")));
        Assert.IsTrue(input.Tracker.WasPressed(Keys.Up));
        Assert.IsFalse(input.Tracker.IsDown(Keys.Up), "A keyboard player on the arrows does not thrust.");
    }

    [TestMethod]
    public void Tracker_ReportsPadButtonPresses_AndKeyboardKeys()
    {
        var input = new FakeInput();

        input.StepPad(1, FakeInput.Pad(buttons: [Buttons.Start]));
        Assert.IsTrue(input.Tracker.WasAnyPadPressed(Buttons.Start));
        input.Step();
        Assert.IsFalse(input.Tracker.WasAnyPadPressed(Buttons.Start));

        input.Step(Keys.Q);
        Assert.AreEqual(Keys.Q, input.Tracker.FirstPressedKey());
        input.Step(Keys.Q);
        Assert.IsNull(input.Tracker.FirstPressedKey());
    }

    [TestMethod]
    public void Tracker_IgnoresPadsWhileTheWindowIsInactive()
    {
        var input = new FakeInput();
        input.StepPad(0, FakeInput.Pad(buttons: [Buttons.A]));

        input.Tracker.Update(isWindowActive: false);

        Assert.IsFalse(input.Tracker.PadState(0).IsButtonDown(Buttons.A));
    }
}
