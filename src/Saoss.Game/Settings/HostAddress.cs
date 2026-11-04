namespace Saoss.Game.Settings;

/// <summary>
/// Rules of the host name or address typed in the join menu (net.cpp readip): lower-case letters, digits, '.' and
/// '-', plus ':' for IPv6 addresses. Upper-case letters are typed as lower case, like the original.
/// </summary>
public static class HostAddress
{
    /// <summary>Longest host text; the original's buffer took 62 characters.</summary>
    public const int MaxLength = 62;

    /// <summary>The character as it is stored, or null when it is not allowed.</summary>
    public static char? Accept(char c)
    {
        var lower = char.ToLowerInvariant(c);
        return lower is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '.' or '-' or ':' ? lower : null;
    }

    /// <summary>Whether the text is a non-empty host of allowed characters within the length limit.</summary>
    public static bool IsValid(string text) => text.Length is > 0 and <= MaxLength && text.All(c => Accept(c) == c);
}
