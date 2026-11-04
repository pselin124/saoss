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
        if (TryRecord(width, static (g, w) => g.LineWidth(w)))
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
        var startJob = _primitiveJobs[a];
        var endJob = _primitiveJobs[b];
        EnsureBatchSpace(4, 6);
        var first = _batchCount;
        AddBatchVertex(v0, startJob);
        AddBatchVertex(v1, startJob);
        AddBatchVertex(v2, endJob);
        AddBatchVertex(v3, endJob);
        ReadOnlySpan<int> quadIndices = [0, 1, 2, 0, 2, 3];
        foreach (var index in quadIndices)
        {
            _indices[_indexCount++] = first + index;
        }
    }

    private void AddBatchVertex(VertexPositionColorTexture vertex, int job)
    {
        _batch[_batchCount] = vertex;
        _batchJobs[_batchCount++] = job;
    }

    private static VertexPositionColorTexture ToEye(VertexPositionColorTexture vertex, Vector4 clip, Matrix inverseProjection)
    {
        var eye = Vector4.Transform(clip, inverseProjection);
        return new VertexPositionColorTexture(new Vector3(eye.X, eye.Y, eye.Z) / eye.W, vertex.Color, vertex.TextureCoordinate);
    }
}
