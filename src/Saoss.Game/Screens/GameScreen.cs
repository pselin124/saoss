using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Core.Simulation;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Screens;

/// <summary>
/// A running local match: reads the local players' keys, steps the simulation and draws the scene.
/// Escape returns to the main menu.
/// </summary>
public sealed class GameScreen : IScreen, IDisposable
{
    private const int Detail = 2;

    private readonly World _world;
    private readonly LegacyGl _gl;
    private readonly SceneTextures _textures;
    private readonly SceneState _state;
    private readonly SceneRenderer _renderer;
    private readonly ShipRenderer _ships;
    private readonly List<LocalPlayerInput> _inputs;
    private readonly Action _exit;

    /// <summary>Starts a match. Players marked human are mapped to the original keyboard layouts in order.</summary>
    public GameScreen(GraphicsDevice device, MatchSettings settings, int seed, Action exit)
    {
        _exit = exit;
        _world = new World(settings, seed);
        _gl = new LegacyGl(device);
        _textures = new SceneTextures(device);
        var models = new SharedModels(_gl, Detail);
        _ships = new ShipRenderer(_gl, _textures, models, Detail);
        _renderer = new SceneRenderer(_gl, _textures, models, _ships, Detail);
        _state = new SceneState(Detail, seed + 1);
        _inputs = [.. settings.Players.Where(p => p.IsHuman).Select((_, i) => new LocalPlayerInput(PlayerKeyBindings.Defaults[i % PlayerKeyBindings.Defaults.Count]))];
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Escape))
        {
            _exit();
            return;
        }

        var humans = _world.Ships.Where(s => s.IsHuman).ToList();
        var issued = new ShipControls[humans.Count];
        for (var i = 0; i < humans.Count && i < _inputs.Count; i++)
        {
            issued[i] = _inputs[i].Read(keyboard);
            humans[i].Controls = issued[i];
        }

        _world.Step();

        for (var i = 0; i < humans.Count && i < _inputs.Count; i++)
        {
            _inputs[i].AfterStep(issued[i], humans[i].Controls);
        }

        // Like the original, particles spawned this tick already move once at the end of it.
        _ships.Update(_world, _state);
        _state.Update(_world);
    }

    /// <inheritdoc/>
    public void Draw() => _renderer.Draw(_world, _state);

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.Dispose();
        _textures.Dispose();
    }
}
