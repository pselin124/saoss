using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Popper (ship 23): a pale green egg body on two grey side tubes, with a gun ball and barrel in front and
/// two red engine discs at the back that flip over while thrusting [alus23, case 23].
/// </summary>
public static class PopperModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_23]. It starts with the two engine discs in the frame the draw code
    /// pushed and pops that frame before the body tubes, egg and gun.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);
        gl.Color(0.8f, 0.1f, 0.1f);
        gl.Disk(0, 0.35f, 8 + (8 * detail), 1 + (1 * detail));
        gl.Translate(-1.2f, 0, 0);
        gl.Disk(0, 0.35f, 8 + (8 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.7f, 0.7f, 0.7f);
        gl.Translate(-0.6f, -1.5f, -0.35f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.35f, 0.35f, 3, 8 + (8 * detail), 1 + (1 * detail));
        gl.Translate(1.2f, 0, 0);
        gl.Cylinder(0.35f, 0.35f, 3, 8 + (8 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.7f, 0.85f, 0.7f);
        gl.Scale(0.9f, 1.2f, 0.75f);
        gl.Sphere(1, 18 + (8 * detail), 10 + (8 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.65f, 0.73f, 0.65f);
        gl.Translate(0, 1, 0.3f);
        gl.Sphere(0.3f, 18 + (8 * detail), 10 + (8 * detail));
        gl.Rotate(-90, 1, 0, 0);
        gl.Color(0.65f, 0.68f, 0.65f);
        gl.Cylinder(0.2f, 0.15f, 0.6f, 7 + (7 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship, pushing the engine disc frame that the list pops.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        gl.Rotate(c.Angle - 90.0f, 0.0f, 0.0f, 1.0f);
        gl.Scale(c.Radius * 0.5f, c.Radius * 0.5f, c.Radius * 0.5f);

        gl.PushMatrix();
        gl.Translate(0.6f, -1.5f, -0.35f);
        gl.Rotate(-90, 1, 0, 0);
        if (c.IsThrusting)
        {
            gl.Rotate(-180, 1, 0, 0);
        }

        gl.CallList(c.Models.PopperBody);
        gl.PopMatrix();
    }
}
