using Saoss.Game.Input;
using Saoss.Game.Settings;
using Saoss.Network.Protocol;

namespace Saoss.Tests.Input;

[TestClass]
public sealed class TextInputBufferTests
{
    [TestMethod]
    public void Type_KeepsAcceptedCharactersAndHandlesBackspace()
    {
        var buffer = Nicknames();

        buffer.Type(['a', '<', 'B', '\b', 'c', ' ', '1']);

        Assert.AreEqual("ac 1", buffer.Text);
    }

    [TestMethod]
    public void Type_StopsAtTheLengthLimit()
    {
        var buffer = Nicknames();

        buffer.Type([.. "abcdefghijklmno"]);

        Assert.AreEqual("abcdefghij", buffer.Text);
    }

    [TestMethod]
    public void Type_IgnoresEnterTabAndEscape_AndBackspaceOnEmptyText()
    {
        var buffer = Nicknames();

        var changed = buffer.Type(['\b', '\r', '\t', '\u001b']);

        Assert.IsFalse(changed);
        Assert.AreEqual(string.Empty, buffer.Text);
    }

    [TestMethod]
    public void HostAddress_IsTypedInLowerCase_WithoutSpaces()
    {
        var buffer = new TextInputBuffer(HostAddress.MaxLength, HostAddress.Accept);

        buffer.SetText("My-Host.LAN 9");

        Assert.AreEqual("my-host.lan9", buffer.Text);
        Assert.IsTrue(HostAddress.IsValid(buffer.Text));
        Assert.IsFalse(HostAddress.IsValid("My-Host"));
        Assert.IsFalse(HostAddress.IsValid(string.Empty));
    }

    [TestMethod]
    public void VisibleTail_ShowsTheEndOfLongText()
    {
        var buffer = new TextInputBuffer(ChatText.MaxLength, c => c);
        buffer.SetText("hello world");

        Assert.AreEqual("world", buffer.VisibleTail(5));
        Assert.AreEqual("hello world", buffer.VisibleTail(85));
    }

    private static TextInputBuffer Nicknames() => new(Nickname.MaxLength, c => Nickname.IsAllowed(c) ? c : null);
}
