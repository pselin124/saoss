namespace Saoss.Game.Rendering.Legacy;

/// <summary>glPolygonOffset arguments: depth offset = slope * <paramref name="Factor"/> + resolution * <paramref name="Units"/>.</summary>
/// <param name="Factor">Multiplier of the polygon's depth slope.</param>
/// <param name="Units">Multiplier of the smallest resolvable depth difference; negative moves toward the viewer.</param>
public readonly record struct GlPolygonOffset(float Factor, float Units);
