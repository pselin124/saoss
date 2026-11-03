using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Tringu (ship 1): a pink oval body between two golden slanted wings, with two white ball-and-cannon
/// guns pointing forward [alus1, case 1].
/// </summary>
public static class TringuModel
{
    /// <summary>Records the whole ship [DRAW_ALUS_1].</summary>
    public static GlDisplayList Record(LegacyGl gl, SharedModels shared, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);
        gl.PushMatrix();
        gl.Translate(0, -0.5f, 0);

        Wing(gl, shared, -0.7f, 60);
        Wing(gl, shared, 0.7f, -60);

        gl.PushMatrix();
        gl.Color(1, 0.5f, 1);
        gl.Translate(0, 0, 0.5f);
        gl.Scale(1.5f, 1, 1);
        gl.Sphere(0.5f, 8 + (detail * 15), 5 + (detail * 10));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(1, 1, 1);
        gl.Translate(0, 0, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Translate(-1, 1, 0);
        gl.Sphere(0.5f, 6 + (detail * 10), 5 + (detail * 7));
        gl.Cylinder(0.2f, 0.1f, 2, 5 + (detail * 5), 1 + (detail * 2));
        gl.Translate(1 * 2, 0, 0);
        gl.Sphere(0.5f, 6 + (detail * 10), 5 + (detail * 7));
        gl.Cylinder(0.2f, 0.1f, 2, 5 + (detail * 5), 1 + (detail * 2));
        gl.PopMatrix();

        gl.PopMatrix();
        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.0f * 0.7f);
        c.Gl.CallList(c.Models.TringuBody);
        c.Gl.PopMatrix();
    }

    private static void Wing(LegacyGl gl, SharedModels shared, float x, float degrees)
    {
        gl.PushMatrix();
        gl.Color(0.7f, 0.5f, 0.0f);
        gl.Translate(x, 0.5f, 0);
        gl.Rotate(degrees, 0, 0, 1);
        gl.Scale(1.5f, 0.2f, 0.7f);
        gl.CallList(shared.Cube);
        gl.PopMatrix();
    }
}
