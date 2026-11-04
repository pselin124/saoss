using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering;

/// <summary>
/// Layout of the original font texture: a 16x16 grid of 64 px ASCII cells in a 1024 px image,
/// with hand-tuned per-character widths. A glyph is a 2x2 unit quad centered on the pen position.
/// </summary>
public static class GlyphMetrics
{
    /// <summary>Character drawn for anything outside printable ASCII.</summary>
    public const char FallbackCharacter = '?';

    private const int FirstPrintable = 32;
    private const int LastPrintable = 126;
    private const float CellPixels = 64f;
    private const float TexturePixels = 1024f;

    // Advance per character as a fraction of the 2-unit glyph quad (original letterwidth table).
    private static readonly float[] AdvanceFactors =
    [
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
        0.5f, 0.2f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.2f, 0.45f, 0.2f, 0.5f,
        0.6f, 0.5f, 0.6f, 0.6f, 0.65f, 0.65f, 0.6f, 0.65f, 0.6f, 0.6f, 0.2f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f,
        1.0f, 0.7f, 0.6f, 0.7f, 0.7f, 0.65f, 0.6f, 0.7f, 0.8f, 0.6f, 0.7f, 0.7f, 0.6f, 0.9f, 0.85f, 0.8f,
        0.6f, 0.9f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 1.0f, 0.8f, 0.7f, 0.8f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.5f, 0.6f, 0.6f, 0.2f, 0.4f, 0.6f, 0.2f, 0.8f, 0.5f, 0.55f,
        0.55f, 0.55f, 0.5f, 0.55f, 0.55f, 0.55f, 0.6f, 0.8f, 0.6f, 0.6f, 0.6f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
    ];

    /// <summary>Returns the character actually present in the font for <paramref name="c"/>.</summary>
    public static char Normalize(char c) => c is >= (char)FirstPrintable and <= (char)LastPrintable ? c : FallbackCharacter;

    /// <summary>Horizontal pen advance after drawing <paramref name="c"/> at the given glyph size.</summary>
    public static float Advance(char c, float size = 1f) => 2f * size * AdvanceFactors[Normalize(c)];

    /// <summary>Total pen advance for a whole line of text.</summary>
    public static float MeasureWidth(string text, float size = 1f)
    {
        var width = 0f;
        foreach (var c in text)
        {
            width += Advance(c, size);
        }

        return width;
    }

    /// <summary>
    /// Pen start that centers the drawn line on x = 0. Glyphs sit in the middle of their cells, so the line's
    /// ink runs from half the first advance before the start to half the last advance after the last pen position.
    /// </summary>
    public static float CenteredStart(string text, float size = 1f)
    {
        if (text.Length == 0)
        {
            return 0f;
        }

        var penSpan = MeasureWidth(text, size) - ((Advance(text[0], size) + Advance(text[^1], size)) / 2f);
        return -penSpan / 2f;
    }

    /// <summary>
    /// Texture coordinates of the glyph cell, inset slightly so the grid lines in the image are not sampled.
    /// </summary>
    public static (Vector2 TopLeft, Vector2 BottomRight) TextureBounds(char c)
    {
        var code = Normalize(c);
        var column = code & 15;
        var row = code >> 4;
        var left = (column * CellPixels + 2f) / TexturePixels;
        var right = ((column + 1) * CellPixels - 1f) / TexturePixels;
        var top = (row * CellPixels + 2f) / TexturePixels;
        var bottom = ((row + 1) * CellPixels - 1f) / TexturePixels;
        return (new Vector2(left, top), new Vector2(right, bottom));
    }
}
