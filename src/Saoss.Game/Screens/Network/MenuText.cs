using Microsoft.Xna.Framework;

namespace Saoss.Game.Screens.Network;

/// <summary>A line of menu text that is not selectable, such as a title or the current host.</summary>
/// <param name="Position">Where gl_println put it.</param>
/// <param name="Text">The text, read every frame.</param>
/// <param name="Color">Its color.</param>
/// <param name="Size">Glyph half size.</param>
public sealed record MenuText(Vector3 Position, Func<string> Text, Color Color, float Size = 1f);
