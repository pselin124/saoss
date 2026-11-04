namespace Saoss.Game.Screens.Network;

/// <summary>Which key chose a menu entry; the original menus acted on Enter, Left and Right alike.</summary>
public enum MenuDirection
{
    /// <summary>Enter (or Space where the original took it).</summary>
    Enter,

    /// <summary>Left arrow: decrease.</summary>
    Left,

    /// <summary>Right arrow: increase.</summary>
    Right,
}
