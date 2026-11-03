using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Trek (ship 5): a flattened sand-colored saucer-cylinder hull with a flared nose funnel, blue speed
/// stripes, an engine nozzle and two angled nacelles on struts at the back [alus5, case 5].
/// </summary>
public static class TrekModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_5]. Steps: hull, engine nozzle, speed stripes, front funnel,
    /// then both nacelles.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);

        // Hull.
        gl.PushMatrix();
        gl.Color(0.65f, 0.6f, 0.4f);
        gl.Translate(0, 1, 0);
        gl.Rotate(90, 1, 0, 0);
        gl.Scale(1, 0.5f, 1);
        gl.Cylinder(0.5f, 0.5f, 2, 15 + (5 * detail), 3 + (2 * detail));
        gl.Sphere(0.5f, 15 + (detail * 5), 8 + (8 * detail));
        gl.Translate(0, 0, 2);
        gl.Scale(1, 1, 0.25f);
        gl.Sphere(0.5f, 15 + (detail * 5), 8 + (8 * detail));
        gl.PopMatrix();

        // Engine nozzle.
        gl.PushMatrix();
        gl.Color(0.65f, 0.6f, 0.6f);
        gl.Translate(0, -1, 0);
        gl.Rotate(90, 1, 0, 0);
        gl.Scale(1, 0.75f, 1);
        gl.Cylinder(0.25f, 0.27f, 0.45f, 7 + (5 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        // Speed stripes.
        gl.PushMatrix();
        gl.Color(0.1f, 0.05f, 0.8f);
        gl.Translate(0, 0.4f, 0.2f);
        gl.Scale(0.2f, 1, 0.5f);
        gl.Sphere(0.3f, 6 + (detail * 8), 5 + (6 * detail));
        gl.Translate(0, -0.8f, 0);
        gl.Sphere(0.3f, 6 + (detail * 8), 5 + (6 * detail));
        gl.PopMatrix();

        // Front funnel.
        gl.PushMatrix();
        gl.Color(0.65f, 0.6f, 0.4f);
        gl.Translate(0, 0, -0.3f);
        gl.Scale(0.5f, 0.3f, 1);
        gl.Sphere(0.55f, 6 + (detail * 8), 5 + (6 * detail));
        gl.PopMatrix();
        gl.PushMatrix();
        gl.Color(0.65f, 0.6f, 0.6f);
        gl.Translate(0, 0, -0.6f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Scale(1, 0.75f, 1);
        gl.Cylinder(0.2f, 0.5f, 1.2f, 7 + (5 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        RecordNacelle(gl, detail, 1);
        RecordNacelle(gl, detail, -1);

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.4f);
        c.Gl.CallList(c.Models.TrekBody);
        c.Gl.PopMatrix();
    }

    // One strut and capped nacelle; side is +1 for the right, -1 for the left.
    private static void RecordNacelle(LegacyGl gl, int detail, float side)
    {
        gl.PushMatrix();
        gl.Color(0.65f, 0.6f, 0.4f);
        gl.Translate(0.3f * side, 0, -0.1f);
        gl.Rotate(110 * side, 0, 1, 0);
        gl.Rotate(20, 1, 0, 0);
        gl.Scale(0.3f, 0.6f, 1);
        gl.Cylinder(0.2f, 0.5f, 0.7f, 7 + (5 * detail), 1 + (2 * detail));
        gl.PopMatrix();
        gl.PushMatrix();
        gl.Color(0.60f, 0.6f, 0.6f);
        gl.Translate(0.9f * side, -1.05f, -0.3f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, 0.15f, 7 + (4 * detail), 1 + (2 * detail));
        gl.Cylinder(0.15f, 0.15f, 1.25f, 7 + (5 * detail), 1 + (2 * detail));
        gl.Translate(0, 0, 1.25f);
        gl.Disk(0, 0.15f, 7 + (4 * detail), 1 + (2 * detail));
        gl.PopMatrix();
    }
}
