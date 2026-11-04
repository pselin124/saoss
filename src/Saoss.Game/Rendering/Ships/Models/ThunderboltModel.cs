using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Thunderbolt (ship 7): a grey cylinder with flattened end caps, a pink speed bulb on top, a red-tipped grey
/// arrowhead nose, two fan-shaped side wings and an engine bell with a red exhaust disc [alus7, case 7].
/// </summary>
public static class ThunderboltModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_7] (the first alus7 in aluspiirto.cpp; the second one is commented out).
    /// Steps: center cylinder and its caps, speed bulb, two nose layers, wings, engine; ends with the exhaust color set.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);

        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Translate(0, -1, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.7f, 0.7f, 2, 10 + (5 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        RecordCap(gl, detail, -1);
        RecordCap(gl, detail, 1);

        // Speed bulb.
        gl.PushMatrix();
        gl.Color(0.7f, 0.5f, 0.5f);
        gl.Translate(0, 0, 0.6f);
        gl.Scale(0.35f, 2, 1);
        gl.Sphere(0.3f, 10 + (5 * detail), 4 + (4 * detail));
        gl.PopMatrix();

        // Nose, a larger grey and a smaller red four-sided point.
        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Translate(0, 1.0f, 0);
        gl.Scale(0.7f, 1.5f, 0.3f);
        gl.Sphere(1, 4, 4 + (8 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.8f, 0.3f, 0.3f);
        gl.Translate(0, 1.0f, 0);
        gl.Scale(0.525f, 1.125f, 0.3375f);
        gl.Sphere(1, 4, 4 + (8 * detail));
        gl.PopMatrix();

        // Wings.
        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Translate(0.7f, 1, 0);
        gl.Rotate(180 + 45, 0.0f, 0.0f, 1.0f);
        gl.PartialDisk(0, 2, 10, 1, 0, 45);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Translate(-0.7f, 1, 0);
        gl.Rotate(180, 0.0f, 0.0f, 1.0f);
        gl.PartialDisk(0, 2, 10, 1, 0, 45);
        gl.PopMatrix();

        // Engine.
        gl.PushMatrix();
        gl.Color(0.5f, 0.5f, 0.5f);
        gl.Translate(0, -1, 0);
        gl.Rotate(90, 1, 0, 0);
        gl.Cylinder(0.25f, 0.35f, 0.5f, 8 + (8 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        gl.Color(0.8f, 0.1f, 0.1f);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship; the exhaust disc flips over to face the other way while thrusting.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.1f * 0.5f);
        gl.CallList(c.Models.ThunderboltBody);

        gl.PushMatrix();
        gl.Translate(0, -1.5f, 0);
        gl.Rotate(c.IsThrusting ? 90 : -90, 1, 0, 0);

        // Zero loops at detail 0: GLU rejected that and drew nothing.
        gl.Disk(0, 0.35f, 8 + (8 * c.Detail), 2 * c.Detail);
        gl.PopMatrix();

        gl.PopMatrix();
        ShipGlState.Overlay(gl);
    }

    private static void RecordCap(LegacyGl gl, int detail, float y)
    {
        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Translate(0, y, 0);
        gl.Scale(1, 0.15f, 1);
        gl.Sphere(0.7f, 10 + (5 * detail), 10 + (5 * detail));
        gl.PopMatrix();
    }
}
