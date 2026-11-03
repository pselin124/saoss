using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Elephant (ship 16): a green triangular face with two big blue eyes on red stalks and a dark red trunk
/// at the back that swings up and down [alus16, case 16].
/// </summary>
public static class ElephantModel
{
    private const float StalkRadius = 0.15f;
    private const float EyeX = 0.7f;
    private const float StalkLength = 1f;
    private const float EyeRadius = 0.5f;

    /// <summary>
    /// Records the head [DRAW_ALUS_16_OSA_1] and the trunk [DRAW_ALUS_16_OSA_2]. The head list leaves the trunk
    /// frame pushed with the trunk color set; the trunk list pops it.
    /// </summary>
    public static (GlDisplayList Head, GlDisplayList Trunk) Record(LegacyGl gl, int detail)
    {
        var head = gl.NewList();
        RecordHead(gl, detail);
        gl.EndList();

        var trunk = gl.NewList();
        gl.Scale(1, 3, 0.4f);
        gl.Sphere(0.3f, 8 + (8 * detail), 5 + (5 * detail));
        gl.PopMatrix();
        gl.EndList();
        return (head, trunk);
    }

    /// <summary>Draws the ship; the trunk swings with the sine of <c>PropellerAngle</c>.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.1f * 0.7f);
        gl.CallList(c.Models.ElephantHead);
        gl.Rotate(MathF.Sin(c.Ship.PropellerAngle / 50.0f) * 35, 1, 0, 0);
        gl.CallList(c.Models.ElephantTrunk);
        gl.PopMatrix();
        ShipGlState.Overlay(gl);
    }

    /// <summary>
    /// The head list: triangular face, the eye stalks (the right one is drawn twice in the original), the eyes,
    /// then the trunk frame left pushed.
    /// </summary>
    private static void RecordHead(LegacyGl gl, int detail)
    {
        ShipGlState.Solid(gl);
        gl.Color(0.1f, 0.8f, 0.1f);
        gl.PushMatrix();
        gl.Translate(0, -0.25f, 0);
        gl.Rotate(180, 0, 1, 0);
        gl.Rotate(180, 1, 0, 0);
        gl.Scale(1.5f, 1, 0.5f);
        gl.Sphere(1, 3, 25 + (detail * 15));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f);
        gl.Translate(EyeX, 0, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(StalkRadius, StalkRadius, StalkLength, 10 + (10 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        gl.Color(0.8f, 0.1f, 0.1f);
        RecordStalk(gl, detail, EyeX);
        RecordStalk(gl, detail, -EyeX);

        gl.Color(0.1f, 0.1f, 0.8f);
        RecordEye(gl, detail, EyeX);
        RecordEye(gl, detail, -EyeX);

        gl.Color(0.6f, 0.1f, 0.1f);
        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        gl.Rotate(90, 0, 1, 0);
    }

    private static void RecordStalk(LegacyGl gl, int detail, float x)
    {
        gl.PushMatrix();
        gl.Translate(x, 0, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(StalkRadius, StalkRadius, StalkLength, 10 + (10 * detail), 1 + (2 * detail));
        gl.PopMatrix();
    }

    private static void RecordEye(LegacyGl gl, int detail, float x)
    {
        gl.PushMatrix();
        gl.Translate(x, StalkRadius + 1, 0);
        gl.Sphere(EyeRadius, 20 + (18 * detail), 16 + (15 * detail));
        gl.PopMatrix();
    }
}
