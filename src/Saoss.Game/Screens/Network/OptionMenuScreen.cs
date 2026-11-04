using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The original's simple list menu (montakoHostia, joinMenu, hostSettingsMenu, nikinVaihtoMenu, netGameSets): red
/// entries, values beside some of them and one rotating arrow. Up and Down move with wrap-around; Enter, Left and
/// Right choose the entry; Escape selects the last entry and chooses it. An entry can open a
/// <see cref="SubScreen"/> that replaces the menu until it closes.
/// </summary>
public sealed class OptionMenuScreen : IScreen, IDisposable
{
    private readonly MenuScene _scene;
    private readonly OptionMenuLayout _layout;
    private readonly IReadOnlyList<OptionMenuEntry> _entries;
    private float _arrowSpinDegrees;

    /// <summary>Creates the menu with entry <paramref name="selection"/> highlighted.</summary>
    public OptionMenuScreen(GraphicsDevice graphicsDevice, GameAssets assets, OptionMenuLayout layout, IReadOnlyList<OptionMenuEntry> entries, int selection = 0)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _layout = layout;
        _entries = entries;
        Selection = selection;
    }

    /// <summary>The highlighted entry.</summary>
    public int Selection { get; private set; }

    /// <summary>A screen shown instead of the menu, such as a submenu or text entry.</summary>
    public SubScreen Child { get; } = new();

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (Child.Update(keyboard))
        {
            return;
        }

        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);
        var last = _entries.Count - 1;
        if (keyboard.WasPressed(Keys.Enter))
        {
            _entries[Selection].Choose(MenuDirection.Enter);
        }
        else if (keyboard.WasPressed(Keys.Left))
        {
            _entries[Selection].Choose(MenuDirection.Left);
        }
        else if (keyboard.WasPressed(Keys.Right))
        {
            _entries[Selection].Choose(MenuDirection.Right);
        }
        else if (keyboard.WasPressed(Keys.Up))
        {
            Selection = Selection > 0 ? Selection - 1 : last;
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            Selection = Selection < last ? Selection + 1 : 0;
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            Selection = last;
            _entries[last].Choose(MenuDirection.Enter);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (Child.Draw())
        {
            return;
        }

        _scene.Begin(_layout.Background);
        if (_layout.Panel is { } panel)
        {
            _scene.DrawPanel(panel.Left, panel.Bottom, panel.Right, panel.Top);
        }

        var offset = _layout.WithSceneOffset ? MenuScene.SceneOffset : Vector3.Zero;
        foreach (var text in _layout.Texts)
        {
            _scene.DrawText(text.Position + offset, text.Text(), text.Color, text.Size);
        }

        for (var i = 0; i < _entries.Count; i++)
        {
            var position = _layout.LabelPosition(i) + offset;
            _scene.DrawText(position, _entries[i].Label, Color.Red);
            if (_entries[i].Value is { } value)
            {
                _scene.DrawText(position with { X = _entries[i].ValueX + offset.X }, value(), _entries[i].ValueColor);
            }
        }

        _scene.DrawArrow(_layout.ArrowX, _layout.ArrowY(Selection), _layout.ArrowScale, _arrowSpinDegrees, pointsLeft: false, _layout.WithSceneOffset);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Child.Dispose();
        _scene.Dispose();
    }
}
