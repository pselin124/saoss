using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Wasp (ship 2): a red cylinder body striped yellow and purple, flat pink wings, a pale head with two
/// blue eyes and a brown tail spike [alus2, case 2].
/// </summary>
public static class WaspModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_2]. Steps: body cylinder, yellow and purple stripes, two flat wings,
    /// head, eyes, rear cap and tail.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);

        // Body.
        gl.PushMatrix();
        gl.Color(1, 0.1f, 0.1f);
        gl.Translate(0, -1, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.5f, 0.5f, 2, 10 + (10 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        RecordStripes(gl, detail);

        // Wings.
        gl.PushMatrix();
        gl.Color(1, 0.15f, 0.5f);
        gl.Scale(2, 1, 0.05f);
        gl.Sphere(1, 10 + (8 * detail), 5 + (5 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(1, 0.15f, 0.9f);
        gl.Scale(1.4f, 0.5f, 0.1f);
        gl.Sphere(1, 4, 4);
        gl.PopMatrix();

        // Head.
        gl.PushMatrix();
        gl.Color(1, 0.8f, 0.6f);
        gl.Translate(0, 1, 0);
        gl.Scale(1, 0.75f, 1);
        gl.Sphere(0.5f, 10 + (10 * detail), 5 + (10 * detail));
        gl.PopMatrix();

        // Eyes; the first one ignores the detail level in the original.
        gl.PushMatrix();
        gl.Color(0.2f, 0.2f, 0.7f);
        gl.Translate(0.2f, 1.15f, 0.2f);
        gl.Sphere(0.2f, 10, 5);
        gl.Translate(-0.4f, 0, 0);
        gl.Sphere(0.2f, 10 + (10 * detail), 5 + (5 * detail));
        gl.PopMatrix();

        // Rear.
        gl.PushMatrix();
        gl.Color(1, 0.5f, 0.2f);
        gl.Translate(0, -1, 0);
        gl.Scale(1, 0.3f, 1);
        gl.Sphere(0.5f, 13 + (10 * detail), 5 + (10 * detail));
        gl.PopMatrix();

        // Tail.
        gl.Color(0.6f, 0.4f, 0.2f);
        gl.PushMatrix();
        gl.Translate(0, -0.7f, 0);
        gl.Scale(0.2f, 1, 0.2f);
        gl.Sphere(1, 6 + (8 * detail), 8 + (8 * detail));
        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        gl.Scale(c.Radius / 1.0f * 0.6f, c.Radius / 1.0f * 0.6f, c.Radius / 1.0f * 0.7f);
        gl.Rotate(c.Angle - 90f, 0, 0, 1);
        gl.CallList(c.Models.WaspBody);
        gl.PopMatrix();
    }

    private static void RecordStripes(LegacyGl gl, int detail)
    {
        var slices = 10 + (10 * detail);
        var stacks = 1 + (2 * detail);
        gl.PushMatrix();
        gl.Color(1, 0.9f, 0.1f);
        gl.Translate(0, -0.8f, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.52f, 0.52f, 0.2f, slices, stacks);
        gl.Translate(0, 0, 0.8f);
        gl.Cylinder(0.52f, 0.52f, 0.2f, slices, stacks);
        gl.Translate(0, 0, 0.8f);
        gl.Cylinder(0.52f, 0.52f, 0.1f, slices, stacks);
        gl.Color(1, 0.15f, 1);
        gl.Translate(0, 0, -0.2f);
        gl.Cylinder(0.52f, 0.52f, 0.2f, slices, stacks);
        gl.Translate(0, 0, -0.8f);
        gl.Cylinder(0.52f, 0.52f, 0.2f, slices, stacks);
        gl.Translate(0, 0, -0.7f);
        gl.Cylinder(0.52f, 0.52f, 0.1f, slices, stacks);
        gl.PopMatrix();
    }
}
