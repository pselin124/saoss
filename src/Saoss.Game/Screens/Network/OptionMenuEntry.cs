using Microsoft.Xna.Framework;

namespace Saoss.Game.Screens.Network;

/// <summary>A selectable line of an <see cref="OptionMenuScreen"/>, optionally with a value beside it.</summary>
/// <param name="Label">Text of the entry.</param>
/// <param name="Choose">Runs when the entry is chosen with the given key.</param>
public sealed record OptionMenuEntry(string Label, Action<MenuDirection> Choose)
{
    /// <summary>Current value drawn at <see cref="ValueX"/> on the entry's line, or null for none.</summary>
    public Func<string>? Value { get; init; }

    /// <summary>Horizontal position of the value.</summary>
    public float ValueX { get; init; }

    /// <summary>Color of the value.</summary>
    public Color ValueColor { get; init; } = Color.Red;
}
