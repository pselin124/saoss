using System.Globalization;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens;

/// <summary>
/// State and keys of the controls menu, apart from drawing. Rows: the player (1-4), the player's device, one row
/// per <see cref="PlayerAction"/> with its key, "Reset All Keys" and "Previous Menu". Up and Down move with
/// wrap-around; Left, Right and Enter change the player or device; Enter on an action waits for a key (Escape
/// cancels, Escape and Enter cannot be bound) and a key used by another action is swapped with it. Escape leaves.
/// </summary>
public sealed class ControlsMenu
{
    /// <summary>Row of the player number.</summary>
    public const int PlayerRow = 0;

    /// <summary>Row of the player's device.</summary>
    public const int DeviceRow = 1;

    /// <summary>Row of the first action; the others follow in <see cref="PlayerAction"/> order.</summary>
    public const int FirstActionRow = 2;

    /// <summary>Row that resets every player's keys to the original layouts.</summary>
    public const int ResetRow = FirstActionRow + ActionCount;

    /// <summary>Row that leaves the menu; the last row.</summary>
    public const int BackRow = ResetRow + 1;

    private const int ActionCount = 6;
    private const int PlayerCount = GameSettings.MaxLocalHumans;

    private readonly PlayerControlSettings[] _controls;
    private readonly bool[] _isPadConnected = new bool[KeyboardTracker.GamePadCount];

    /// <summary>Opens the menu on the saved controls of local players 1-4.</summary>
    public ControlsMenu(IReadOnlyList<PlayerControlSettings> controls) =>
        _controls = [.. Enumerable.Range(0, PlayerCount).Select(i => controls.ElementAtOrDefault(i)?.Validated(i) ?? PlayerControlSettings.Default(i))];

    /// <summary>The highlighted row.</summary>
    public int Selection { get; private set; }

    /// <summary>The local player shown, 0-3.</summary>
    public int Player { get; private set; }

    /// <summary>The edited controls of local players 1-4.</summary>
    public IReadOnlyList<PlayerControlSettings> Controls => _controls;

    /// <summary>Whether the menu waits for the key of the selected action.</summary>
    public bool IsWaitingForKey { get; private set; }

    /// <summary>A line telling what happened or what to do; empty when there is nothing to say.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>The action on <paramref name="row"/>, or null for the other rows.</summary>
    public static PlayerAction? ActionAt(int row) =>
        row is >= FirstActionRow and < ResetRow ? (PlayerAction)(row - FirstActionRow) : null;

    /// <summary>Menu name of <paramref name="device"/>.</summary>
    public static string DeviceName(InputDevice device) => device == InputDevice.Keyboard
        ? "Keyboard"
        : "Gamepad " + (device - InputDevice.Gamepad1 + 1).ToString(CultureInfo.InvariantCulture);

    /// <summary>Whether gamepad <paramref name="index"/> (0-3) was connected on the last update.</summary>
    public bool IsPadConnected(int index) => _isPadConnected[index];

    /// <summary>The key of <paramref name="action"/> of the shown player.</summary>
    public Keys KeyOf(PlayerAction action) => KeysOf(Player).KeyFor(action);

    /// <summary>Applies this update's input; returns true when the player leaves the menu.</summary>
    public bool Update(KeyboardTracker input)
    {
        for (var i = 0; i < _isPadConnected.Length; i++)
        {
            _isPadConnected[i] = input.PadState(i).IsConnected;
        }

        if (IsWaitingForKey)
        {
            WaitForKey(input);
            return false;
        }

        if (input.WasPressed(Keys.Escape))
        {
            return true;
        }

        if (input.WasPressed(Keys.Enter))
        {
            return Choose(increase: true);
        }

        if (input.WasPressed(Keys.Right) || input.WasPressed(Keys.Left))
        {
            Change(increase: input.WasPressed(Keys.Right));
        }
        else if (input.WasPressed(Keys.Up))
        {
            Selection = Selection > 0 ? Selection - 1 : BackRow;
        }
        else if (input.WasPressed(Keys.Down))
        {
            Selection = Selection < BackRow ? Selection + 1 : 0;
        }

        return false;
    }

    private static string ActionName(PlayerAction action) => action.ToString();

    private static string PlayerName(int player) => "Player " + (player + 1).ToString(CultureInfo.InvariantCulture);

    private PlayerKeyBindings KeysOf(int player) => _controls[player].Keys ?? PlayerKeyBindings.Defaults[player];

    private bool Choose(bool increase)
    {
        if (ActionAt(Selection) is { } action)
        {
            IsWaitingForKey = true;
            Message = $"Press a key for {ActionName(action)} - Escape cancels";
            return false;
        }

        switch (Selection)
        {
            case ResetRow:
                for (var i = 0; i < PlayerCount; i++)
                {
                    _controls[i] = _controls[i] with { Keys = PlayerKeyBindings.Defaults[i] };
                }

                Message = "All keys reset to the original layouts";
                return false;
            case BackRow:
                return true;
            default:
                Change(increase);
                return false;
        }
    }

    private void Change(bool increase)
    {
        if (Selection == PlayerRow)
        {
            Player = (Player + (increase ? 1 : PlayerCount - 1)) % PlayerCount;
            Message = string.Empty;
        }
        else if (Selection == DeviceRow)
        {
            _controls[Player] = _controls[Player] with { Device = NextDevice(_controls[Player].Device, increase) };
        }
    }

    // A gamepad another player already has is skipped; the keyboard is shared by all.
    private InputDevice NextDevice(InputDevice device, bool increase)
    {
        var count = Enum.GetValues<InputDevice>().Length;
        var step = increase ? 1 : count - 1;
        do
        {
            device = (InputDevice)(((int)device + step) % count);
        }
        while (device != InputDevice.Keyboard && _controls.Where((_, i) => i != Player).Any(c => c.Device == device));

        return device;
    }

    private void WaitForKey(KeyboardTracker input)
    {
        if (input.WasPressed(Keys.Escape))
        {
            IsWaitingForKey = false;
            Message = string.Empty;
            return;
        }

        if (input.FirstPressedKey() is not { } key)
        {
            return;
        }

        if (!PlayerKeyBindings.IsBindable(key))
        {
            Message = $"{KeyNames.Of(key)} is reserved - press another key";
            return;
        }

        Assign(new BindingSlot(Player, ActionAt(Selection)!.Value), key);
        IsWaitingForKey = false;
    }

    private void Assign(BindingSlot slot, Keys key)
    {
        var result = KeyBindingEditor.Assign([.. Enumerable.Range(0, PlayerCount).Select(KeysOf)], slot, key);
        for (var i = 0; i < PlayerCount; i++)
        {
            _controls[i] = _controls[i] with { Keys = result.Bindings[i] };
        }

        Message = result.SwappedWith is { } other
            ? $"Swapped: {PlayerName(other.Player)} {ActionName(other.Action)} is now {KeyNames.Of(KeysOf(other.Player).KeyFor(other.Action))}"
            : string.Empty;
    }
}
