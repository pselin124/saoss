using Microsoft.Xna.Framework.Input;

namespace Saoss.Game.Input;

/// <summary>
/// Assigns keys to the local players' actions so that every key belongs to at most one action of one player: a key
/// already in use is swapped, the old owner getting the key the changed action had.
/// </summary>
public static class KeyBindingEditor
{
    /// <summary>
    /// Puts <paramref name="slot"/> on <paramref name="key"/>, swapping with the action that had it.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="key"/> is reserved or not a key.</exception>
    public static KeyAssignment Assign(IReadOnlyList<PlayerKeyBindings> bindings, BindingSlot slot, Keys key)
    {
        if (!PlayerKeyBindings.IsBindable(key))
        {
            throw new ArgumentException($"{key} cannot be bound.", nameof(key));
        }

        var oldKey = bindings[slot.Player].KeyFor(slot.Action);
        var owner = FindOwner(bindings, key);
        if (owner == null || owner == slot)
        {
            return new KeyAssignment(Replace(bindings, slot, key), null);
        }

        return new KeyAssignment(Replace(Replace(bindings, slot, key), owner.Value, oldKey), owner);
    }

    /// <summary>The action that uses <paramref name="key"/>, or null.</summary>
    public static BindingSlot? FindOwner(IReadOnlyList<PlayerKeyBindings> bindings, Keys key)
    {
        for (var player = 0; player < bindings.Count; player++)
        {
            foreach (var action in Enum.GetValues<PlayerAction>())
            {
                if (bindings[player].KeyFor(action) == key)
                {
                    return new BindingSlot(player, action);
                }
            }
        }

        return null;
    }

    private static PlayerKeyBindings[] Replace(IReadOnlyList<PlayerKeyBindings> bindings, BindingSlot slot, Keys key) =>
        [.. bindings.Select((keys, i) => i == slot.Player ? keys.With(slot.Action, key) : keys)];
}
