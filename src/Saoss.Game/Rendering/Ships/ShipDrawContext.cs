using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Rendering.Ships;

/// <summary>
/// Everything a ship model needs to draw one ship in one frame (the arguments and globals used by Alus::piirra).
/// </summary>
/// <param name="Gl">The GL emulation.</param>
/// <param name="Models">Recorded ship display lists.</param>
/// <param name="Textures">Scene textures.</param>
/// <param name="Ship">The ship being drawn [player[numero2]].</param>
/// <param name="Detail">Detail level 0-3 [sets->detail].</param>
public readonly record struct ShipDrawContext(LegacyGl Gl, ShipModels Models, SceneTextures Textures, Ship Ship, int Detail)
{
    /// <summary>Ship collision radius, the model scale [sade].</summary>
    public float Radius => Ship.Definition.Radius;

    /// <summary>Heading in degrees [kulma].</summary>
    public float Angle => Ship.Angle;

    /// <summary>Whether the thrust key is held, which shows exhaust parts [kaasutus].</summary>
    public bool IsThrusting => Ship.Controls.Thrust;

    /// <summary>
    /// The scale-then-turn most ships start with: uniform scale, then rotate so the model's +Y points along the heading.
    /// </summary>
    public void ScaleAndTurn(float scale)
    {
        Gl.Scale(scale, scale, scale);
        Gl.Rotate(Angle - 90f, 0f, 0f, 1f);
    }

}
