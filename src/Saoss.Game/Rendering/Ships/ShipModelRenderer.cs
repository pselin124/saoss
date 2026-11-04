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
    private const float FlowerScaleOffset = 2f;

    /// <summary>
    /// Draws the ship at its position on the ship plane, or at height <paramref name="z"/>. Steps: disable culling,
    /// draw the team glow quad, draw the ship type's model, draw the Capture-the-Flower flower it carries (if any,
    /// in the flower team's color), re-enable culling (the original left culling on afterwards).
    /// Network nicknames are not drawn.
    /// </summary>
    public void Draw(Ship ship, SceneState state, int? carriedFlowerTeam = null, float z = GameConstants.ShipPlaneZ)
    {
        gl.Disable(GlCapability.CullFace);
        gl.PushMatrix();
        gl.Translate(ship.X, ship.Y, z);
        DrawTeamGlow(ship);
        DrawModel(new ShipDrawContext(gl, models, textures, ship, state.Detail));
        if (carriedFlowerTeam is { } team)
        {
            DrawCarriedFlower(ship, team);
        }

        gl.PopMatrix();
        gl.Enable(GlCapability.CullFace);
    }

    /// <summary>
    /// Draws only the model at the current origin, as the intro does (<c>Alus::piirra</c> with <c>debugmode</c> set):
    /// no team glow and no carried flower. <paramref name="shadow"/> selects the Stick Insect's shadow list
    /// [varjomode]; other models ignore it. Culling is disabled while drawing and enabled afterwards.
    /// </summary>
    public void DrawModelOnly(Ship ship, int detail, bool shadow = false)
    {
        gl.Disable(GlCapability.CullFace);
        gl.PushMatrix();
        DrawModel(new ShipDrawContext(gl, models, textures, ship, detail, shadow));
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

    // The flower is drawn in whatever frame the model left behind, as in the original (Alus.cpp, after the switch).
    private void DrawCarriedFlower(Ship ship, int team)
    {
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);
        gl.PushMatrix();
        SceneDrawing.TeamColor(gl, team);
        var size = (ship.Definition.Radius / 2f) + FlowerScaleOffset;
        gl.Scale(size, size, size);
        gl.Rotate(90, 1, 0, 0);
        gl.Rotate(ship.Angle, 0, 1, 0);
        gl.CallList(models.Shared.Flower4);
        gl.PopMatrix();
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
