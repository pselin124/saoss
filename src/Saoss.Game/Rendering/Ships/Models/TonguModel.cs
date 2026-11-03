using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Tongu (ship 19): a flat purple disc with two translucent red eyes, two violet guns, three engine
/// nozzles at the back and a red tongue that sticks out and back in as the special recharges [alus19, case 19].
/// </summary>
public static class TonguModel
{
    /// <summary>Records the disc body, eyes and guns [DRAW_ALUS_19].</summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Texture2D);
        gl.Disable(GlCapability.Blend);

        gl.Color(0.3f, 0.1f, 0.8f);
        gl.PushMatrix();
        gl.Scale(1, 1, 0.5f);
        gl.Sphere(1, 20 + (15 * detail), 10 + (10 * detail));
        gl.PopMatrix();

        // Eyes.
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);
        gl.Color(0.6f, 0.2f, 0.2f, 0.6f);
        gl.PushMatrix();
        gl.Translate(0.35f, 0.65f, 0.20f);
        gl.Sphere(0.2f, 15 + (8 * detail), 8 + (6 * detail));
        gl.Translate(-0.35f * 2, 0, 0);
        gl.Sphere(0.2f, 15 + (8 * detail), 8 + (6 * detail));
        gl.PopMatrix();
        gl.Disable(GlCapability.Blend);

        // Guns.
        gl.PushMatrix();
        gl.Rotate(-90, 1, 0, 0);
        gl.Translate(-0.7f, 0, 0.35f);
        RecordGun(gl, detail);
        gl.Translate(0.7f * 2, 0, 0);
        RecordGun(gl, detail);
        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    /// <summary>
    /// Draws the ship. Steps: body list, the three engines (red exhaust discs while thrusting), and the tongue,
    /// whose length rises and falls over the special cooldown.
    /// </summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        var detail = c.Detail;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.0f * 0.7f);
        gl.CallList(c.Models.TonguBody);

        DrawEngine(c, 0);
        DrawEngine(c, 40.0f);
        DrawEngine(c, -40.0f);

        float cooldown = c.Ship.SpecialCooldown;
        float fullCooldown = c.Ship.Definition.Special.Cooldown;
        var tongue = cooldown < fullCooldown / 2 ? cooldown / 20 : (fullCooldown - cooldown) / 20;
        gl.PushMatrix();
        gl.Translate(0, 0.9f + (tongue / 5), 0);
        gl.Color(0.8f, 0.2f, 0.2f);
        gl.Scale(0.3f, tongue + 0.1f, 0.1f);
        gl.Sphere(1, 12 + (8 * detail), 4);
        gl.PopMatrix();

        gl.PopMatrix();
        ShipGlState.Overlay(gl);
    }

    private static void DrawEngine(ShipDrawContext c, float degrees)
    {
        var gl = c.Gl;
        var detail = c.Detail;
        gl.PushMatrix();
        if (degrees != 0)
        {
            gl.Rotate(degrees, 0.0f, 0.0f, 1.0f);
        }

        gl.Translate(0, -0.8f, 0);
        gl.Rotate(90.0f, 1.0f, 0.0f, 0.0f);
        gl.Color(0.3f, 0.1f, 0.8f);
        gl.Cylinder(0.2f, 0.3f, 0.6f, 15 + (8 * detail), 4 + (2 * detail));
        gl.Translate(0, 0, 0.6f);
        if (c.IsThrusting)
        {
            gl.Color(0.8f, 0.1f, 0.1f);
            gl.Disk(0, 0.3f, 15 + (8 * detail), 2 + (1 * detail));
        }

        gl.PopMatrix();
    }

    private static void RecordGun(LegacyGl gl, int detail)
    {
        gl.Color(0.4f, 0.3f, 1);
        gl.Sphere(0.33f, 15 + (8 * detail), 8 + (8 * detail));
        gl.Color(0.7f, 0.5f, 1);
        gl.Cylinder(0.13f, 0.07f, 1, 5 + (5 * detail), 1 + (2 * detail));
    }
}
