using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Cubix (ship 17): a light open frame of grey rods joining two translucent red balls in front and two
/// larger translucent blue balls at the back [alus17, case 17].
/// </summary>
public static class CubixModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_17]. Steps: front, back, left and right rods, front balls, back balls.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        gl.Enable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);

        var slices = 5 + (4 * detail);
        var stacks = 1 + (1 * detail);
        RecordRod(gl, -0.3f, 1, 0, 0.6f, slices, stacks);
        RecordRod(gl, -0.45f, -1, 0, 0.9f, slices, stacks);
        RecordRod(gl, -1, -1, 82.5f, 2.02f - 0.55f - 0.45f, slices, stacks);
        RecordRod(gl, 1, -1, 97.5f, 2.02f - 0.55f - 0.45f, slices, stacks);

        gl.PushMatrix();
        gl.Translate(-0.75f, 1, 0);
        gl.Color(0.8f, 0.1f, 0.1f, 0.7f);
        gl.Sphere(0.5f, 15 + (10 * detail), 8 + (8 * detail));
        gl.Translate(0.75f * 2, 0, 0);
        gl.Sphere(0.5f, 15 + (10 * detail), 8 + (8 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(-1, -1, 0);
        gl.Color(0.1f, 0.1f, 0.8f, 0.7f);
        gl.Sphere(0.6f, 15 + (10 * detail), 8 + (8 * detail));
        gl.Translate(1 * 2, 0, 0);
        gl.Sphere(0.6f, 15 + (10 * detail), 8 + (8 * detail));
        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.0f * 0.7f);
        c.Gl.CallList(c.Models.CubixBody);
        c.Gl.PopMatrix();
    }

    // A rod flipped into the XY plane; a non-zero tilt turns it into a side rod that starts 0.55 from its ball.
    private static void RecordRod(LegacyGl gl, float x, float y, float tilt, float length, int slices, int stacks)
    {
        gl.PushMatrix();
        gl.Translate(x, y, 0);
        gl.Color(0.7f, 0.7f, 0.7f, 1.0f);
        gl.Rotate(-180.0f, 1.0f, 0.0f, 1.0f);
        if (tilt != 0)
        {
            gl.Rotate(tilt, 1, 0, 0);
            gl.Translate(0, 0, 0.55f);
        }

        gl.Cylinder(0.1f, 0.1f, length, slices, stacks);
        gl.PopMatrix();
    }
}
