using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;

namespace Saoss.Tests.Input;

[TestClass]
public sealed class KeyBindingEditorTests
{
    [TestMethod]
    public void Assign_FreeKey_ChangesOnlyThatAction()
    {
        var result = KeyBindingEditor.Assign(PlayerKeyBindings.Defaults, new BindingSlot(0, PlayerAction.Fire), Keys.Space);

        Assert.IsNull(result.SwappedWith);
        Assert.AreEqual(PlayerKeyBindings.Defaults[0] with { Fire = Keys.Space }, result.Bindings[0]);
        CollectionAssert.AreEqual(PlayerKeyBindings.Defaults.Skip(1).ToArray(), result.Bindings.Skip(1).ToArray());
    }

    [TestMethod]
    public void Assign_KeyOfAnotherPlayer_SwapsTheKeys()
    {
        var result = KeyBindingEditor.Assign(PlayerKeyBindings.Defaults, new BindingSlot(0, PlayerAction.Fire), Keys.W);

        Assert.AreEqual(new BindingSlot(1, PlayerAction.Thrust), result.SwappedWith);
        Assert.AreEqual(Keys.W, result.Bindings[0].Fire);
        Assert.AreEqual(Keys.RightShift, result.Bindings[1].Thrust);
    }

    [TestMethod]
    public void Assign_KeyOfAnotherActionOfTheSamePlayer_SwapsWithinThePlayer()
    {
        var result = KeyBindingEditor.Assign(PlayerKeyBindings.Defaults, new BindingSlot(2, PlayerAction.Left), Keys.L);

        Assert.AreEqual(new BindingSlot(2, PlayerAction.Right), result.SwappedWith);
        Assert.AreEqual(PlayerKeyBindings.Defaults[2] with { Left = Keys.L, Right = Keys.J }, result.Bindings[2]);
    }

    [TestMethod]
    public void Assign_ItsOwnKey_ChangesNothing()
    {
        var result = KeyBindingEditor.Assign(PlayerKeyBindings.Defaults, new BindingSlot(3, PlayerAction.Special), Keys.Decimal);

        Assert.IsNull(result.SwappedWith);
        CollectionAssert.AreEqual(PlayerKeyBindings.Defaults.ToArray(), result.Bindings.ToArray());
    }

    [TestMethod]
    public void Assign_ReservedKey_IsRefused()
    {
        Assert.ThrowsExactly<ArgumentException>(() => KeyBindingEditor.Assign(PlayerKeyBindings.Defaults, new BindingSlot(0, PlayerAction.Fire), Keys.Escape));
        Assert.ThrowsExactly<ArgumentException>(() => KeyBindingEditor.Assign(PlayerKeyBindings.Defaults, new BindingSlot(0, PlayerAction.Fire), Keys.Enter));
        Assert.IsFalse(PlayerKeyBindings.IsBindable(Keys.None));
        Assert.IsTrue(PlayerKeyBindings.IsBindable(Keys.Space));
    }

    [TestMethod]
    public void Defaults_UseEveryKeyOnce_AndNoReservedKey()
    {
        var keys = PlayerKeyBindings.Defaults.SelectMany(b => Enum.GetValues<PlayerAction>().Select(b.KeyFor)).ToArray();

        Assert.HasCount(keys.Length, keys.Distinct());
        Assert.IsTrue(keys.All(PlayerKeyBindings.IsBindable));
    }

    [TestMethod]
    public void KeyNames_AreShortAndReadable()
    {
        Assert.AreEqual("Right Shift", KeyNames.Of(Keys.RightShift));
        Assert.AreEqual("Num 8", KeyNames.Of(Keys.NumPad8));
        Assert.AreEqual("Num .", KeyNames.Of(Keys.Decimal));
        Assert.AreEqual("5", KeyNames.Of(Keys.D5));
        Assert.AreEqual("F1", KeyNames.Of(Keys.F1));
        Assert.AreEqual("W", KeyNames.Of(Keys.W));
    }
}
