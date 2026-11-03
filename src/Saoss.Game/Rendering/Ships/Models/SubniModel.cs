using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Subni (ship 3): a horizontal grey-pink pole with a red diamond blade in the middle, two blade discs
/// behind it and a gun arm with a short barrel at each end [alus3, case 3].
/// </summary>
public static class SubniModel
{
    /// <summary>
    /// Records the whole ship [DRAW_ALUS_3]. Steps: capped pole, blade with its two discs, gun arms, gun barrels.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        ShipGlState.Solid(gl);
        gl.Translate(0, -0.5f, 0);

        // Pole with capped ends.
        gl.PushMatrix();
        gl.Color(0.6f, 0.50f, 0.5f);
        gl.Rotate(90, 0, 1, 0);
        gl.Translate(0, 0, -1.5f);
        gl.Cylinder(0.3f, 0.3f, 3, 7 + (5 * detail), 1 + (1 * detail));
        gl.Rotate(180, 1, 0, 0);
        gl.Disk(0, 0.3f, 7 + (5 * detail), 1 + (1 * detail));
        gl.Rotate(-90, 1, 0, 0);
        gl.Translate(0, 3, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, 0.3f, 7 + (5 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        // Blade.
        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f);
        gl.Scale(0.5f, 1.2f, 0.15f);
        gl.Sphere(1, 4, 5 + (8 * detail));
        gl.Scale(1f / 0.5f, 1f / 1.2f, 1f / 0.15f);
        gl.Translate(0, -0.7f, 0);
        gl.Color(0.6f, 0.4f, 0.4f);
        gl.Rotate(10, 0, 1, 0);
        gl.Disk(0, 1.0f, 10 + (5 * detail), 1 + (1 * detail));
        gl.Rotate(-20, 0, 1, 0);
        gl.Disk(0, 1.0f, 10 + (5 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        // Gun arms.
        gl.PushMatrix();
        gl.Color(0.55f, 0.5f, 0.5f);
        gl.Translate(1.2f, 0.5f, 0);
        gl.Scale(1, 2.5f, 1);
        gl.Sphere(0.25f, 5 + (8 * detail), 4 + (8 * detail));
        gl.Translate(-2.4f, 0, 0);
        gl.Sphere(0.25f, 5 + (8 * detail), 4 + (8 * detail));
        gl.PopMatrix();

        // Gun barrels.
        gl.PushMatrix();
        gl.Color(0.55f, 0.5f, 0.5f);
        gl.Translate(1.2f, 0.75f, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.15f, 0.12f, 1, 5 + (5 * detail), 1 + (1 * detail));
        gl.Translate(-2.4f, 0, 0);
        gl.Cylinder(0.15f, 0.12f, 1, 5 + (5 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        ShipGlState.Overlay(gl);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.7f);
        c.Gl.CallList(c.Models.SubniBody);
        c.Gl.PopMatrix();
    }
}
