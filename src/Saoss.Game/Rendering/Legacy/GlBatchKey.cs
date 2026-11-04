using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Everything that decides how a batch of vertices is drawn. Vertices are batched while this stays the same;
/// any change flushes the batch, so every emulated GL state that affects drawing must be part of it.
/// </summary>
/// <param name="Texture">Texture to sample (already resolved for the texture environment), or null.</param>
/// <param name="Blend">Blend factors, or null when blending is disabled.</param>
/// <param name="DepthTest">Whether depth is tested and written.</param>
/// <param name="Cull">Which winding is culled.</param>
/// <param name="Stencil">Stencil test settings, or null when the stencil test is disabled.</param>
/// <param name="PolygonOffset">Depth offset for filled polygons, or null when disabled (always null for lines).</param>
/// <param name="Projection">Projection matrix.</param>
/// <param name="IsLine">Whether the batch holds lines instead of triangles.</param>
public readonly record struct GlBatchKey(
    Texture2D? Texture,
    (GlBlend Source, GlBlend Destination)? Blend,
    bool DepthTest,
    CullMode Cull,
    GlStencil? Stencil,
    GlPolygonOffset? PolygonOffset,
    Matrix Projection,
    bool IsLine);
