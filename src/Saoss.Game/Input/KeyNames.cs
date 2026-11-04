using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Short readable names of keys for the controls menu, such as "Right Shift", "Num 8" or "5". Punctuation keys get
/// words, as the menu font has no glyphs for most punctuation.
/// </summary>
public static class KeyNames
{
    private static readonly Dictionary<Keys, string> Special = new()
    {
        [Keys.Decimal] = "Num .",
        [Keys.Add] = "Num Plus",
        [Keys.Subtract] = "Num Minus",
        [Keys.Multiply] = "Num Star",
        [Keys.Divide] = "Num Slash",
        [Keys.OemPlus] = "Plus",
        [Keys.OemMinus] = "Minus",
        [Keys.OemComma] = "Comma",
        [Keys.OemPeriod] = "Period",
        [Keys.OemQuestion] = "Slash",
        [Keys.OemSemicolon] = "Semicolon",
        [Keys.OemQuotes] = "Quote",
        [Keys.OemOpenBrackets] = "Left Bracket",
        [Keys.OemCloseBrackets] = "Right Bracket",
        [Keys.OemPipe] = "Backslash",
        [Keys.OemBackslash] = "Backslash 2",
        [Keys.OemTilde] = "Tilde",
    };

    /// <summary>The name shown for <paramref name="key"/>.</summary>
    public static string Of(Keys key)
    {
        if (Special.TryGetValue(key, out var name))
        {
            return name;
        }

        if (key is >= Keys.D0 and <= Keys.D9)
        {
            return ((char)('0' + (key - Keys.D0))).ToString();
        }

        if (key is >= Keys.NumPad0 and <= Keys.NumPad9)
        {
            return "Num " + (char)('0' + (key - Keys.NumPad0));
        }

        return SplitWords(key.ToString());
    }

    // "RightShift" -> "Right Shift"; "F1" stays.
    private static string SplitWords(string text)
    {
        var builder = new StringBuilder(text.Length + 4);
        for (var i = 0; i < text.Length; i++)
        {
            if (i > 0 && char.IsUpper(text[i]) && char.IsLower(text[i - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(text[i]);
        }

        return builder.ToString();
    }
}
