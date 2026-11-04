using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>Line width: lines wider than one pixel are drawn as screen-space quads (<see cref="GlWideLine"/>).</summary>
public sealed partial class LegacyGl
{
    private float _lineWidth = 1f;

    /// <summary>glLineWidth, in pixels of a <see cref="GlWideLine.ReferenceHeight"/>-pixel-high window.</summary>
    public void LineWidth(float width)
    {
        if (TryRecord(g => g.LineWidth(width)))
        {
            return;
        }

        _lineWidth = width;
    }

    private bool IsWideLine => _lineWidth > 1f;

    private void EndWideLines(GlPrimitive primitive)
    {
        PrepareBatch(drawAsLines: false, isLinePrimitive: true);
        var inverseProjection = Matrix.Invert(_projection);
        var step = primitive == GlPrimitive.Lines ? 2 : 1;
        for (var i = 0; i + 1 < _primitive.Count; i += step)
        {
            AddWideLine(i, i + 1, inverseProjection);
        }

        if (primitive == GlPrimitive.LineLoop && _primitive.Count > 2)
        {
            AddWideLine(_primitive.Count - 1, 0, inverseProjection);
        }
    }

    // Each segment becomes two triangles with its end vertices' colors; segments behind the camera are dropped.
    private void AddWideLine(int a, int b, Matrix inverseProjection)
    {
        var start = _primitive[a];
        var end = _primitive[b];
        var quad = GlWideLine.Expand(
            Vector4.Transform(new Vector4(start.Position, 1f), _projection),
            Vector4.Transform(new Vector4(end.Position, 1f), _projection),
            GlWideLine.ScaledWidth(_lineWidth, ViewportHeight),
            new Vector2(ViewportWidth, ViewportHeight));
        if (quad is not { } corners)
        {
            return;
        }

        var v0 = ToEye(start, corners.StartRight, inverseProjection);
        var v1 = ToEye(start, corners.StartLeft, inverseProjection);
        var v2 = ToEye(end, corners.EndLeft, inverseProjection);
        var v3 = ToEye(end, corners.EndRight, inverseProjection);
        EnsureBatchSpace(6);
        _batch[_batchCount++] = v0;
        _batch[_batchCount++] = v1;
        _batch[_batchCount++] = v2;
        _batch[_batchCount++] = v0;
        _batch[_batchCount++] = v2;
        _batch[_batchCount++] = v3;
    }

    private static VertexPositionColorTexture ToEye(VertexPositionColorTexture vertex, Vector4 clip, Matrix inverseProjection)
    {
        var eye = Vector4.Transform(clip, inverseProjection);
        return new VertexPositionColorTexture(new Vector3(eye.X, eye.Y, eye.Z) / eye.W, vertex.Color, vertex.TextureCoordinate);
    }
}
