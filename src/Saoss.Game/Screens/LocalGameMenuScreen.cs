using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Core.Simulation;
using Saoss.Game.Input;

namespace Saoss.Game.Screens;

/// <summary>
/// The local game menu (localgame.cpp onePlayerGameMenu and drawOnePlayerGameMenu): new games of each kind,
/// resuming the previous match, settings and exit. Up and Down move the selection with wrap-around, Enter chooses
/// and Escape chooses "Exit Menu". The third entry shows the starting credits of a credits game.
/// </summary>
public sealed class LocalGameMenuScreen : IScreen, IDisposable
{
    private const float TextX = -25f;
    private const float TextTopY = 4f;
    private const float TextLineSpacing = 2f;
    private const float CreditsNumberX = -12f;
    private const float CreditsWordX = -5f;
    private const float ArrowX = -7.8f;
    private const float PanelLeft = -2.6f;
    private const float PanelBottom = -1.2f;
    private const float PanelRight = 1.3f;
    private const float PanelTop = 0.55f;

    private static readonly LocalGameMenuItem LastItem = Enum.GetValues<LocalGameMenuItem>().Max();

    // In menu order; the credits entry continues with the number and "Credits".
    private static readonly string[] Lines =
    [
        "Start With Random Ships",
        "Resume Game",
        "Start With",
        "Start With Credits & Random Ships",
        "Flower Domination",
        "Capture The Flower",
        "Eat The Mushroom",
        "Settings",
        "Exit Menu",
    ];

    private static readonly Vector3 ArrowScale = new(0.2f, 0.3f, 0.3f);

    private readonly MenuScene _scene;
    private readonly string _credits;
    private readonly Action<LocalGameMenuItem> _choose;
    private float _arrowSpinDegrees;

    /// <summary>
    /// Creates the menu with <paramref name="selection"/> highlighted. <paramref name="choose"/> runs when the
    /// player confirms an entry.
    /// </summary>
    public LocalGameMenuScreen(GraphicsDevice graphicsDevice, GameAssets assets, int credits, LocalGameMenuItem selection, Action<LocalGameMenuItem> choose)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _credits = credits.ToString(CultureInfo.InvariantCulture);
        _choose = choose;
        Selection = selection;
    }

    /// <summary>The highlighted entry.</summary>
    public LocalGameMenuItem Selection { get; private set; }

    /// <summary>The kind of new game an entry starts, or null for entries that do not start one.</summary>
    public static LocalGameKind? GameKindOf(LocalGameMenuItem item) => item switch
    {
        LocalGameMenuItem.StartWithRandomShips => LocalGameKind.RandomShips,
        LocalGameMenuItem.StartWithCredits => LocalGameKind.CreditsChooseShips,
        LocalGameMenuItem.StartWithCreditsAndRandomShips => LocalGameKind.CreditsRandomShips,
        LocalGameMenuItem.FlowerDomination => LocalGameKind.FlowerDomination,
        LocalGameMenuItem.CaptureTheFlower => LocalGameKind.CaptureTheFlower,
        LocalGameMenuItem.EatTheMushroom => LocalGameKind.EatTheMushroom,
        _ => null,
    };

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _arrowSpinDegrees = MenuScene.AdvanceSpin(_arrowSpinDegrees);

        if (keyboard.WasPressed(Keys.Enter))
        {
            _choose(Selection);
        }
        else if (keyboard.WasPressed(Keys.Up))
        {
            Selection = Selection > 0 ? Selection - 1 : LastItem;
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            Selection = Selection < LastItem ? Selection + 1 : 0;
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            Selection = LocalGameMenuItem.ExitMenu;
            _choose(Selection);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin();
        _scene.DrawPanel(PanelLeft, PanelBottom, PanelRight, PanelTop);
        DrawItems();
        _scene.DrawArrow(ArrowX, MenuScene.ArrowY((int)Selection), ArrowScale, _arrowSpinDegrees, pointsLeft: false);
    }

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();

    // The credits entry is three pieces of text on one line, so the number has a fixed slot.
    private void DrawItems()
    {
        for (var i = 0; i < Lines.Length; i++)
        {
            _scene.DrawText(TextX, LineY(i), Lines[i]);
        }

        var creditsY = LineY((int)LocalGameMenuItem.StartWithCredits);
        _scene.DrawText(CreditsNumberX, creditsY, _credits);
        _scene.DrawText(CreditsWordX, creditsY, "Credits");
    }

    private static float LineY(int line) => TextTopY - (line * TextLineSpacing);
}
