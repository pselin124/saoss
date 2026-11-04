using Saoss.Game.Screens.Network;
using Saoss.Network.Sessions;

namespace Saoss.Tests.Screens.Network;

[TestClass]
public sealed class ChatViewTests
{
    [TestMethod]
    public void Rows_PadTheSenderAndPutTheNewestFirst()
    {
        var rows = ChatView.Rows([new ChatLine("tgo", "hi"), new ChatLine("cinere", "moi")]);

        CollectionAssert.AreEqual(new[] { "cinere    :moi", "tgo       :hi" }, rows.ToArray());
    }

    [TestMethod]
    public void LongLines_ContinueOnRowsWithTheSamePrefix()
    {
        var text = new string('a', ChatView.TextPerRow) + "bc";

        var rows = ChatView.Rows([new ChatLine("tgo", text)]);

        Assert.HasCount(2, rows);
        Assert.AreEqual("tgo       :bc", rows[0]);
        Assert.AreEqual("tgo       :" + new string('a', ChatView.TextPerRow), rows[1]);
    }

    [TestMethod]
    public void Rows_KeepOnlyTheNewestRows()
    {
        var lines = Enumerable.Range(0, 30).Select(i => new ChatLine("p", i.ToString(System.Globalization.CultureInfo.InvariantCulture))).ToList();

        var rows = ChatView.Rows(lines);

        Assert.HasCount(ChatView.MaxRows, rows);
        Assert.AreEqual("p         :29", rows[0]);
        Assert.AreEqual("p         :7", rows[^1]);
    }
}
