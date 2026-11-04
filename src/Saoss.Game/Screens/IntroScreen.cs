using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Intro;

namespace Saoss.Game.Screens;

/// <summary>
/// The intro shown at program start (<c>AlotaLippu</c> in lippu.cpp): the flag scene runs until Enter or Escape is
/// pressed, then <c>finished</c> is called. The original advanced the scene 150 frames per second whatever the game
/// speed, so each tick advances as many intro frames as fall into it.
/// </summary>
public sealed class IntroScreen : IScreen, IDisposable
{
    private const int FramesPerSecond = 150;

    private readonly IntroScene _scene;
    private readonly ClothFlag _flag = new();
    private readonly int _ticksPerSecond;
    private readonly Action _finished;
    private int _frame;
    private int _pendingFrameTicks;

    /// <summary>
    /// Creates the intro at <paramref name="detail"/> (0-3) for an update rate of <paramref name="ticksPerSecond"/>;
    /// <paramref name="finished"/> runs when the player leaves it. <paramref name="seed"/> fixes the ships' random
    /// flight for reproducible frames; null varies it.
    /// </summary>
    public IntroScreen(GraphicsDevice device, int detail, int ticksPerSecond, Action finished, int? seed = null)
    {
        _scene = new IntroScene(device, detail, seed is { } value ? new Random(value) : new Random());
        _ticksPerSecond = ticksPerSecond;
        _finished = finished;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Enter) || keyboard.WasPressed(Keys.Escape))
        {
            _finished();
            return;
        }

        _scene.SetThrust(keyboard.IsDown(Keys.Up));
        _pendingFrameTicks += FramesPerSecond;
        while (_pendingFrameTicks >= _ticksPerSecond)
        {
            _pendingFrameTicks -= _ticksPerSecond;
            _frame++;
            _flag.Step(_frame);
        }
    }

    /// <inheritdoc/>
    public void Draw() => _scene.Draw(_frame, _flag);

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();
}
