using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Audio;
using Saoss.Game.Diagnostics;
using Saoss.Game.Input;
using Saoss.Game.Match;
using Saoss.Game.Multiplayer;
using Saoss.Game.Rendering;
using Saoss.Game.Screens;
using Saoss.Game.Settings;

namespace Saoss.Game;

/// <summary>
/// MonoGame entry point: owns the window, the fixed-rate update loop, the settings, the active screen, the
/// current local match and network play. The local match outlives visits to the local game menu so "Resume Game"
/// can continue it.
/// </summary>
public sealed class SaossGame : Microsoft.Xna.Framework.Game, IGameShell
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly KeyboardTracker _keyboard = new();
    private readonly StartupOptions _startup;
    private readonly SettingsStore _settingsStore;
    private readonly MultiplayerLauncher _multiplayer;
    private readonly PerformanceMonitor? _monitor;
    private readonly PerformanceReadout? _readout;
    private readonly Benchmark? _benchmark;
    private GameSettings _settings;
    private GameAssets? _assets;
    private GameAudio? _audio;
    private IScreen? _screen;
    private GameScreen? _match;
    private LocalGameMenuItem _localMenuSelection;
    private bool _applyingVideoMode;

    /// <summary>
    /// Creates the game window with the saved resolution, fullscreen mode and game speed.
    /// </summary>
    /// <param name="settingsStore">Where the settings are loaded from and saved to.</param>
    /// <param name="startup">The first screen: the intro, the main menu, network play or a developer view.</param>
    public SaossGame(SettingsStore settingsStore, StartupOptions? startup = null)
    {
        _startup = startup ?? new StartupOptions();
        _settingsStore = settingsStore;
        _settings = WithStartupOverrides(settingsStore.Load(), _startup);
        _multiplayer = new MultiplayerLauncher(this);
        var isBenchmark = _startup.Benchmark != null;
        var isUncapped = isBenchmark || _startup.Capture != null;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = _settings.Width,
            PreferredBackBufferHeight = _settings.Height,
            IsFullScreen = _settings.Fullscreen,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            SynchronizeWithVerticalRetrace = !isUncapped,
            HardwareModeSwitch = false,
        };
        IsFixedTimeStep = !isUncapped;
        TargetElapsedTime = TickDuration(_settings.TicksPerSecond);
        if (isUncapped)
        {
            InactiveSleepTime = TimeSpan.Zero;
        }

        if (_startup.ShowPerformance || isBenchmark)
        {
            _monitor = new PerformanceMonitor();
            _readout = _startup.ShowPerformance ? new PerformanceReadout(title => Window.Title = title) : null;
            _benchmark = isBenchmark ? new Benchmark(_startup.Benchmark!) : null;
        }

        Window.Title = "Saoss";
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        Window.TextInput += (_, e) => _keyboard.AddTypedCharacter(e.Character);
    }

    /// <inheritdoc/>
    GameAssets IGameShell.Assets => _assets!;

    /// <inheritdoc/>
    GameSettings IGameShell.Settings => _settings;

    /// <inheritdoc/>
    GameAudio? IGameShell.Audio => _audio;

    /// <inheritdoc/>
    IReadOnlyList<Resolution> IGameShell.Resolutions => AvailableResolutions();

    /// <inheritdoc/>
    void IGameShell.Show(IScreen screen, bool disposePrevious) => ShowScreen(screen, disposePrevious);

    /// <inheritdoc/>
    void IGameShell.ApplySettings(GameSettings settings) => ApplySettings(settings);

    /// <inheritdoc/>
    void IGameShell.ShowMainMenu() => ShowMainMenu();

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        _assets = GameAssets.Load(GraphicsDevice);
        _audio = GameAudio.TryCreate(_settings);
        switch (_startup.Screen)
        {
            case StartupScreen.ShipGallery:
                ShowScreen(new ShipGalleryScreen(GraphicsDevice, Exit));
                break;
            case StartupScreen.VictoryPreview:
                StartMatch(MatchSettings.ForLocalMenu(LocalGameKind.FlowerDomination, VictoryPreview.Players(_startup.PreviewWinners)));
                VictoryPreview.EndOnNextStep(_match!.World);
                break;
            case StartupScreen.CreditsVictoryPreview:
                StartMatch(MatchSettings.ForLocalMenu(LocalGameKind.CreditsRandomShips, VictoryPreview.Players(_startup.PreviewWinners)));
                VictoryPreview.EndOnNextStep(_match!.World);
                break;
            case StartupScreen.MainMenu:
                ShowMainMenu();
                break;
            case StartupScreen.HostLobby:
                _multiplayer.Host(_startup.AutoStartPlayers);
                break;
            case StartupScreen.Join when _startup.JoinHost is { } host:
                _multiplayer.Join(host);
                break;
            case StartupScreen.BenchmarkMatch:
                StartMatch(BenchmarkMatch.Create(_settings, _startup.BenchmarkPlayers), BenchmarkMatch.Seed);
                break;
            default:
                ShowIntro();
                break;
        }
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        _multiplayer.Dispose();
        if (_screen != _match)
        {
            (_screen as IDisposable)?.Dispose();
        }

        _match?.Dispose();
        _assets?.Dispose();
        _audio?.Dispose();
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        _monitor?.BeginUpdate();
        _keyboard.Update(IsActive);
        _multiplayer.Update();
        _screen?.Update(_keyboard);
        _audio?.Update();
        base.Update(gameTime);
        _monitor?.EndUpdate();
    }

    /// <inheritdoc/>
    protected override bool BeginDraw()
    {
        _monitor?.BeginDraw();
        return base.BeginDraw();
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        _screen?.Draw();
        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void EndDraw()
    {
        if (_startup.Capture?.OnFrameDrawn(GraphicsDevice) == true)
        {
            Exit();
        }

        base.EndDraw();
        if (_monitor == null)
        {
            return;
        }

        var sample = _monitor.EndFrame();
        _readout?.AddFrame(sample);
        if (_benchmark?.AddFrame(sample, _monitor.Elapsed) == true)
        {
            Exit();
        }
    }

    // The local match is kept when another screen replaces it, so it can be resumed; network screens whose owner
    // disposes them are kept too.
    private void ShowScreen(IScreen screen, bool disposePrevious = true)
    {
        if (disposePrevious && _screen != _match && _screen != screen)
        {
            (_screen as IDisposable)?.Dispose();
        }

        _screen = screen;
    }

    private void ShowMainMenu() => ShowMainMenu(returningFromSettings: false);

    // The start track loops during the intro; leaving it moves on to the main menu and the playlist.
    private void ShowIntro()
    {
        _audio?.BeginIntroMusic();
        ShowScreen(new IntroScreen(GraphicsDevice, _settings.Detail, _settings.TicksPerSecond, () =>
        {
            _audio?.EndIntroMusic();
            ShowMainMenu();
        }, _startup.Capture != null ? FrameCapture.Seed : null));
    }

    private void ShowMainMenu(bool returningFromSettings) =>
        ShowScreen(new MainMenuScreen(
            GraphicsDevice,
            _assets!,
            Exit,
            OpenLocalGameMenu,
            () => OpenSettings(() => ShowMainMenu(returningFromSettings: true)),
            _multiplayer.ShowHostSetup,
            _multiplayer.ShowJoinSetup,
            returningFromSettings));

    private void OpenLocalGameMenu()
    {
        _localMenuSelection = LocalGameMenuItem.StartWithRandomShips;
        ShowLocalGameMenu();
    }

    private void ShowLocalGameMenu() =>
        ShowScreen(new LocalGameMenuScreen(GraphicsDevice, _assets!, _settings.Credits, _localMenuSelection, OnLocalGameMenuChoice));

    // Both the main menu and the local game menu open the settings; leaving them returns to the opener.
    private void OpenSettings(Action back) =>
        ShowScreen(new SettingsMenuScreen(
            GraphicsDevice,
            _assets!,
            _settings,
            AvailableResolutions(),
            ApplySettings,
            settings =>
            {
                ApplySettings(settings);
                back();
            },
            volume => _audio?.PreviewMusicVolume(volume)));

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
    private void StartMatch(MatchSettings settings, int? fixedSeed = null)
    {
        var previous = _match;
        var seed = fixedSeed ?? (_startup.Capture != null ? FrameCapture.Seed : Environment.TickCount);
        _match = new GameScreen(GraphicsDevice, new LocalMatchDriver(settings, seed), _settings.Detail, _settings.CameraMode, seed + 1, _audio, OnMatchLeft);
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
    /// only when its size or fullscreen mode changed, and the audio at once. Players, credits, camera and detail apply
    /// to the next match.
    /// </summary>
    private void ApplySettings(GameSettings settings)
    {
        var videoChanged = settings.Width != _settings.Width || settings.Height != _settings.Height || settings.Fullscreen != _settings.Fullscreen;
        _settings = settings;
        TargetElapsedTime = TickDuration(settings.TicksPerSecond);
        _audio?.Apply(settings);
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

    // Command-line overrides apply to this run only; a benchmark always uses a 1920x1080 window.
    private static GameSettings WithStartupOverrides(GameSettings settings, StartupOptions startup)
    {
        var result = startup.Detail is { } detail ? settings with { Detail = Math.Clamp(detail, 0, GameSettings.MaxDetail) } : settings;
        return startup.Benchmark == null
            ? result
            : result with { Width = BenchmarkOptions.Width, Height = BenchmarkOptions.Height, Fullscreen = false };
    }
}
