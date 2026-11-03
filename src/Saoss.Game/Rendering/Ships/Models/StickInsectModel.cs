using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Stick Insect (ship 21): a thin capped brown stick with two antennae ending in red balls and four flat
/// green leaves along its sides [alus21, case 21].
/// </summary>
public static class StickInsectModel
{
    /// <summary>
    /// Records the colored ship [DRAW_ALUS_21_NORMAL] and the same shape without colors or state changes
    /// [DRAW_ALUS_21_SHADOW], which the intro scene used for shadows.
    /// </summary>
    public static (GlDisplayList Body, GlDisplayList Shadow) Record(LegacyGl gl, int detail)
    {
        var body = gl.NewList();
        ShipGlState.Solid(gl);
        RecordShape(gl, detail, colored: true);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
        gl.EndList();

        var shadow = gl.NewList();
        RecordShape(gl, detail, colored: false);
        gl.EndList();
        return (body, shadow);
    }

    /// <summary>Draws the ship. Matches use the colored list; the shadow list was only drawn by the intro.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.1f * 0.7f);
        c.Gl.CallList(c.Models.StickInsectBody);
        c.Gl.PopMatrix();
    }

    /// <summary>The shape shared by both lists: stick, its end caps, two antennae, four leaves.</summary>
    private static void RecordShape(LegacyGl gl, int detail, bool colored)
    {
        void SetColor(float r, float g, float b)
        {
            if (colored)
            {
                gl.Color(r, g, b);
            }
        }

        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        SetColor(0.6f, 0.50f, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.15f, 0.15f, 2, 7 + (6 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        SetColor(0.6f, 0.50f, 0.5f);
        gl.Translate(0, -1, 0);
        gl.Rotate(90, 1, 0, 0);
        gl.Disk(0, 0.15f, 7 + (6 * detail), 1 + (1 * detail));
        gl.Rotate(-90, 1, 0, 0);
        gl.Translate(0, 2, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, 0.15f, 7 + (6 * detail), 1 + (1 * detail));
        gl.PopMatrix();

        // Antennae: the first tilts around the diagonal (1, 0, 1), the second around X then Z.
        gl.PushMatrix();
        gl.Translate(-0.05f, 1, 0);
        SetColor(0.6f, 0.50f, 0.5f);
        gl.Rotate(30, 1, 0, 1);
        RecordAntenna(gl, detail, SetColor);
        gl.PopMatrix();

        gl.PushMatrix();
        SetColor(0.6f, 0.50f, 0.5f);
        gl.Translate(0.05f, 1, 0);
        gl.Rotate(30, 1, 0, 0);
        gl.Rotate(-30, 0, 0, 1);
        RecordAntenna(gl, detail, SetColor);
        gl.PopMatrix();

        // Leaves.
        gl.PushMatrix();
        gl.Translate(0.66f, 0.25f, 0);
        SetColor(0.2f, 0.8f, 0.2f);
        gl.Scale(1.5f, 1, 0.15f);
        gl.Sphere(0.3f, 10 + (8 * detail), 6 + (8 * detail));
        gl.Translate(0, -1, 0);
        gl.Sphere(0.3f, 10 + (8 * detail), 6 + (8 * detail));
        gl.Translate(-0.88f, 0.5f, 0);
        gl.Sphere(0.3f, 10 + (8 * detail), 6 + (8 * detail));
        gl.Translate(0, 1, 0);
        gl.Sphere(0.3f, 10 + (8 * detail), 6 + (8 * detail));
        gl.PopMatrix();
    }

    private static void RecordAntenna(LegacyGl gl, int detail, Action<float, float, float> setColor)
    {
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.05f, 0.05f, 0.5f, 5 + (4 * detail), 1 + (1 * detail));
        gl.Translate(0.0f, 0.0f, 0.6f);
        setColor(0.7f, 0.20f, 0.2f);
        gl.Sphere(0.2f, 10 + (6 * detail), 6 + (5 * detail));
    }
}
