namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// One recorded call of a <see cref="GlDisplayList"/>. Vertex attributes keep their arguments inline so replaying
/// them needs no delegate; every other call is a delegate.
/// </summary>
/// <param name="Op">The kind of call.</param>
/// <param name="X">First argument of a vertex attribute.</param>
/// <param name="Y">Second argument of a vertex attribute.</param>
/// <param name="Z">Third argument of a vertex attribute.</param>
/// <param name="W">Fourth argument of a vertex attribute.</param>
/// <param name="Call">The call for <see cref="GlListOp.Call"/>.</param>
internal readonly record struct GlListCommand(GlListOp Op, float X, float Y, float Z, float W, Action<LegacyGl>? Call = null);
