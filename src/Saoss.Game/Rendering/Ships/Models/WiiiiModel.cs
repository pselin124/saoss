using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Wiiii (ship 15): a grey tower of stacked discs inside a translucent red tube, with a red engine ball
/// at the bottom and a side barrel. While boosting (shield flag) it turns into a textured spinning top
/// with a grey spike [alus15, case 15].
/// </summary>
public static class WiiiiModel
{
    /// <summary>
    /// Records the base [DRAW_ALUS_15_OSA_1], the tower [DRAW_ALUS_15_OSA_2] and the spinning-top form
    /// [DRAW_ALUS_15_OSA_3]. The base list leaves the engine ball frame pushed; the tower list pops it.
    /// </summary>
    public static (GlDisplayList Base, GlDisplayList Tower, GlDisplayList Top) Record(LegacyGl gl, int detail)
    {
        var baseList = gl.NewList();
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);
        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        gl.Color(0.5f, 0.5f, 0.5f, 1.0f);
        gl.Rotate(90, 1, 0, 0);
        gl.Cylinder(0.5f, 0.5f, 0.5f, 20 + (15 * detail), 1 + (2 * detail));
        gl.PopMatrix();
        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f, 0.8f);
        gl.Translate(0, -1.35f, 0);
        gl.EndList();

        var tower = gl.NewList();
        RecordTower(gl, detail);
        gl.EndList();

        var top = gl.NewList();
        RecordTop(gl, detail);
        gl.EndList();
        return (baseList, tower, top);
    }

    /// <summary>
    /// Draws the ship: normally the base and the tower, whose engine ball is squashed when not thrusting;
    /// with the shield on, the textured spinning top instead.
    /// </summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.1f * 0.65f);
        if (!c.Ship.IsShieldActive)
        {
            gl.CallList(c.Models.WiiiiBase);
            if (c.IsThrusting)
            {
                gl.Scale(1, 1, 1);
            }
            else
            {
                gl.Scale(1, 0.5f, 1);
            }

            gl.CallList(c.Models.WiiiiTower);
        }
        else
        {
            gl.BindTexture(c.Textures.Tottero);
            gl.CallList(c.Models.WiiiiTop);
        }

        gl.PopMatrix();
    }

    /// <summary>
    /// The tower list: engine ball (popping the base frame), five stacked discs, the side barrel with two red
    /// balls, the translucent red tube and its red dome.
    /// </summary>
    private static void RecordTower(LegacyGl gl, int detail)
    {
        gl.Sphere(0.48f, 20 + (15 * detail), 10 + (5 * detail));
        gl.PopMatrix();

        RecordDisc(gl, detail, (0.5f, 0.5f, 0.5f), -1.0f, 0.8f);
        RecordDisc(gl, detail, (0.6f, 0.6f, 0.6f), -0.6f, 0.8f);
        RecordDisc(gl, detail, (0.6f, 0.6f, 0.6f), -0.2f, 0.8f);
        RecordDisc(gl, detail, (0.6f, 0.6f, 0.6f), 0.2f, 0.8f);
        RecordDisc(gl, detail, (0.6f, 0.6f, 0.6f), 0.6f, 1.05f);

        // Barrel.
        gl.PushMatrix();
        gl.Translate(0.85f, 0.6f, 0);
        gl.Color(0.5f, 0.5f, 0.5f, 1.0f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.15f, 0.15f, 0.8f, 15 + (10 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f, 0.7f);
        gl.Translate(0.85f, 1.0f, 0);
        gl.Scale(1, 0.6f, 1);
        gl.Sphere(0.2f, 12 + (8 * detail), 8 + (6 * detail));
        gl.Translate(0, 0.4f, 0);
        gl.Sphere(0.2f, 12 + (8 * detail), 8 + (6 * detail));
        gl.PopMatrix();

        // Red tube and dome.
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);
        gl.PushMatrix();
        gl.Color(0.9f, 0.1f, 0.1f, 0.6f);
        gl.Translate(0, -1, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.7f, 0.7f, 1.5f, 7 + (8 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.9f, 0.1f, 0.1f, 0.6f);
        gl.Translate(0, 0.75f, 0);
        gl.Scale(0.9f, 0.5f, 0.9f);
        gl.Sphere(1, 20 + (15 * detail), 8 + (6 * detail));
        gl.PopMatrix();

        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
    }

    private static void RecordDisc(LegacyGl gl, int detail, (float R, float G, float B) color, float y, float radius)
    {
        gl.PushMatrix();
        gl.Color(color.R, color.G, color.B, 1.0f);
        gl.Translate(0, y, 0);
        gl.Scale(1, 0.2f, 1);
        gl.Sphere(radius, 20 + (15 * detail), 7 + (8 * detail));
        gl.PopMatrix();
    }

    // The quadric of this list had gluQuadricTexture on, so all three parts get texture coordinates.
    private static void RecordTop(LegacyGl gl, int detail)
    {
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);

        gl.PushMatrix();
        gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
        gl.Translate(0, 0.75f, 0);
        gl.Sphere(0.5f, 18 + (8 * detail), 12 + (6 * detail), textured: true);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0, 0.47f, 1.5f, 18 + (8 * detail), 1 + (2 * detail), textured: true);
        gl.PopMatrix();

        // Spike, untextured.
        gl.Disable(GlCapability.Texture2D);
        gl.Color(0.75f, 0.7f, 0.7f, 1.0f);
        gl.PushMatrix();
        gl.Translate(0, 1.1f, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.25f, 0, 0.6f, 8 + (8 * detail), 1 + (2 * detail), textured: true);
        gl.PopMatrix();

        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
    }
}
