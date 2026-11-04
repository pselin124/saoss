using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Ships;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws ships with everything attached to them (projectiles, slime, trails, beams, respawn sparks) and the
/// per-player HUD, and keeps the visual-only per-ship state and effect particles up to date.
/// </summary>
public sealed class ShipRenderer(LegacyGl gl, SceneTextures textures, SharedModels models, int detail)
{
    private const float SpawnSparkRatio = 12.5f;
    private const int TrailTimerAfterRespawn = 10;

    private readonly ShipModelRenderer _models = new(gl, textures, new ShipModels(gl, models, detail));
    private readonly ProjectileRenderer _projectiles = new(gl, textures);
    private readonly HudRenderer _hud = new(gl, textures);
    private readonly EffectSpawner _effects = new(detail);
    private readonly List<ShipVisualState> _visuals = [];

    /// <summary>
    /// Advances the visual state by one tick, after the simulation step and before the particles move.
    /// Steps: ship particles and exhaust trails of living ships (liiku, laskeKaasujaljet), respawn sparks
    /// (laskeAlkuSirpaleet), then the particles and respawn animations requested by this tick's events.
    /// </summary>
    public void Update(World world, SceneState state)
    {
        while (_visuals.Count < world.Ships.Count)
        {
            _visuals.Add(new ShipVisualState((int)(state.Particles.Capacity / SpawnSparkRatio)));
        }

        foreach (var ship in world.Ships)
        {
            if (ship.IsInGame)
            {
                UpdateShip(ship, _visuals[ship.Index], state);
            }
        }

        foreach (var gameEvent in world.Events)
        {
            _effects.Handle(gameEvent, world, state);
            switch (gameEvent)
            {
                case ShipRespawning e:
                    _visuals[e.ShipIndex].TrailTimer = TrailTimerAfterRespawn;
                    SpawnSparks.Place(world.Ships[e.ShipIndex], _visuals[e.ShipIndex], state.Random);
                    break;
                case ShipRevived e:
                    _visuals[e.ShipIndex].TrailTimer = TrailTimerAfterRespawn;
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Draws a living ship: model, primary and special projectiles, slime, exhaust trails and, for
    /// beam weapons, the beams (the per-player part of drawFrame).
    /// </summary>
    public void DrawShip(Ship ship, World world, SceneState state)
    {
        var carriedFlower = (world.Rules as CaptureTheFlowerMode)?.CarriedBy(ship.Index);
        _models.Draw(ship, state, carriedFlower?.Team);
        _projectiles.DrawPrimary(ship, state.Camera);
        _projectiles.DrawSpecial(ship);
        _projectiles.DrawSlimes(ship);
        if (ship.Index < _visuals.Count)
        {
            ExhaustTrails.Draw(gl, textures, ship, _visuals[ship.Index]);
        }

        if (HasBeams(ship.Type))
        {
            _projectiles.DrawBeams(ship);
        }
    }

    /// <summary>Draws the respawn animation of a ship that is about to reappear.</summary>
    public void DrawSpawnSparks(Ship ship, SceneState state)
    {
        if (ship.Index < _visuals.Count)
        {
            SpawnSparks.Draw(gl, textures, ship, _visuals[ship.Index]);
        }
    }

    /// <summary>Draws the ship's block of the HUD.</summary>
    public void DrawHud(Ship ship, World world, SceneState state) => _hud.Draw(ship, world);

    /// <summary>Draws just the ship model and team glow centered at the origin, for the purchase menu.</summary>
    public void DrawPreview(Ship ship, SceneState state) => _models.Draw(ship, state, z: 0f);

    // Ships whose weapons leave lines (special 5, 6, 7, primary 9, 11 or ship 18 in the original).
    private static bool HasBeams(ShipType type) =>
        type is ShipType.Trek or ShipType.Ufo or ShipType.Bomba or ShipType.PeaceCannon or ShipType.Shapperi or ShipType.Electra;

    // The original did this work before the death timer counted down, so the respawn sparks use the
    // timer value from before this tick's step.
    private void UpdateShip(Ship ship, ShipVisualState visual, SceneState state)
    {
        if (ship.IsAlive)
        {
            _effects.SpawnShipParticles(ship, state);
            if (detail < 1 && ship.IsThrusting)
            {
                ExhaustTrails.MarkThrust(ship, visual);
            }

            ExhaustTrails.Update(ship, visual);
        }

        var timer = visual.PreviousDeathTimer;
        if (timer > 0 && timer < GameConstants.SpawnTicks)
        {
            SpawnSparks.Advance(visual);
            if (timer == 1)
            {
                SpawnSparks.Burst(ship, visual, state.Particles, state.Random);
            }
        }

        visual.PreviousDeathTimer = ship.DeathTimer;
    }
}
