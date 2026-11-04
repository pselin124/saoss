using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Input;
using Saoss.Game.Rendering;
using Saoss.Game.Screens;

namespace Saoss.Game;

/// <summary>
/// MonoGame entry point: owns the window, the fixed-rate update loop, the active screen and the current local
/// match. The match outlives visits to the local game menu so "Resume Game" can continue it.
/// </summary>
public sealed class SaossGame : Microsoft.Xna.Framework.Game
{
    private const int DefaultWidth = 1280;
    private const int DefaultHeight = 720;

    private readonly GraphicsDeviceManager _graphics;
    private readonly KeyboardTracker _keyboard = new();
    private readonly StartupScreen _startup;
    private GameAssets? _assets;
    private IScreen? _screen;
    private GameScreen? _match;
    private LocalGameMenuItem _localMenuSelection;

    /// <summary>
    /// Creates the game window with the original 100 Hz update rate.
    /// </summary>
    /// <param name="startup">The first screen: the main menu or a developer view.</param>
    public SaossGame(StartupScreen startup = StartupScreen.MainMenu)
    {
        _startup = startup;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = DefaultWidth,
            PreferredBackBufferHeight = DefaultHeight,
            PreferredDepthStencilFormat = DepthFormat.Depth24,
            SynchronizeWithVerticalRetrace = true,
            HardwareModeSwitch = false,
        };
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / GameConstants.TicksPerSecond);
        Window.Title = "Saoss";
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        _assets = GameAssets.Load(GraphicsDevice);
        switch (_startup)
        {
            case StartupScreen.ShipGallery:
                ShowScreen(new ShipGalleryScreen(GraphicsDevice, Exit));
                break;
            case StartupScreen.VictoryPreview:
                StartMatch(MatchSettings.ForLocalMenu(LocalGameKind.FlowerDomination, VictoryPreview.Players));
                VictoryPreview.EndOnNextStep(_match!.World);
                break;
            case StartupScreen.CreditsVictoryPreview:
                StartMatch(MatchSettings.ForLocalMenu(LocalGameKind.CreditsRandomShips, VictoryPreview.Players));
                VictoryPreview.EndOnNextStep(_match!.World);
                break;
            default:
                ShowMainMenu();
                break;
        }
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        if (_screen != _match)
        {
            (_screen as IDisposable)?.Dispose();
        }

        _match?.Dispose();
        _assets?.Dispose();
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        _keyboard.Update(IsActive);
        _screen?.Update(_keyboard);
        base.Update(gameTime);
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        _screen?.Draw();
        base.Draw(gameTime);
    }

    // The match is kept when another screen replaces it, so it can be resumed.
    private void ShowScreen(IScreen screen)
    {
        if (_screen != _match)
        {
            (_screen as IDisposable)?.Dispose();
        }

        _screen = screen;
    }

    private void ShowMainMenu() => ShowScreen(new MainMenuScreen(GraphicsDevice, _assets!, Exit, OpenLocalGameMenu));

    private void OpenLocalGameMenu()
    {
        _localMenuSelection = LocalGameMenuItem.StartWithRandomShips;
        ShowLocalGameMenu();
    }

    private void ShowLocalGameMenu() =>
        ShowScreen(new LocalGameMenuScreen(GraphicsDevice, _assets!, MatchSettings.DefaultCredits, _localMenuSelection, OnLocalGameMenuChoice));

    // The menu remembers its selection between matches, like the original's menu loop.
    private void OnLocalGameMenuChoice(LocalGameMenuItem item)
    {
        _localMenuSelection = item;
        switch (item)
        {
            case LocalGameMenuItem.ResumeGame when _match != null:
                ShowScreen(_match);
                break;
            case LocalGameMenuItem.ResumeGame:
                StartMatch(LocalGameKind.RandomShips);
                break;
            case LocalGameMenuItem.Settings:
                // The settings menu is ported in phase 5; until then this entry leaves the menu as it is.
                break;
            case LocalGameMenuItem.ExitMenu:
                ShowMainMenu();
                break;
            default:
                StartMatch(LocalGameMenuScreen.GameKindOf(item)!.Value);
                break;
        }
    }

    private void StartMatch(LocalGameKind kind) => StartMatch(MatchSettings.ForLocalMenu(kind, LocalPlayerSetup.Default));

    // A new match replaces the previous one for good.
    private void StartMatch(MatchSettings settings)
    {
        var previous = _match;
        _match = new GameScreen(GraphicsDevice, settings, Environment.TickCount, OnMatchLeft);
        ShowScreen(_match);
        previous?.Dispose();
    }

    // Escape (no outcome) goes straight back to the menu; a finished match shows who won first.
    private void OnMatchLeft(MatchOutcome? outcome)
    {
        if (outcome == null)
        {
            ShowLocalGameMenu();
            return;
        }

        ShowScreen(new VictoryScreen(_match!, outcome, ShowLocalGameMenu));
    }

    private void OnClientSizeChanged(object? sender, EventArgs e)
    {
        var bounds = Window.ClientBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        _graphics.PreferredBackBufferWidth = bounds.Width;
        _graphics.PreferredBackBufferHeight = bounds.Height;
        _graphics.ApplyChanges();
    }
}
