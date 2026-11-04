using Microsoft.Xna.Framework.Input;
using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Game.Input;

namespace Saoss.Game.Screens;

/// <summary>
/// Selection state and keys of the in-match ship purchase menu (the human part of valitseAlus).
/// Entries are the 24 ship types followed by "random ship". Thrust or Right moves to the previous entry,
/// Down or Left to the next, Fire or Enter buys. The buttons come from the buying player's keys or gamepad.
/// </summary>
public sealed class ShipPurchaseMenu
{
    /// <summary>Index of the "random ship" entry, after the last ship type.</summary>
    public const int RandomEntry = GameConstants.ShipTypeCount;

    /// <summary>Highlighted entry: a ship type index, or <see cref="RandomEntry"/>.</summary>
    public int Selection { get; private set; }

    /// <summary>The ship type to buy, or null for the random entry.</summary>
    public ShipType? SelectedType => Selection == RandomEntry ? null : (ShipType)Selection;

    /// <summary>Starts a new purchase on the first ship.</summary>
    public void Reset() => Selection = 0;

    /// <summary>The player tried to buy a ship they cannot afford: the original jumped to the cheapest ship.</summary>
    public void JumpToFallback(ShipType fallback) => Selection = (int)fallback;

    /// <summary>Applies this update's key presses; returns true when the player confirmed the purchase.</summary>
    public bool Update(KeyboardTracker keyboard, PlayerControls controls) => Update(controls.Pressed(keyboard), keyboard.WasKeyPressed(Keys.Enter));

    /// <summary>
    /// Applies the player's buttons <paramref name="pressed"/> this update; <paramref name="isEnterPressed"/> buys
    /// like Fire. Returns true when the player confirmed the purchase.
    /// </summary>
    public bool Update(PlayerButtons pressed, bool isEnterPressed)
    {
        if (isEnterPressed || pressed.Fire)
        {
            return true;
        }

        if (pressed.Thrust || pressed.Right)
        {
            Selection = Selection > 0 ? Selection - 1 : RandomEntry;
        }
        else if (pressed.Down || pressed.Left)
        {
            Selection = Selection < RandomEntry ? Selection + 1 : 0;
        }

        return false;
    }
}
