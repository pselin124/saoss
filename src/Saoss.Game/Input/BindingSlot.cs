namespace Saoss.Game.Input;

/// <summary>
/// One action of one local player.
/// </summary>
/// <param name="Player">Local player, 0-3.</param>
/// <param name="Action">The action.</param>
public readonly record struct BindingSlot(int Player, PlayerAction Action);
