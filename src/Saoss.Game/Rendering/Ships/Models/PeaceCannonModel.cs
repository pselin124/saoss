using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Peace Cannon (ship 9): a green ball on a green wedge with two small cubes at the back and a fat
/// white barrel pointing forward [alus9, case 9].
/// </summary>
public static class PeaceCannonModel
{
    /// <summary>Records the whole ship [DRAW_ALUS_9]; the cubes and wedge reuse the color left by the previous draw.</summary>
    public static GlDisplayList Record(LegacyGl gl, SharedModels shared, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);
        gl.Translate(0, -1.2f, 0);

        gl.PushMatrix();
        gl.Color(0.3f, 0.7f, 0.1f);
        gl.Translate(-0.9f, 0, 0);
        gl.Rotate(90, 0, 0, 1);
        gl.Scale(0.4f, 0.4f, 0.4f);
        gl.CallList(shared.Cube);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(0.9f, 0, 0);
        gl.Rotate(90, 0, 0, 1);
        gl.Scale(0.4f, 0.4f, 0.4f);
        gl.CallList(shared.Cube);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(0, 1.4f, 0);
        gl.Scale(1.3f, 1, 0.4f);
        gl.CallList(shared.Wedge);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.4f, 0.7f, 0.2f);
        gl.Translate(0, 1.3f, 0);
        gl.Scale(0.85f, 0.85f, 0.85f);
        gl.Sphere(1.0f, 15 + (10 * detail), 10 + (10 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(1, 1, 1);
        gl.Translate(0, 1.4f, 0.2f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.3f, 0.3f, 2, 5 + (detail * 5), 1 + (detail * 2));
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
        c.Gl.CallList(c.Models.PeaceCannonBody);
        c.Gl.PopMatrix();
    }
}
