using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Ufo (ship 6): a blue flying saucer with a dome and a white cannon on top that turns independently
/// toward the gun direction [alus6, case 6].
/// </summary>
public static class UfoModel
{
    /// <summary>
    /// Records the saucer [DRAW_ALUS_6_OSA_1] and the cannon [DRAW_ALUS_6_OSA_2]. The saucer list leaves the
    /// cannon frame pushed; the cannon list pops it.
    /// </summary>
    public static (GlDisplayList Saucer, GlDisplayList Cannon) Record(LegacyGl gl, int detail)
    {
        var saucer = gl.NewList();
        ShipGlState.Solid(gl);
        gl.PushMatrix();
        gl.Color(0, 0, 1);
        gl.Scale(1.5f, 1.5f, 0.25f);
        gl.Sphere(1.0f, 14 + (12 * detail), 12 + (12 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0, 0.2f, 1);
        gl.Sphere(1.0f, 12 + (10 * detail), 8 + (8 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(1, 1, 1);
        gl.Translate(0, 0, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.EndList();

        var cannon = gl.NewList();
        gl.Cylinder(0.2f, 0.1f, 2, 5 + (5 * detail), 2 + (2 * detail));
        gl.PopMatrix();
        ShipGlState.Overlay(gl);
        gl.EndList();
        return (saucer, cannon);
    }

    /// <summary>Draws the ship, turning the cannon by the difference between heading and gun direction.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.0f * 0.7f);
        gl.CallList(c.Models.UfoSaucer);
        gl.Rotate(c.Angle - c.Ship.WeaponAngle, 0, 1, 0);
        gl.CallList(c.Models.UfoCannon);
        gl.PopMatrix();
    }
}
