using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Bulldog (ship 14): a tapering tan barrel with a ball at the back (red while thrusting) and a grey
/// spiky collar of nine thorns around it [alus14, case 14].
/// </summary>
public static class BulldogModel
{
    /// <summary>Records the barrel [DRAW_ALUS_14_OSA_1] and the rear ball with thorns [DRAW_ALUS_14_OSA_2].</summary>
    public static (GlDisplayList Barrel, GlDisplayList Rear) Record(LegacyGl gl, int detail)
    {
        var barrel = gl.NewList();
        ShipGlState.Solid(gl);
        gl.PushMatrix();
        gl.Color(0.7f, 0.6f, 0.5f);
        gl.Translate(0, -1, 0);
        gl.Rotate(-90.0f, 1.0f, 0.0f, 0.0f);
        gl.Cylinder(0.5f, 0.3f, 0.5f, 15 + (10 * detail), 1 + (2 * detail));
        gl.Translate(0, 0, 0.5f);
        gl.Color(0.7f, 0.63f, 0.56f);
        gl.Cylinder(0.3f, 0.15f, 1.5f, 15 + (10 * detail), 1 + (2 * detail));
        gl.PopMatrix();
        gl.EndList();

        var rear = gl.NewList();

        // Rear ball in the color chosen by the draw code.
        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        gl.Scale(0.5f, 0.3f, 0.5f);
        gl.Sphere(1, 15 + (10 * detail), 8 + (5 * detail));
        gl.PopMatrix();

        // Collar and thorns.
        gl.PushMatrix();
        gl.Color(0.8f, 0.8f, 0.8f);
        gl.Translate(0, -0.5f, 0);
        gl.Scale(0.4f, 0.2f, 0.4f);
        gl.Sphere(1, 12 + (8 * detail), 7 + (6 * detail));
        gl.Scale(1 / 0.4f, 1 / 0.2f, 1 / 0.4f);
        gl.Translate(0, -0.1f, 0);
        gl.Rotate(-25.0f, 0, 1, 0);
        for (var i = 0; i < 9; i++)
        {
            gl.Rotate(-20, 1, 0, 0);
            gl.Translate(0, 0, -0.2f);
            gl.Cylinder(0.1f, 0.001f, 1.25f, 6 + (6 * detail), 1 + (2 * detail));
            gl.Translate(0, 0, 0.2f);
            gl.Rotate(20, 1, 0, 0);
            gl.Rotate(-40.0f, 0, 1, 0);
        }

        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return (barrel, rear);
    }

    /// <summary>Draws the ship; the rear ball is red while thrusting.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius * 0.7f);
        gl.CallList(c.Models.BulldogBarrel);
        if (c.IsThrusting)
        {
            gl.Color(0.7f, 0.2f, 0.2f);
        }
        else
        {
            gl.Color(0.6f, 0.5f, 0.5f);
        }

        gl.CallList(c.Models.BulldogRear);
        gl.PopMatrix();
    }
}
