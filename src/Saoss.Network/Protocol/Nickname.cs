namespace Saoss.Network.Protocol;

/// <summary>
/// Nickname rules of the original's nickname entry (net.cpp readnick): letters including the Nordic ones, digits,
/// '.', '-' and space.
/// </summary>
public static class Nickname
{
    /// <summary>Longest nickname; the original stored ten characters plus the terminator [netnick[11]].</summary>
    public const int MaxLength = 10;

    /// <summary>Used when nothing allowed is left after sanitizing.</summary>
    public const string Fallback = "player";

    /// <summary>Whether the character may appear in a nickname.</summary>
    public static bool IsAllowed(char c) =>
        c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9')
            or '.' or '-' or ' ' or 'ä' or 'ö' or 'å' or 'Ä' or 'Ö' or 'Å';

    /// <summary>Whether the text is a non-empty nickname of allowed characters within the length limit.</summary>
    public static bool IsValid(string text) => text.Length is > 0 and <= MaxLength && text.All(IsAllowed);

    /// <summary>Drops characters that are not allowed and cuts the rest to <see cref="MaxLength"/>.</summary>
    public static string Sanitize(string? text)
    {
        var kept = new string([.. (text ?? string.Empty).Where(IsAllowed).Take(MaxLength)]);
        return kept.Length > 0 ? kept : Fallback;
    }
}
