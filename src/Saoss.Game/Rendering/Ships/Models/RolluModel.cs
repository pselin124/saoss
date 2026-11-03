using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Rollu (ship 4): a pale propeller plane seen from above: a short mast carrying a spinning three-blade
/// propeller, a flat wing with two yellow engine cones, a pink fan-shaped hull, two white struts,
/// a pale oval cockpit and three blue lights. Red exhaust discs glow while thrusting [alus4, case 4].
/// </summary>
public static class RolluModel
{
    /// <summary>
    /// Records the mast [DRAW_ALUS_4_OSA_1] and the rest of the plane [DRAW_ALUS_4_OSA_2]. The mast list
    /// leaves the propeller frame pushed; the body list pops it.
    /// </summary>
    public static (GlDisplayList Mast, GlDisplayList Body) Record(LegacyGl gl, SharedModels shared, int detail)
    {
        var mast = gl.NewList();
        ShipGlState.Solid(gl);
        gl.Translate(0, -0.4f, 0);
        gl.Rotate(90, 1, 0, 0);
        gl.PushMatrix();
        gl.Color(0.8f, 0.8f, 1);
        gl.Cylinder(0.1f, 0.1f, 1, 5 + (8 * detail), 2 + (2 * detail));
        gl.PushMatrix();
        gl.Translate(0, 0, 1);
        gl.Disk(0, 0.1f, 5 + (8 * detail), 2 + (2 * detail));
        gl.Translate(0, 0, -1);
        gl.Rotate(180, 0, 1, 0);
        gl.Disk(0, 0.1f, 5 + (8 * detail), 2 + (2 * detail));
        gl.PopMatrix();
        gl.Color(1, 1, 1);
        gl.EndList();

        var body = gl.NewList();
        gl.PopMatrix();
        RecordWingAndEngines(gl, shared, detail);
        RecordHull(gl, detail);
        ShipGlState.Overlay(gl);
        gl.EndList();
        return (mast, body);
    }

    /// <summary>
    /// Draws the ship. Steps: mast, three propeller blades turned by <c>PropellerAngle</c>, the two exhaust
    /// discs when thrusting, then the body.
    /// </summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        var slices = 5 + (8 * c.Detail);
        var loops = 2 + (2 * c.Detail);
        gl.PushMatrix();
        gl.Scale(c.Radius / 1.0f * 0.6f, c.Radius / 1.0f * 0.6f, c.Radius / 1.0f * 0.7f);
        gl.Rotate(c.Angle - 90f, 0, 0, 1);
        gl.CallList(c.Models.RolluMast);

        for (var i = 0; i < 3; i++)
        {
            gl.PushMatrix();
            gl.Rotate(c.Ship.PropellerAngle + (i * 120), 0, 0, 1);
            gl.Rotate(5, 0, 1, 0);
            gl.Scale(0.02f, 0.3f, 0.2f);
            gl.Translate(0, 1, 3.5f);
            gl.CallList(c.Models.Shared.Cube);
            gl.PopMatrix();
        }

        gl.PushMatrix();
        if (c.IsThrusting)
        {
            gl.Translate(1, 0, 0.1f);
            gl.Color(0.9f, 0.1f, 0.1f);
            gl.Translate(0, 0, 0.7f);
            gl.Disk(0, 0.3f, slices, loops);
            gl.Translate(0, 0, -0.7f);
            gl.Translate(-2, 0, 0);
            gl.Color(0.9f, 0.1f, 0.1f);
            gl.Translate(0, 0, 0.7f);
            gl.Disk(0, 0.3f, slices, loops);
        }

        gl.PopMatrix();

        gl.CallList(c.Models.RolluBody);
        gl.PopMatrix();
    }

    private static void RecordWingAndEngines(LegacyGl gl, SharedModels shared, int detail)
    {
        gl.PushMatrix();
        gl.Color(0.8f, 0.8f, 1);
        gl.Scale(1.5f, 0.2f, 0.03f);
        gl.Translate(0, 0, 3);
        gl.CallList(shared.Cube);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(1, 1, 0);
        gl.Translate(1, 0, 0.1f);
        gl.Cylinder(0.1f, 0.3f, 0.7f, 5 + (8 * detail), 2 + (2 * detail));
        gl.Translate(-2, 0, 0);
        gl.Color(1, 1, 0);
        gl.Cylinder(0.1f, 0.3f, 0.7f, 5 + (8 * detail), 2 + (2 * detail));
        gl.PopMatrix();
    }

    private static void RecordHull(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        gl.Scale(1, 0.1f, 1);
        gl.Color(1, 0.7f, 1);
        gl.Translate(0, 0, -1.4f);
        gl.Cylinder(0.8f, 1.2f, 1, 5, 2);
        gl.PopMatrix();

        // Struts.
        gl.PushMatrix();
        gl.Translate(0, 0, -2);
        gl.Color(1, 1, 1);
        gl.PushMatrix();
        gl.Translate(0.6f, 0, 0);
        gl.Rotate(20, 0, 1, 0);
        gl.Scale(0.7f, 1, 1);
        gl.Cylinder(0.2f, 0.2f, 2.2f, 5 + (8 * detail), 2 + (2 * detail));
        gl.PopMatrix();
        gl.Translate(-0.6f, 0, 0);
        gl.Rotate(-20, 0, 1, 0);
        gl.Scale(0.7f, 1, 1);
        gl.Cylinder(0.2f, 0.2f, 2.2f, 5 + (8 * detail), 2 + (2 * detail));
        gl.PopMatrix();

        // Cockpit.
        gl.PushMatrix();
        gl.Color(1, 0.9f, 1);
        gl.Translate(0, 0, -1.9f);
        gl.Scale(1.7f, 0.6f, 0.3f);
        gl.Sphere(0.5f, 10 + (15 * detail), 5 + (8 * detail));
        gl.PopMatrix();

        // Lights.
        gl.PushMatrix();
        gl.Color(0, 0, 1);
        gl.Translate(0.3f, 0, -0.8f);
        gl.Sphere(0.2f, 10 + (10 * detail), 5 + (8 * detail));
        gl.Translate(-0.6f, 0, 0);
        gl.Sphere(0.2f, 10 + (10 * detail), 5 + (8 * detail));
        gl.Translate(0.3f, 0, -0.2f);
        gl.Sphere(0.2f, 10 + (10 * detail), 5 + (8 * detail));
        gl.PopMatrix();
    }
}
