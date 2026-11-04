using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Screens;

/// <summary>
/// A running local match: reads the local players' keys, steps the simulation and draws the scene.
/// While a human player must buy a ship, the purchase menu replaces the match. Escape, or the end of the match,
/// leaves the screen and reports the outcome (null when the match was abandoned).
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
    private readonly ShipPurchaseRenderer _purchaseRenderer;
    private readonly VictoryRenderer _victoryRenderer;
    private readonly ShipPurchaseMenu _purchaseMenu = new();
    private readonly Dictionary<int, (LocalPlayerInput Input, PlayerKeyBindings Keys)> _humans;
    private readonly Action<MatchOutcome?> _exit;
    private ShipChoice? _shownChoice;
    private int _menuTicks;

    /// <summary>Starts a match. Players marked human are mapped to the original keyboard layouts in order.</summary>
    public GameScreen(GraphicsDevice device, MatchSettings settings, int seed, Action<MatchOutcome?> exit)
    {
        _exit = exit;
        _world = new World(settings, seed);
        _gl = new LegacyGl(device);
        _textures = new SceneTextures(device);
        var models = new SharedModels(_gl, Detail);
        _ships = new ShipRenderer(_gl, _textures, models, Detail);
        _renderer = new SceneRenderer(_gl, _textures, models, _ships, Detail);
        _purchaseRenderer = new ShipPurchaseRenderer(_gl, _textures, _ships);
        _victoryRenderer = new VictoryRenderer(_gl, _textures, _ships);
        _state = new SceneState(Detail, seed + 1);
        _humans = _world.Ships
            .Where(s => s.IsHuman)
            .Select((ship, i) => (ship.Index, Keys: PlayerKeyBindings.Defaults[i % PlayerKeyBindings.Defaults.Count]))
            .ToDictionary(h => h.Index, h => (new LocalPlayerInput(h.Keys), h.Keys));
    }

    /// <summary>How the match ended, or null while it is running.</summary>
    public MatchOutcome? Outcome => _world.Outcome;

    /// <summary>The match being played.</summary>
    public World World => _world;

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Escape))
        {
            _exit(null);
            return;
        }

        if (_world.PendingChoice is { } choice)
        {
            UpdatePurchase(keyboard, choice);
            return;
        }

        StepMatch(keyboard);
        if (_world.Outcome is { } outcome)
        {
            _exit(outcome);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (_world.PendingChoice is { } choice)
        {
            _purchaseRenderer.Draw(_world.Ships[choice.ShipIndex], _purchaseMenu.Selection, _menuTicks, _state);
            return;
        }

        _renderer.Draw(_world, _state);
    }

    /// <summary>Draws the end-of-match screen with this match's graphics resources.</summary>
    public void DrawVictory(VictoryLayout layout) => _victoryRenderer.Draw(layout, _state);

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.Dispose();
        _textures.Dispose();
    }

    // The match is paused while the menu is up, like the original's blocking menu loop.
    private void UpdatePurchase(KeyboardTracker keyboard, ShipChoice choice)
    {
        if (!ReferenceEquals(choice, _shownChoice))
        {
            _shownChoice = choice;
            _purchaseMenu.Reset();
        }

        _menuTicks++;
        if (!_humans.TryGetValue(choice.ShipIndex, out var human) || !_purchaseMenu.Update(keyboard, human.Keys))
        {
            return;
        }

        if (!_world.ChooseShip(choice.ShipIndex, _purchaseMenu.SelectedType))
        {
            _purchaseMenu.JumpToFallback(ShipShop.FallbackShip.Type);
        }
    }

    private void StepMatch(KeyboardTracker keyboard)
    {
        var issued = new Dictionary<int, ShipControls>();
        foreach (var (index, human) in _humans)
        {
            issued[index] = human.Input.Read(keyboard);
            _world.Ships[index].Controls = issued[index];
        }

        _world.Step();

        foreach (var (index, human) in _humans)
        {
            human.Input.AfterStep(issued[index], _world.Ships[index].Controls);
        }

        // Like the original, particles spawned this tick already move once at the end of it.
        _ships.Update(_world, _state);
        _state.Update(_world);
    }
}
