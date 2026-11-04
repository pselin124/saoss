namespace Saoss.Game.Input;

/// <summary>
/// The layouts after a key was assigned, and the action that had the key before and got the old key in exchange.
/// </summary>
/// <param name="Bindings">The layouts of local players 1-4.</param>
/// <param name="SwappedWith">The previous owner of the key, or null if the key was free.</param>
public sealed record KeyAssignment(IReadOnlyList<PlayerKeyBindings> Bindings, BindingSlot? SwappedWith);
