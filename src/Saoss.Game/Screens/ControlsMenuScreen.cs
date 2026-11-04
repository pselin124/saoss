using System.Globalization;
using Microsoft.Xna.Framework;
using Saoss.Game.Input;
using Saoss.Game.Settings;

namespace Saoss.Game.Screens;

/// <summary>
/// The controls menu, opened from the settings menu: the <see cref="ControlsMenu"/> drawn like the other settings
/// menus, red labels with the values beside them and a rotating arrow, plus a message line below.
/// </summary>
public sealed class ControlsMenuScreen : IScreen
{
    private const float TitleX = -8f;
    private const float TitleY = 8f;
    private const float TitleZ = -8f;
    private const float LabelX = -20f;
    private const float ValueX = -4f;
    private const float RowTopY = 14f;
    private const float RowSpacing = 2f;
    private const float MessageRow = ControlsMenu.BackRow + 1.5f;
    private const float HintRow = MessageRow + 1f;
    private const float SmallTextSize = 0.7f;
    private const float ArrowX = -6.7f;
    private const float ArrowTopY = 3.8f;
    private const float ArrowRowStep = 0.54667f;
    private const string PadHint = "Gamepad: stick or D-pad steers - A or RT fires - B or LT special";

    private static readonly Vector3 ArrowScale = new(0.3f);
    private static readonly Color UnusedKeyColor = new(0.45f, 0f, 0f);
    private static readonly Color MessageColor = new(1f, 1f, 0f);
    private static readonly Color MissingPadColor = new(0.5f, 0.5f, 0.5f);

    private readonly MenuScene _scene;
    private readonly ControlsMenu _menu;
    private readonly Action<IReadOnlyList<PlayerControlSettings>> _close;
    private float _arrowSpinDegrees;

    /// <summary>
    /// Opens the menu on <paramref name="controls"/>; <paramref name="close"/> receives the edited controls when the
    /// player leaves.
    /// </summary>
    public ControlsMenuScreen(MenuScene scene, IReadOnlyList<PlayerControlSettings> controls, Action<IReadOnlyList<PlayerControlSettings>> close)
    {
        _scene = scene;
        _menu = new ControlsMenu(controls);
        _close = close;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        if (_menu.Update(keyboard))
        {
            _close(_menu.Controls);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin(withBackground: false);
        _scene.DrawText(new Vector3(TitleX, TitleY, TitleZ), "Controls", Color.Red);
        var controls = _menu.Controls[_menu.Player];
        DrawRow(ControlsMenu.PlayerRow, "Player:", (_menu.Player + 1).ToString(CultureInfo.InvariantCulture), Color.Red);
        DrawDevice(controls.Device);
        var keyColor = controls.Device == InputDevice.Keyboard ? Color.Red : UnusedKeyColor;
        foreach (var action in Enum.GetValues<PlayerAction>())
        {
            var row = ControlsMenu.FirstActionRow + (int)action;
            var isWaiting = _menu.IsWaitingForKey && _menu.Selection == row;
            DrawRow(row, action + ":", isWaiting ? "..." : KeyNames.Of(_menu.KeyOf(action)), keyColor);
        }

        DrawRow(ControlsMenu.ResetRow, "Reset All Keys", string.Empty, Color.Red);
        DrawRow(ControlsMenu.BackRow, "Previous Menu", string.Empty, Color.Red);
        DrawSmall(MessageRow, _menu.Message, MessageColor);
        if (controls.Device != InputDevice.Keyboard)
        {
            DrawSmall(HintRow, PadHint, Color.Red);
        }

        var arrowY = ArrowTopY - (_menu.Selection * ArrowRowStep);
        _scene.DrawArrow(ArrowX, arrowY, ArrowScale, _arrowSpinDegrees, pointsLeft: false, withSceneOffset: false);
    }

    private static float RowY(float row) => RowTopY - (row * RowSpacing);

    private void DrawDevice(InputDevice device)
    {
        var name = ControlsMenu.DeviceName(device);
        var isMissing = device != InputDevice.Keyboard && !_menu.IsPadConnected(device - InputDevice.Gamepad1);
        DrawRow(ControlsMenu.DeviceRow, "Device:", isMissing ? name + " - not connected" : name, isMissing ? MissingPadColor : Color.Red);
    }

    private void DrawRow(int row, string label, string value, Color valueColor)
    {
        _scene.DrawText(new Vector3(LabelX, RowY(row), MenuScene.TextZ), label, Color.Red);
        if (value.Length > 0)
        {
            _scene.DrawText(new Vector3(ValueX, RowY(row), MenuScene.TextZ), value, valueColor);
        }
    }

    private void DrawSmall(float row, string text, Color color) =>
        _scene.DrawText(new Vector3(LabelX, RowY(row), MenuScene.TextZ), text, color, SmallTextSize);
}
