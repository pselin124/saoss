using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Saoss.Core;
using Saoss.Game.Input;
using Saoss.Game.Rendering;
using Saoss.Game.Screens;

namespace Saoss.Game;

/// <summary>
/// MonoGame entry point: owns the window, the fixed-rate update loop and the active screen.
/// </summary>
public sealed class SaossGame : Microsoft.Xna.Framework.Game
{
    private const int DefaultWidth = 1280;
    private const int DefaultHeight = 720;

    private readonly GraphicsDeviceManager _graphics;
    private readonly KeyboardTracker _keyboard = new();
    private GameAssets? _assets;
    private IScreen? _screen;

    /// <summary>
    /// Creates the game window with the original 100 Hz update rate.
    /// </summary>
    public SaossGame()
    {
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
        _screen = new MainMenuScreen(GraphicsDevice, _assets, Exit);
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        (_screen as IDisposable)?.Dispose();
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
