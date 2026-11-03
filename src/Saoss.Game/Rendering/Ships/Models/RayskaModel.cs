using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Rayska (ship 8): a pale ray-like creature with a pink tongue in front, two blue eyes, green spots
/// along both sides and a flared tail funnel that shows a red exhaust disc while thrusting [alus8, case 8].
/// </summary>
public static class RayskaModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_8]. Steps: body, tongue, eyes, spots on both sides, tail funnel.
    /// The list leaves the tail frame pushed so the draw code can put the exhaust disc at its end.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);
        gl.Translate(0, 0.6f, 0);

        gl.PushMatrix();
        gl.Color(0.8f, 0.73f, 0.7f);
        gl.Scale(1, 1.25f, 0.5f);
        gl.Sphere(1.0f, 12 + (10 * detail), 6 + (5 * detail));
        gl.PopMatrix();

        // Tongue.
        gl.PushMatrix();
        gl.Color(1, 0.17f, 0.63f);
        gl.Translate(0, 1.25f, 0);
        gl.Scale(1, 2.3f, 0.5f);
        gl.Sphere(0.3f, 10 + (8 * detail), 5 + (8 * detail));
        gl.PopMatrix();

        RecordEye(gl, detail, 1);
        RecordEye(gl, detail, -1);
        RecordSpots(gl, detail, 1);
        RecordSpots(gl, detail, -1);

        // Tail funnel, left pushed.
        gl.PushMatrix();
        gl.Color(0.8f, 0.73f, 0.7f);
        gl.Translate(0, -0.9f, 0);
        gl.Rotate(90, 1, 0, 0);
        gl.Scale(1.5f, 0.6f, 1);
        gl.Cylinder(0.35f, 0.5f, 1.7f, 8 + (7 * detail), 2 + (2 * detail));
        gl.Translate(0, 0, 1.7f);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship and its exhaust disc, then pops the tail frame left by the list.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.1f * 0.5f);
        gl.CallList(c.Models.RayskaBody);
        if (c.IsThrusting)
        {
            gl.Color(0.8f, 0.1f, 0.1f);
            gl.Disk(0, 0.5f, 8 + (7 * c.Detail), 2 + (2 * c.Detail));
        }

        gl.PopMatrix();
        gl.PopMatrix();
        ShipGlState.Overlay(gl);
    }

    // Eye with pupil; side is +1 for the right, -1 for the left.
    private static void RecordEye(LegacyGl gl, int detail, float side)
    {
        gl.PushMatrix();
        gl.Color(0.1f, 0.1f, 0.8f);
        gl.Translate(0.55f * side, 0.7f, 0.25f);
        gl.Sphere(0.2f, 10 + (10 * detail), 5 + (5 * detail));
        gl.Color(0.05f, 0.05f, 0.3f);
        gl.Translate(0.05f * side, 0.1f, 0.10f);
        gl.Sphere(0.08f, 8 + (5 * detail), 5 + (3 * detail));
        gl.PopMatrix();
    }

    private static void RecordSpots(LegacyGl gl, int detail, float side)
    {
        gl.PushMatrix();
        gl.Color(0.1f, 0.7f, 0.1f);
        gl.Translate(0.6f * side, 0, 0.35f);
        gl.Sphere(0.1f, 8 + (5 * detail), 5 + (5 * detail));
        gl.Translate(-0.1f * side, -0.4f, 0);
        gl.Sphere(0.1f, 8 + (5 * detail), 5 + (5 * detail));
        gl.Translate(-0.25f * side, -0.28f, 0);
        gl.Sphere(0.1f, 8 + (5 * detail), 5 + (5 * detail));
        gl.PopMatrix();
    }
}
