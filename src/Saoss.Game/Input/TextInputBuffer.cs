namespace Saoss.Game.Input;

/// <summary>
/// One line of typed text (readnick, readip and the chat's write line): characters pass through
/// <see cref="Accept"/>, which may change them (lower case) or refuse them; text stops at the length limit; '\b'
/// deletes the last character. Other control characters (Enter, Tab, Escape) are ignored.
/// </summary>
/// <param name="maxLength">Longest text.</param>
/// <param name="accept">Returns the character to store, or null to refuse it.</param>
public sealed class TextInputBuffer(int maxLength, Func<char, char?> accept)
{
    private const char Backspace = '\b';

    private static readonly char[] LineBreaks = ['\r', '\n'];

    /// <summary>The text typed so far.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>Replaces the text, keeping only accepted characters within the limit.</summary>
    public void SetText(string text)
    {
        Text = string.Empty;
        Type(text.Where(c => c != Backspace).ToList());
    }

    /// <summary>Applies typed characters in order; returns true when the text changed.</summary>
    public bool Type(IReadOnlyList<char> characters)
    {
        var before = Text;
        foreach (var c in characters)
        {
            if (c == Backspace)
            {
                Text = Text.Length > 0 ? Text[..^1] : Text;
            }
            else if (!char.IsControl(c) && Text.Length < maxLength && accept(c) is { } stored)
            {
                Text += stored;
            }
        }

        return Text != before;
    }

    /// <summary>
    /// Appends pasted text as if its first line were typed (the same filter and length limit); returns true when the
    /// text changed. Tabs become spaces.
    /// </summary>
    public bool Paste(string text)
    {
        var lineEnd = text.IndexOfAny(LineBreaks);
        var line = (lineEnd < 0 ? text : text[..lineEnd]).Replace('\t', ' ');
        return Type([.. line.Where(c => c != Backspace)]);
    }

    /// <summary>Applies this update's typed characters and pasted text from <paramref name="keyboard"/>.</summary>
    public bool Edit(KeyboardTracker keyboard) => Type(keyboard.TypedCharacters) | Paste(keyboard.PastedText);

    /// <summary>Empties the text.</summary>
    public void Clear() => Text = string.Empty;

    /// <summary>The end of the text that fits in <paramref name="width"/> characters, so the caret end stays visible.</summary>
    public string VisibleTail(int width) => Text.Length <= width ? Text : Text[^width..];
}
