using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Moth (ship 12): a thin grey spine with three pairs of flat blue wings, a pale green nose and a red
/// exhaust disc at the tail that brightens while thrusting [alus12, case 12].
/// </summary>
public static class MothModel
{
    /// <summary>
    /// Records the spine [DRAW_ALUS_12_OSA_1] and the wings [DRAW_ALUS_12_OSA_2]. The spine list leaves the
    /// tail frame pushed for the exhaust disc; the wing list pops it.
    /// </summary>
    public static (GlDisplayList Spine, GlDisplayList Wings) Record(LegacyGl gl, int detail)
    {
        var spine = gl.NewList();
        ShipGlState.Solid(gl);
        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Translate(0, -1, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.2f, 0.2f, 2, 8 + (6 * detail), 1 + (2 * detail));
        gl.EndList();

        var wings = gl.NewList();
        gl.PopMatrix();
        RecordWing(gl, -0.5f, -0.7f, 30, (0.6f, 0.3f, 0.1f), 12 + (6 * detail), 8 + (4 * detail));
        RecordWing(gl, 0.5f, -0.7f, -30, (0.6f, 0.3f, 0.1f), 12 + (6 * detail), 8 + (4 * detail));
        RecordWing(gl, -0.7f, -0.1f, 5, (0.85f, 0.3f, 0.1f), 7 + (7 * detail), 5 + (5 * detail));
        RecordWing(gl, 0.7f, -0.1f, -5, (0.85f, 0.3f, 0.1f), 7 + (7 * detail), 5 + (5 * detail));
        RecordWing(gl, -0.7f, 0.75f, -40, (0.85f, 0.3f, 0.1f), 7 + (7 * detail), 5 + (5 * detail));
        RecordWing(gl, 0.7f, 0.75f, 40, (0.85f, 0.3f, 0.1f), 7 + (7 * detail), 5 + (5 * detail));

        // Nose.
        gl.PushMatrix();
        gl.Color(0.5f, 0.7f, 0.5f);
        gl.Translate(0, 1, 0);
        gl.Scale(0.2f, 0.15f, 0.2f);
        gl.Sphere(1, 10 + (5 * detail), 5 + (4 * detail));
        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return (spine, wings);
    }

    /// <summary>Draws the ship with a bright red exhaust disc while thrusting and a dark one otherwise.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        gl.Scale(c.Radius / 1.1f * 0.65f, c.Radius / 1.1f * 0.65f, c.Radius / 1.1f * 0.65f);
        gl.Disable(GlCapability.Texture2D);
        gl.Rotate(c.Angle - 90f, 0, 0, 1);
        gl.CallList(c.Models.MothSpine);
        if (c.IsThrusting)
        {
            gl.Color(0.9f, 0.1f, 0.1f);
        }
        else
        {
            gl.Color(0.5f, 0.1f, 0.1f);
        }

        gl.Disk(0, 0.2f, 8 + (6 * c.Detail), 1 + (2 * c.Detail));
        gl.CallList(c.Models.MothWings);
        gl.PopMatrix();
    }

    private static void RecordWing(LegacyGl gl, float x, float y, float degrees, (float X, float Y, float Z) scale, int slices, int stacks)
    {
        gl.PushMatrix();
        gl.Color(0.1f, 0.2f, 0.8f);
        gl.Translate(x, y, 0);
        gl.Rotate(degrees, 0, 0, 1);
        gl.Scale(scale.X, scale.Y, scale.Z);
        gl.Sphere(1, slices, stacks);
        gl.PopMatrix();
    }
}
