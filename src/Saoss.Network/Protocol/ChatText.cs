using System.Text;

namespace Saoss.Network.Protocol;

/// <summary>Chat line rules: at most 500 characters [MAXCHATKIRJOTUSMERKIT], no control characters.</summary>
public static class ChatText
{
    /// <summary>Longest chat line.</summary>
    public const int MaxLength = 500;

    /// <summary>Removes control characters and lone surrogates and cuts the text to <see cref="MaxLength"/>.</summary>
    public static string Sanitize(string? text)
    {
        var builder = new StringBuilder();
        foreach (var rune in (text ?? string.Empty).EnumerateRunes())
        {
            if (Rune.IsControl(rune) || rune == Rune.ReplacementChar)
            {
                continue;
            }

            if (builder.Length + rune.Utf16SequenceLength > MaxLength)
            {
                break;
            }

            builder.Append(rune.ToString());
        }

        return builder.ToString().Trim();
    }
}
