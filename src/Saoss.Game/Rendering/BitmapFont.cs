using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering;

/// <summary>
/// Draws text in 3D space with the original bitmap font. The font image is white glyphs on black,
/// so it is drawn with <see cref="LegacyGraphics.TextBlend"/> to make the black transparent.
/// </summary>
public sealed class BitmapFont(Texture2D texture)
{
    private const int VerticesPerGlyph = 6;

    private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[64 * VerticesPerGlyph];

    /// <summary>
    /// Draws one line of text whose first glyph is centered at the origin of <paramref name="world"/>.
    /// The effect must already have View and Projection set; texture and lighting state are set here.
    /// </summary>
    public void Draw(BasicEffect effect, Matrix world, string text, Color color, float size = 1f)
    {
        if (text.Length == 0)
        {
            return;
        }

        EnsureCapacity(text.Length * VerticesPerGlyph);
        var penX = 0f;
        var count = 0;
        foreach (var c in text)
        {
            var (uvTopLeft, uvBottomRight) = GlyphMetrics.TextureBounds(c);
            var left = penX - size;
            var right = penX + size;
            var bottomLeft = new VertexPositionColorTexture(new Vector3(left, -size, 0), color, new Vector2(uvTopLeft.X, uvBottomRight.Y));
            var bottomRight = new VertexPositionColorTexture(new Vector3(right, -size, 0), color, uvBottomRight);
            var topRight = new VertexPositionColorTexture(new Vector3(right, size, 0), color, new Vector2(uvBottomRight.X, uvTopLeft.Y));
            var topLeft = new VertexPositionColorTexture(new Vector3(left, size, 0), color, uvTopLeft);
            _vertices[count++] = bottomLeft;
            _vertices[count++] = bottomRight;
            _vertices[count++] = topRight;
            _vertices[count++] = bottomLeft;
            _vertices[count++] = topRight;
            _vertices[count++] = topLeft;
            penX += GlyphMetrics.Advance(c, size);
        }

        effect.World = world;
        effect.LightingEnabled = false;
        effect.TextureEnabled = true;
        effect.Texture = texture;
        effect.VertexColorEnabled = true;
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, count / 3);
        }
    }

    private void EnsureCapacity(int vertexCount)
    {
        if (_vertices.Length < vertexCount)
        {
            _vertices = new VertexPositionColorTexture[vertexCount];
        }
    }
}
