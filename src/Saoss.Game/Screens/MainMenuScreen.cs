using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Rendering;

namespace Saoss.Game.Screens;

/// <summary>
/// The first menu: title picture, menu items in the bitmap font and rotating 3D arrows around the selection.
/// Layout values come from the original <c>drawmenu</c>.
/// </summary>
public sealed class MainMenuScreen : IScreen, IDisposable
{
    private const int LocalGameIndex = 0;
    private const int SettingsIndex = 3;
    private const int QuitIndex = 4;
    private const float TextX = -20f;
    private const float TextTopY = 4f;
    private const float TextLineSpacing = 2f;
    private const float ArrowLeftX = -6.5f;
    private const float ArrowGapAfterText = 0.4f;
    private const float PanelLeft = -2f;
    private const float PanelBottom = -0.5f;
    private const float PanelRight = -0.1f;
    private const float PanelTop = 0.5f;

    private static readonly string[] Items = ["Local Game", "Host Multiplayer", "Join Multiplayer", "Settings", "Quit"];
    private static readonly Vector3 ArrowScale = new(0.22f, 0.3f, 0.3f);

    private readonly MenuScene _scene;
    private readonly Action _quit;
    private readonly Action _openLocalGame;
    private readonly Action _openSettings;
    private int _selection;
    private float _arrowSpinDegrees;

    /// <summary>
    /// Creates the menu, highlighting Settings when <paramref name="returningFromSettings"/> is set. The callbacks
    /// run when the player chooses Quit, Local Game or Settings.
    /// </summary>
    public MainMenuScreen(GraphicsDevice graphicsDevice, GameAssets assets, Action quit, Action openLocalGame, Action openSettings, bool returningFromSettings = false)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _quit = quit;
        _openLocalGame = openLocalGame;
        _openSettings = openSettings;
        _selection = returningFromSettings ? SettingsIndex : LocalGameIndex;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);

        if (keyboard.WasPressed(Keys.Up))
        {
            _selection = _selection > 0 ? _selection - 1 : Items.Length - 1;
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            _selection = _selection < Items.Length - 1 ? _selection + 1 : 0;
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            _selection = QuitIndex;
            _quit();
        }
        else if (keyboard.WasPressed(Keys.Enter) && _selection == QuitIndex)
        {
            _quit();
        }
        else if (keyboard.WasPressed(Keys.Enter) && _selection == LocalGameIndex)
        {
            _openLocalGame();
        }
        else if (keyboard.WasPressed(Keys.Enter) && _selection == SettingsIndex)
        {
            _openSettings();
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin();
        _scene.DrawPanel(PanelLeft, PanelBottom, PanelRight, PanelTop);
        for (var i = 0; i < Items.Length; i++)
        {
            _scene.DrawText(TextX, TextTopY - (i * TextLineSpacing), Items[i]);
        }

        var y = MenuScene.ArrowY(_selection);
        _scene.DrawArrow(ArrowLeftX, y, ArrowScale, _arrowSpinDegrees, pointsLeft: false);

        // Place the right arrow just after the text by projecting its end from text depth to arrow depth.
        var textDistance = MenuScene.CameraZ - MenuScene.TextZ;
        var textEnd = TextX + GlyphMetrics.MeasureWidth(Items[_selection]);
        var rightX = (textEnd * MenuScene.CameraZ / textDistance) + ArrowGapAfterText;
        _scene.DrawArrow(rightX, y, ArrowScale, _arrowSpinDegrees, pointsLeft: true);
    }

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();
}
