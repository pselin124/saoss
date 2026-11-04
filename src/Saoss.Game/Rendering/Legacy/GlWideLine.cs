using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Expands a line segment into a screen-space quad for glLineWidth above one pixel, which MonoGame cannot draw.
/// The quad is perpendicular to the segment on screen, like GL's antialiased (GL_LINE_SMOOTH) wide lines.
/// </summary>
public static class GlWideLine
{
    /// <summary>
    /// Window height the original's line widths are taken to be in pixels for (1024x768 was common in 2003). Widths
    /// scale with the window height so lines keep their thickness relative to the picture at any resolution.
    /// </summary>
    public const float ReferenceHeight = 768f;

    private const float MinClipW = 1e-5f;
    private const float MinPixelLength = 1e-4f;

    /// <summary>The width in pixels of a GL line width at the given viewport height.</summary>
    public static float ScaledWidth(float width, int viewportHeight) => width * viewportHeight / ReferenceHeight;

    /// <summary>
    /// The four clip-space corners (start-right, start-left, end-left, end-right) of the segment from
    /// <paramref name="start"/> to <paramref name="end"/> widened to <paramref name="widthPixels"/>, or null when an
    /// end is behind the camera or the segment has no length on screen.
    /// </summary>
    public static (Vector4 StartRight, Vector4 StartLeft, Vector4 EndLeft, Vector4 EndRight)? Expand(
        Vector4 start, Vector4 end, float widthPixels, Vector2 viewportSize)
    {
        if (start.W <= MinClipW || end.W <= MinClipW)
        {
            return null;
        }

        var halfViewport = viewportSize / 2f;
        var pixelDelta = ((new Vector2(end.X, end.Y) / end.W) - (new Vector2(start.X, start.Y) / start.W)) * halfViewport;
        var length = pixelDelta.Length();
        if (length < MinPixelLength)
        {
            return null;
        }

        var normalPixels = new Vector2(-pixelDelta.Y, pixelDelta.X) / length * (widthPixels / 2f);
        var normalNdc = normalPixels / halfViewport;
        var startOffset = new Vector4(normalNdc * start.W, 0, 0);
        var endOffset = new Vector4(normalNdc * end.W, 0, 0);
        return (start - startOffset, start + startOffset, end + endOffset, end - endOffset);
    }
}
