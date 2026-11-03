using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering;

/// <summary>
/// Vertex with position, normal and color, used for lit, untextured models.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color) : IVertexType
{
    /// <summary>Layout shared by all vertices of this type.</summary>
    public static readonly VertexDeclaration Declaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0));

    /// <summary>Position in model space.</summary>
    public Vector3 Position { get; } = position;

    /// <summary>Unit normal in model space.</summary>
    public Vector3 Normal { get; } = normal;

    /// <summary>Vertex color, used as the material color when lighting.</summary>
    public Color Color { get; } = color;

    VertexDeclaration IVertexType.VertexDeclaration => Declaration;
}
