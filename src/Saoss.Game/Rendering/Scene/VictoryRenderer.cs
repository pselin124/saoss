using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws the end-of-match screen (peliloppu.cpp drawVictory) from a <see cref="VictoryLayout"/>: the winners'
/// ships side by side, nose up, below red "team N won" text.
/// </summary>
public sealed class VictoryRenderer(LegacyGl gl, SceneTextures textures, ShipRenderer ships)
{
    private const float FieldOfView = 45f;
    private const float NearPlane = 1f;
    private const float FarPlane = 100f;
    private const float CameraZ = 15f;
    private const float TextZ = -25f;

    private readonly GlText _text = new(gl, textures.Font);
    private readonly GameRandom _standInRandom = new(0);
    private VictoryLayout? _layout;
    private List<Ship> _standIns = [];

    /// <summary>Draws one frame of the screen.</summary>
    public void Draw(VictoryLayout layout, SceneState state)
    {
        gl.MatrixMode(GlMatrixMode.Projection);
        gl.LoadIdentity();
        gl.Perspective(FieldOfView, (float)gl.ViewportWidth / gl.ViewportHeight, NearPlane, FarPlane);
        gl.MatrixMode(GlMatrixMode.ModelView);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.DepthTest);
        gl.Clear();
        gl.LoadIdentity();
        gl.LightPosition(0, 0, 0, 0, 1);
        gl.LookAt(0, 0, CameraZ, 0, 0, 0, 0, 1, 0);
        DrawShips(layout, state);

        // The original drew the text with whatever blending the last ship left; additive keeps the font's black clear.
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.DepthTest);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        gl.Color(1, 0, 0, 1);
        foreach (var text in layout.Texts)
        {
            _text.Println(text.X, text.Y, TextZ, text.Text);
        }

        gl.Flush();
    }

    private void DrawShips(VictoryLayout layout, SceneState state)
    {
        if (!ReferenceEquals(layout, _layout))
        {
            _layout = layout;
            _standIns = [.. layout.Ships.Select((ship, slot) =>
                StandInShips.Create(ship, ship.Definition, layout.ShipX(slot), VictoryLayout.ShipY, VictoryLayout.ShipHeading, _standInRandom))];
        }

        gl.PushMatrix();
        gl.Scale(layout.ShipScale, layout.ShipScale, layout.ShipScale);
        gl.Translate(0, 0, VictoryLayout.ShipZ);
        foreach (var ship in _standIns)
        {
            ships.DrawPreview(ship, state);
        }

        gl.PopMatrix();
    }
}
