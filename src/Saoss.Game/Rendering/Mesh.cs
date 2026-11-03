using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering;

/// <summary>
/// Static triangle-list geometry on the GPU; replaces an OpenGL display list.
/// </summary>
public sealed class Mesh : IDisposable
{
    private readonly VertexBuffer _vertexBuffer;

    /// <summary>Uploads the given triangle-list vertices.</summary>
    public Mesh(GraphicsDevice graphicsDevice, VertexPositionNormalColor[] vertices)
    {
        _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalColor.Declaration, vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);
    }

    /// <summary>Draws the mesh with every pass of the effect, which must already have its matrices set.</summary>
    public void Draw(Effect effect)
    {
        var device = _vertexBuffer.GraphicsDevice;
        device.SetVertexBuffer(_vertexBuffer);
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount / 3);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _vertexBuffer.Dispose();
}
