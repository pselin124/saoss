using Microsoft.Xna.Framework;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// Where an <see cref="OptionMenuScreen"/> draws its entries and arrow; the values are the original menu's
/// gl_println and glTranslatef coordinates.
/// </summary>
public sealed record OptionMenuLayout
{
    /// <summary>Position of entry n's label.</summary>
    public required Func<int, Vector3> LabelPosition { get; init; }

    /// <summary>Height of the arrow at entry n.</summary>
    public required Func<int, float> ArrowY { get; init; }

    /// <summary>Horizontal position of the arrow.</summary>
    public required float ArrowX { get; init; }

    /// <summary>Scale of the arrow.</summary>
    public Vector3 ArrowScale { get; init; } = new(0.28f, 0.3f, 0.3f);

    /// <summary>Whether text and arrow get the menus' small scene offset (drawn after glTranslatef(0.1, 0.1, 0.01)).</summary>
    public bool WithSceneOffset { get; init; }

    /// <summary>Whether the title picture is drawn behind the menu.</summary>
    public bool Background { get; init; } = true;

    /// <summary>Translucent panel behind the text, or null.</summary>
    public MenuPanel? Panel { get; init; }

    /// <summary>Text that is not an entry.</summary>
    public IReadOnlyList<MenuText> Texts { get; init; } = [];
}
