using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>A lit vertex waiting for its color: eye-space position and normal, material color and lighting state.</summary>
/// <param name="Position">Eye-space position.</param>
/// <param name="Normal">Eye-space normal.</param>
/// <param name="Material">Current color, clamped.</param>
/// <param name="Lighting">Index of the lighting state copy the vertex was emitted under.</param>
internal readonly record struct GlShadeJob(Vector3 Position, Vector3 Normal, Vector4 Material, int Lighting);
