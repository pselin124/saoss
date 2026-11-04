using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Input;
using Saoss.Game.Rendering;
using Saoss.Game.Screens;
using Saoss.Game.Settings;

namespace Saoss.Game;

/// <summary>
/// MonoGame entry point: owns the window, the fixed-rate update loop, the settings, the active screen and the
/// current local match. The match outlives visits to the local game menu so "Resume Game" can continue it.
/// </summary>
public sealed class SaossGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly KeyboardTracker _keyboard = new();
    private readonly StartupScreen _startup;
    private readonly SettingsStore _settingsStore;
    private GameSettings _settings;
    private GameAssets? _assets;
    private IScreen? _screen;
    private GameScreen? _match;
    private LocalGameMenuItem _localMenuSelection;
    private bool _applyingVideoMode;

    /// <summary>
    /// Creates the game window with the saved resolution, fullscreen mode and game speed.
    /// </summary>
    /// <param name="settingsStore">Where the settings are loaded from and saved to.</param>
    /// <param name="startup">The first screen: the main menu or a developer view.</param>
    public SaossGame(SettingsStore settingsStore, StartupScreen startup = StartupScreen.MainMenu)
    {
        _startup = startup;
        _settingsStore = settingsStore;
        _settings = settingsStore.Load();
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = _settings.Width,
            PreferredBackBufferHeight = _settings.Height,
            IsFullScreen = _settings.Fullscreen,
            PreferredDepthStencilFormat = DepthFormat.Depth24,
            SynchronizeWithVerticalRetrace = true,
            HardwareModeSwitch = false,
        };
        IsFixedTimeStep = true;
        TargetElapsedTime = TickDuration(_settings.TicksPerSecond);
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

    private void ShowMainMenu() => ShowMainMenu(returningFromSettings: false);

    private void ShowMainMenu(bool returningFromSettings) =>
        ShowScreen(new MainMenuScreen(GraphicsDevice, _assets!, Exit, OpenLocalGameMenu, () => OpenSettings(() => ShowMainMenu(returningFromSettings: true)), returningFromSettings));

    private void OpenLocalGameMenu()
    {
        _localMenuSelection = LocalGameMenuItem.StartWithRandomShips;
        ShowLocalGameMenu();
    }

    private void ShowLocalGameMenu() =>
        ShowScreen(new LocalGameMenuScreen(GraphicsDevice, _assets!, _settings.Credits, _localMenuSelection, OnLocalGameMenuChoice));

    // Both the main menu and the local game menu open the settings; leaving them returns to the opener.
    private void OpenSettings(Action back) =>
        ShowScreen(new SettingsMenuScreen(GraphicsDevice, _assets!, _settings, AvailableResolutions(), ApplySettings, settings =>
        {
            ApplySettings(settings);
            back();
        }));

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
                OpenSettings(ShowLocalGameMenu);
                break;
            case LocalGameMenuItem.ExitMenu:
                ShowMainMenu();
                break;
            default:
                StartMatch(LocalGameMenuScreen.GameKindOf(item)!.Value);
                break;
        }
    }

    private void StartMatch(LocalGameKind kind) => StartMatch(LocalPlayerSetup.CreateMatch(kind, _settings));

    // A new match replaces the previous one for good. Detail and camera are fixed for the match's lifetime.
    private void StartMatch(MatchSettings settings)
    {
        var previous = _match;
        _match = new GameScreen(GraphicsDevice, settings, _settings.Detail, _settings.CameraMode, Environment.TickCount, OnMatchLeft);
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
        if (!_applyingVideoMode)
        {
            MatchBackBufferToWindow();
        }
    }

    // Window resizes change only the back buffer; the saved size changes only through the settings menu.
    private void MatchBackBufferToWindow()
    {
        var bounds = Window.ClientBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0
            || (bounds.Width == _graphics.PreferredBackBufferWidth && bounds.Height == _graphics.PreferredBackBufferHeight))
        {
            return;
        }

        _graphics.PreferredBackBufferWidth = bounds.Width;
        _graphics.PreferredBackBufferHeight = bounds.Height;
        _graphics.ApplyChanges();
    }

    /// <summary>
    /// Applies and saves settings from the settings menu (asetusmenu "Apply"): the game speed at once, the window
    /// only when its size or fullscreen mode changed. Players, credits, camera and detail apply to the next match;
    /// music and sound settings are kept for the audio.
    /// </summary>
    private void ApplySettings(GameSettings settings)
    {
        var videoChanged = settings.Width != _settings.Width || settings.Height != _settings.Height || settings.Fullscreen != _settings.Fullscreen;
        _settings = settings;
        TargetElapsedTime = TickDuration(settings.TicksPerSecond);
        if (videoChanged)
        {
            ApplyVideoMode();
        }

        _settingsStore.Save(settings);
    }

    // Borderless fullscreen covers the desktop, so afterwards the back buffer follows the real window size.
    private void ApplyVideoMode()
    {
        _applyingVideoMode = true;
        _graphics.PreferredBackBufferWidth = _settings.Width;
        _graphics.PreferredBackBufferHeight = _settings.Height;
        _graphics.IsFullScreen = _settings.Fullscreen;
        _graphics.ApplyChanges();
        _applyingVideoMode = false;
        MatchBackBufferToWindow();
    }

    private IReadOnlyList<Resolution> AvailableResolutions()
    {
        var adapter = GraphicsDevice.Adapter;
        var desktop = adapter.CurrentDisplayMode;
        var modes = adapter.SupportedDisplayModes.Select(m => new Resolution(m.Width, m.Height));
        return ResolutionOptions.Build(modes, new Resolution(desktop.Width, desktop.Height));
    }

    private static TimeSpan TickDuration(int ticksPerSecond) => TimeSpan.FromSeconds(1.0 / ticksPerSecond);
}
