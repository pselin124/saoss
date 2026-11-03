using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Rendering.Ships.Models;

namespace Saoss.Game.Rendering.Ships;

/// <summary>
/// Draws a ship's 3D model with its team glow ring, a port of <c>Alus::piirra</c> as called from
/// <c>Player::piirra</c> during a local match.
/// </summary>
public sealed class ShipModelRenderer(LegacyGl gl, SceneTextures textures, ShipModels models)
{
    private const float GlowScale = 3.0f;
    private const float GlowDepth = -0.2f;

    /// <summary>
    /// Draws the ship at its position on the ship plane. Steps: disable culling, draw the team glow
    /// quad, draw the ship type's model, re-enable culling (the original left culling on afterwards).
    /// Network nicknames and the carried Capture-the-Flower flower are not drawn yet.
    /// </summary>
    public void Draw(Ship ship, SceneState state)
    {
        gl.Disable(GlCapability.CullFace);
        gl.PushMatrix();
        gl.Translate(ship.X, ship.Y, GameConstants.ShipPlaneZ);
        DrawTeamGlow(ship);
        DrawModel(new ShipDrawContext(gl, models, textures, ship, state.Detail));
        gl.PopMatrix();
        gl.Enable(GlCapability.CullFace);
    }

    private static void DrawModel(ShipDrawContext c)
    {
        Action<ShipDrawContext> draw = c.Ship.Type switch
        {
            ShipType.Magneto => MagnetoModel.Draw,
            ShipType.Tringu => TringuModel.Draw,
            ShipType.Wasp => WaspModel.Draw,
            ShipType.Subni => SubniModel.Draw,
            ShipType.Rollu => RolluModel.Draw,
            ShipType.Trek => TrekModel.Draw,
            ShipType.Ufo => UfoModel.Draw,
            ShipType.Bomba => BombaModel.Draw,
            ShipType.Rayska => RayskaModel.Draw,
            ShipType.PeaceCannon => PeaceCannonModel.Draw,
            ShipType.Bloddu => BlodduModel.Draw,
            ShipType.Shapperi => ShapperiModel.Draw,
            ShipType.Moth => MothModel.Draw,
            ShipType.PowerVacuum => PowerVacuumModel.Draw,
            ShipType.Bulldog => BulldogModel.Draw,
            ShipType.Wiiii => WiiiiModel.Draw,
            ShipType.Elephant => ElephantModel.Draw,
            ShipType.Cubix => CubixModel.Draw,
            ShipType.Electra => ElectraModel.Draw,
            ShipType.Tongu => TonguModel.Draw,
            ShipType.SpaceTurtle => SpaceTurtleModel.Draw,
            ShipType.StickInsect => StickInsectModel.Draw,
            ShipType.StarWorm => StarWormModel.Draw,
            ShipType.Popper => PopperModel.Draw,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c.Ship.Type, "Unknown ship type."),
        };
        draw(c);
    }

    // Local games draw a hollow glow in the team color under the ship [tiimitexture].
    private void DrawTeamGlow(Ship ship)
    {
        var size = GlowScale * ship.Definition.Radius;
        gl.Disable(GlCapability.Lighting);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.DepthTest);
        SceneDrawing.TeamColor(gl, ship.Team);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcColor);
        gl.BindTexture(textures.TeamGlow);
        SceneDrawing.Quad(gl, size, GlowDepth);
    }
}
