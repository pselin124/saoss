using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws text in the 3D scene with the original bitmap font, a port of <c>gl_println</c>:
/// one quad per character centered on the pen, which then advances by the character's width.
/// </summary>
public sealed class GlText(LegacyGl gl, Texture2D font)
{
    /// <summary>
    /// Draws <paramref name="text"/> starting at the given position; <paramref name="size"/> is the half size
    /// of a glyph quad. Uses the current color and blend state.
    /// </summary>
    public void Println(float x, float y, float z, string text, float size = 1f)
    {
        gl.BindTexture(font);
        gl.PushMatrix();
        gl.Translate(x, y, z);
        foreach (var c in text)
        {
            var (topLeft, bottomRight) = GlyphMetrics.TextureBounds(c);
            gl.Begin(GlPrimitive.Quads);
            gl.Normal(0, 0, 1);
            gl.TexCoord(topLeft.X, bottomRight.Y);
            gl.Vertex(-size, -size, 0);
            gl.TexCoord(bottomRight.X, bottomRight.Y);
            gl.Vertex(size, -size, 0);
            gl.TexCoord(bottomRight.X, topLeft.Y);
            gl.Vertex(size, size, 0);
            gl.TexCoord(topLeft.X, topLeft.Y);
            gl.Vertex(-size, size, 0);
            gl.End();
            gl.Translate(GlyphMetrics.Advance(c, size), 0, 0);
        }

        gl.PopMatrix();
    }
}
