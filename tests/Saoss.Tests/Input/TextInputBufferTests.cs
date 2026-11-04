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

    [TestMethod]
    public void Paste_FiltersLikeTyping_AndStopsAtTheLimit()
    {
        var buffer = Nicknames();
        buffer.SetText("ab");

        var changed = buffer.Paste("c<d\b-ef.ghijklmn");

        Assert.IsTrue(changed);
        Assert.AreEqual("abcd-ef.gh", buffer.Text);
    }

    [TestMethod]
    public void Paste_KeepsOnlyTheFirstLine_WithTabsAsSpaces()
    {
        var buffer = new TextInputBuffer(ChatText.MaxLength, c => c);

        buffer.Paste("hello\tthere\r\nsecond line");

        Assert.AreEqual("hello there", buffer.Text);
    }

    [TestMethod]
    public void Paste_HostAddress_IsLowerCasedWithoutSpaces()
    {
        var buffer = new TextInputBuffer(HostAddress.MaxLength, HostAddress.Accept);

        buffer.Paste(" 192.168.1.20 \n");

        Assert.AreEqual("192.168.1.20", buffer.Text);
    }

    [TestMethod]
    public void Paste_OfEmptyText_ChangesNothing()
    {
        var buffer = Nicknames();

        Assert.IsFalse(buffer.Paste(string.Empty));
        Assert.IsFalse(buffer.Paste("\r\nabc"));
        Assert.AreEqual(string.Empty, buffer.Text);
    }

    private static TextInputBuffer Nicknames() => new(Nickname.MaxLength, c => Nickname.IsAllowed(c) ? c : null);
}
