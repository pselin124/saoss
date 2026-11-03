using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Bloddu (ship 10): an orange-red egg on a short cylinder with a small knob on top and three
/// ball-footed legs below, like a little rocket on a tripod [alus10, case 10].
/// </summary>
public static class BlodduModel
{
    /// <summary>Records the whole ship [DRAW_ALUS_10]. Steps: egg body, capped base cylinder, knob, three legs.</summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);

        gl.PushMatrix();
        gl.Color(1, 0.15f, 0);
        gl.Scale(1, 2, 1);
        gl.Sphere(0.5f, 15 + (detail * 10), 5 + (8 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.5f, 0.5f, 1, 5 + (5 * detail), 1 + (2 * detail));
        gl.Rotate(-180, 1, 0, 0);
        gl.Disk(0, 0.5f, 5 + (5 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        // Knob.
        gl.PushMatrix();
        gl.Color(1, 0.05f, 0);
        gl.Translate(0, 1, 0);
        gl.Scale(0.4f, 0.4f, 0.4f);
        gl.Sphere(0.5f, 8 + (detail * 10), 5 + (5 * detail));
        gl.PopMatrix();

        // Legs.
        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        for (var i = 0; i < 3; i++)
        {
            gl.PushMatrix();
            gl.Rotate(i * 120, 0, 1, 0);
            gl.Translate(0.5f, 0, 0);
            gl.Rotate(30, 0, 0, 1);
            gl.Scale(0.33f, 1, 0.33f);
            gl.Sphere(0.5f, 7 + (detail * 6), 6 + (detail * 5));
            gl.Scale(3, 1, 3);
            gl.Translate(0, -0.5f, 0);
            gl.Rotate(-30, 0, 0, 1);
            gl.Sphere(0.2f, 6 + (detail * 6), 5 + (detail * 4));
            gl.PopMatrix();
        }

        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.1f * 0.7f);
        c.Gl.CallList(c.Models.BlodduBody);
        c.Gl.PopMatrix();
    }
}
