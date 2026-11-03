using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Shapperi (ship 11): a grey triangular delta with a central barrel, a bluish nose pod and a gun turret
/// on each wing tip. Stretches long and thin while its shielded slow mode is on [alus11, case 11].
/// </summary>
public static class ShapperiModel
{
    /// <summary>Records the whole ship [DRAW_ALUS_11]. Steps: delta, barrel, barrel base, nose, two wing guns.</summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);

        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Scale(2, 1.33f, 0.4f);
        gl.Sphere(1, 3, 8 + (8 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.7f);
        gl.Rotate(-90.0f, 1.0f, 0.0f, 0.0f);
        gl.Cylinder(0.5f, 0.4f, 1, 8 + (8 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.7f);
        gl.Scale(1, 0.75f, 1);
        gl.Sphere(0.5f, 8 + (8 * detail), 5 + (5 * detail));
        gl.PopMatrix();

        // Nose.
        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.8f);
        gl.Translate(0, 1, 0);
        gl.Scale(0.5f, 1, 0.5f);
        gl.Sphere(0.8f, 8 + (7 * detail), 5 + (5 * detail));
        gl.PopMatrix();

        RecordGun(gl, detail, 1);
        RecordGun(gl, detail, -1);

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship, stretched while the shield is on.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius * 0.8f);
        gl.Translate(0, -0.2f, 0);
        if (c.Ship.IsShieldActive)
        {
            gl.Scale(0.7f, 1.3f, 0.8f);
        }

        gl.CallList(c.Models.ShapperiBody);
        gl.PopMatrix();
    }

    private static void RecordGun(LegacyGl gl, int detail, float x)
    {
        gl.PushMatrix();
        gl.Color(0.6f, 0.6f, 0.7f);
        gl.Translate(x, 0, 0);
        gl.Sphere(0.3f, 7 + (6 * detail), 5 + (5 * detail));
        gl.Rotate(-90.0f, 1.0f, 0.0f, 0.0f);
        gl.Color(0.7f, 0.7f, 0.7f);
        gl.Cylinder(0.15f, 0.1f, 0.6f, 5 + (5 * detail), 1 + (2 * detail));
        gl.PopMatrix();
    }
}
