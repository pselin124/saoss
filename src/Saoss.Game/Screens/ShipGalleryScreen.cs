using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Rendering.Ships;

namespace Saoss.Game.Screens;

/// <summary>
/// Developer view of all 24 ship models in a 6 x 4 grid, slowly turning, lit like a match.
/// Started with <c>--ship-gallery</c>. Space toggles thrust, S toggles the shield, Escape quits.
/// </summary>
public sealed class ShipGalleryScreen : IScreen, IDisposable
{
    private const int Detail = 2;
    private const int Columns = 6;
    private const float ColumnSpacing = 10f;
    private const float RowSpacing = 9f;
    private const float TurnPerTick = 0.4f;
    private const float PropellerPerTick = 10f;
    private const float CameraHeight = 48f;
    private const float CameraBack = 12f;
    private const float LightHeight = 10f;

    private readonly LegacyGl _gl;
    private readonly SceneTextures _textures;
    private readonly ShipModelRenderer _renderer;
    private readonly SceneState _state = new(Detail, CameraMode.Tilted, seed: 1);
    private readonly List<Ship> _ships = [];
    private readonly Action _exit;
    private bool _thrust;
    private bool _shield;

    /// <summary>Records the models and lines up one ship of every type.</summary>
    public ShipGalleryScreen(GraphicsDevice device, Action exit)
    {
        _exit = exit;
        _gl = new LegacyGl(device);
        _textures = new SceneTextures(device);
        var shared = new SharedModels(_gl, Detail);
        _renderer = new ShipModelRenderer(_gl, _textures, new ShipModels(_gl, shared, Detail));

        var random = new GameRandom(1);
        var types = Enum.GetValues<ShipType>();
        var rows = (types.Length + Columns - 1) / Columns;
        for (var i = 0; i < types.Length; i++)
        {
            var ship = new Ship(i, team: i % 16, isHuman: false);
            ship.Respawn(ShipCatalog.Get(types[i]), random);
            ship.X = ((i % Columns) - ((Columns - 1) / 2f)) * ColumnSpacing;
            ship.Y = (((rows - 1) / 2f) - (i / Columns)) * RowSpacing;
            ship.Angle = 90;
            ship.WeaponAngle = 90;
            _ships.Add(ship);
        }

        _gl.Enable(GlCapability.Lighting);
        _gl.Enable(GlCapability.ColorMaterial);
        _gl.LightModelLocalViewer(true);
        _gl.LightModelAmbient(0.35f, 0.35f, 0.35f, 1f);
        _gl.Enable(GlCapability.Light0);
        _gl.LightDiffuse(0, 1, 1, 1, 1);
        _gl.LightSpecular(0, 1, 1, 1, 1);
        _gl.MaterialShininess(120);
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Escape))
        {
            _exit();
            return;
        }

        _thrust ^= keyboard.WasPressed(Keys.Space);
        _shield ^= keyboard.WasPressed(Keys.S);
        foreach (var ship in _ships)
        {
            ship.Angle += TurnPerTick;
            ship.WeaponAngle = ship.Angle;
            ship.PropellerAngle += PropellerPerTick;
            ship.Controls = ship.Controls with { Thrust = _thrust };
            ship.IsShieldActive = _shield;
            ship.ShiftTail(recordCurrent: true);
        }
    }

    /// <summary>Draws the grid with the match camera setup: 45 degree perspective and light 0 above the ships.</summary>
    public void Draw()
    {
        var gl = _gl;
        gl.MatrixMode(GlMatrixMode.Projection);
        gl.LoadIdentity();
        gl.Perspective(45f, (float)gl.ViewportWidth / gl.ViewportHeight, 1f, 1024f);
        gl.MatrixMode(GlMatrixMode.ModelView);
        gl.LoadIdentity();
        gl.Clear();
        gl.LookAt(0, -CameraBack, GameConstants.ShipPlaneZ + CameraHeight, 0, 0, GameConstants.ShipPlaneZ, 0, 1, 0);
        gl.MaterialSpecular(1, 1, 1, 1);
        gl.LightDiffuse(0, 0.8f, 0.8f, 0.8f, 0.8f);
        gl.LightPosition(0, 0, 0, GameConstants.ShipPlaneZ + LightHeight, 1);

        foreach (var ship in _ships)
        {
            _renderer.Draw(ship, _state);
        }

        gl.Disable(GlCapability.CullFace);
        gl.Flush();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.Dispose();
        _textures.Dispose();
    }
}
