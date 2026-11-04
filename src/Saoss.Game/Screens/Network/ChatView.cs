using Saoss.Network.Sessions;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The chat history as the chat screen shows it (chat.cpp lisaaRivi): every row starts with the sender padded to
/// ten characters and a colon, long lines continue on further rows with the same prefix, and the newest rows come
/// first.
/// </summary>
public static class ChatView
{
    /// <summary>History rows on screen [MAXCHATRIVIT].</summary>
    public const int MaxRows = 23;

    /// <summary>Characters of text per row after the prefix [MAXCHATMERKIT - 12].</summary>
    public const int TextPerRow = 73;

    private const int SenderWidth = 10;

    /// <summary>The last <paramref name="maxRows"/> rows, newest first.</summary>
    public static IReadOnlyList<string> Rows(IReadOnlyList<ChatLine> lines, int maxRows = MaxRows)
    {
        var rows = new List<string>();
        for (var i = lines.Count - 1; i >= 0 && rows.Count < maxRows; i--)
        {
            var wrapped = Wrap(lines[i]);
            for (var j = wrapped.Count - 1; j >= 0 && rows.Count < maxRows; j--)
            {
                rows.Add(wrapped[j]);
            }
        }

        return rows;
    }

    /// <summary>The prefix of a sender's rows: the nickname padded to ten characters and a colon.</summary>
    public static string Prefix(string sender) => sender.PadRight(SenderWidth)[..SenderWidth] + ":";

    private static List<string> Wrap(ChatLine line)
    {
        var prefix = Prefix(line.Sender);
        var rows = new List<string>();
        for (var start = 0; start < line.Text.Length || rows.Count == 0; start += TextPerRow)
        {
            rows.Add(prefix + line.Text.Substring(start, Math.Min(TextPerRow, Math.Max(0, line.Text.Length - start))));
        }

        return rows;
    }
}
