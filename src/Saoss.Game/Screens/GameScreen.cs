using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Audio;
using Saoss.Game.Input;
using Saoss.Game.Match;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Screens;

/// <summary>
/// A running match, local or networked: reads the local players' keys, lets the <see cref="IMatchDriver"/> move the
/// match on and draws the scene. While a local player must buy a ship, the purchase menu replaces the match view
/// (a local match pauses meanwhile; a network match goes on). The end of the match leaves the screen and reports
/// the outcome. Escape leaves a local match at once (outcome null) or calls the network game's escape handler,
/// which usually opens an <see cref="Overlay"/> menu while the match keeps running.
/// </summary>
public sealed class GameScreen : IScreen, IDisposable
{
    private readonly IMatchDriver _driver;
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
    private readonly Action? _escape;
    private readonly GameAudio? _audio;
    private int? _shownChoice;
    private int _menuTicks;
    private bool _isDisposed;

    /// <summary>
    /// Shows the match run by <paramref name="driver"/>, drawn at <paramref name="detail"/> (0-3) with
    /// <paramref name="cameraMode"/>; <paramref name="visualSeed"/> seeds the visual-only effects. The driver's local
    /// ships get the original keyboard layouts in order. <paramref name="audio"/> plays the match's sounds; null runs
    /// it silently. <paramref name="escape"/> handles Escape in a network game; without it Escape leaves the match.
    /// <paramref name="shipNames"/> are the network nicknames shown around the ships and in the HUD.
    /// </summary>
    public GameScreen(
        GraphicsDevice device,
        IMatchDriver driver,
        int detail,
        CameraMode cameraMode,
        int visualSeed,
        GameAudio? audio,
        Action<MatchOutcome?> exit,
        Action? escape = null,
        IReadOnlyList<string>? shipNames = null)
    {
        _driver = driver;
        _exit = exit;
        _escape = escape;
        _audio = audio;
        _gl = new LegacyGl(device);
        _textures = new SceneTextures(device);
        var models = new SharedModels(_gl, detail);
        _ships = new ShipRenderer(_gl, _textures, models, detail);
        _renderer = new SceneRenderer(_gl, _textures, models, _ships, detail);
        _purchaseRenderer = new ShipPurchaseRenderer(_gl, _textures, _ships);
        _victoryRenderer = new VictoryRenderer(_gl, _textures, _ships);
        _state = new SceneState(detail, cameraMode, visualSeed) { ShipNames = shipNames };
        _humans = driver.LocalShips
            .Select((index, i) => (Index: index, Keys: PlayerKeyBindings.Defaults[i % PlayerKeyBindings.Defaults.Count]))
            .ToDictionary(h => h.Index, h => (new LocalPlayerInput(h.Keys), h.Keys));
    }

    /// <summary>How the match ended, or null while it is running.</summary>
    public MatchOutcome? Outcome => World.Outcome;

    /// <summary>The match being played.</summary>
    public World World => _driver.World;

    /// <summary>
    /// A menu drawn instead of the match while the match keeps running (netgamesets.cpp netGameSets(true)); it gets
    /// the keys and the local ships fly without input. Null when the match is shown.
    /// </summary>
    public IScreen? Overlay { get; set; }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (Overlay != null)
        {
            Overlay.Update(keyboard);
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            if (_escape == null)
            {
                _audio?.StopShipLoops();
                _exit(null);
                return;
            }

            _escape();
        }

        if (_isDisposed)
        {
            return;
        }

        var chooser = Overlay == null ? _driver.LocalChoice : null;
        if (chooser is { } ship)
        {
            UpdatePurchase(keyboard, ship);
            if (World.Settings.ChoicesPauseMatch)
            {
                return;
            }
        }
        else
        {
            _shownChoice = null;
        }

        AdvanceMatch(keyboard, readKeys: Overlay == null, chooser);
        if (World.Outcome is { } outcome)
        {
            _audio?.StopShipLoops();
            _exit(outcome);
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (Overlay != null)
        {
            Overlay.Draw();
            return;
        }

        if (_shownChoice is { } ship)
        {
            _purchaseRenderer.Draw(World.Ships[ship], _purchaseMenu.Selection, _menuTicks, _state);
            return;
        }

        _renderer.Draw(World, _state);
    }

    /// <summary>Draws the end-of-match screen with this match's graphics resources.</summary>
    public void DrawVictory(VictoryLayout layout) => _victoryRenderer.Draw(layout, _state);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        (Overlay as IDisposable)?.Dispose();
        _gl.Dispose();
        _textures.Dispose();
    }

    // A local match is paused while the menu is up, like the original's blocking menu loop.
    private void UpdatePurchase(KeyboardTracker keyboard, int ship)
    {
        if (ship != _shownChoice)
        {
            _shownChoice = ship;
            _purchaseMenu.Reset();
            if (World.Settings.ChoicesPauseMatch)
            {
                _audio?.StopShipLoops();
            }
        }

        _menuTicks++;
        if (!_humans.TryGetValue(ship, out var human) || !_purchaseMenu.Update(keyboard, human.Keys))
        {
            return;
        }

        if (!_driver.ChooseShip(ship, _purchaseMenu.SelectedType))
        {
            _purchaseMenu.JumpToFallback(ShipShop.FallbackShip.Type);
            return;
        }

        _shownChoice = null;
    }

    // The buyer, and everybody on this machine while a menu covers the match, fly without input.
    private void AdvanceMatch(KeyboardTracker keyboard, bool readKeys, int? chooser)
    {
        var issued = new Dictionary<int, ShipControls>();
        foreach (var (index, human) in _humans)
        {
            issued[index] = readKeys && index != chooser ? human.Input.Read(keyboard) : default;
        }

        var ticks = _driver.Advance(issued);
        if (_driver.LatchesSpecialLocally && ticks.Count > 0)
        {
            foreach (var (index, human) in _humans)
            {
                human.Input.AfterStep(issued[index], World.Ships[index].Controls);
            }
        }

        foreach (var tick in ticks)
        {
            _audio?.PlayEvents(tick.Events);

            // Like the original, particles spawned this tick already move once at the end of it.
            _ships.Update(World, _state, tick.Events);
            _state.Update(World);
        }
    }
}
